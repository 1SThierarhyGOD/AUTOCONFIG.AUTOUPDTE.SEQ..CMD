// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Aspire.Dashboard.Components.ResourcesGridColumns;
using Aspire.Dashboard.Model;
using Aspire.Dashboard.Otlp.Model;
using Aspire.Dashboard.Otlp.Storage;
using Aspire.Dashboard.Resources;
using Aspire.Dashboard.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Aspire.Dashboard.Components.Pages;

public partial class Resources : ComponentBase, IAsyncDisposable, IPageWithSessionAndUrlState<Resources.ResourcesViewModel, Resources.ResourcesPageState>
{
    private Subscription? _logsSubscription;
    private Dictionary<OtlpApplication, int>? _applicationUnviewedErrorCounts;

    [Inject]
    public required IDashboardClient DashboardClient { get; init; }
    [Inject]
    public required TelemetryRepository TelemetryRepository { get; init; }
    [Inject]
    public required NavigationManager NavigationManager { get; init; }
    [Inject]
    public required IDialogService DialogService { get; init; }
    [Inject]
    public required IToastService ToastService { get; init; }
    [Inject]
    public required BrowserTimeProvider TimeProvider { get; init; }
    [Inject]
    public required IJSRuntime JS { get; init; }
    [Inject]
    public required ProtectedSessionStorage SessionStorage { get; init; }

    public string BasePath => DashboardUrls.ResourcesBasePath;
    public string SessionStorageKey => "Resources_PageState";
    public ResourcesViewModel PageViewModel { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "view")]
    public string? ViewKindName { get; set; }

    private ResourceViewModel? SelectedResource { get; set; }

    private readonly CancellationTokenSource _watchTaskCancellationTokenSource = new();
    private readonly ConcurrentDictionary<string, ResourceViewModel> _resourceByName = new(StringComparers.ResourceName);
    private readonly ConcurrentDictionary<string, bool> _allResourceTypes = [];
    private readonly ConcurrentDictionary<string, bool> _visibleResourceTypes;
    private string _filter = "";
    private bool _isTypeFilterVisible;
    private Task? _resourceSubscriptionTask;
    private bool _isLoading = true;
    private string? _elementIdBeforeDetailsViewOpened;
    private DotNetObjectReference<ResourcesInterop>? _resourcesInteropReference;

    public Resources()
    {
        _visibleResourceTypes = new(StringComparers.ResourceType);
    }

    private bool Filter(ResourceViewModel resource) => _visibleResourceTypes.ContainsKey(resource.ResourceType) && (_filter.Length == 0 || resource.MatchesFilter(_filter)) && resource.State != ResourceStates.HiddenState;

    protected async Task OnResourceTypeVisibilityChangedAsync(string resourceType, bool isVisible)
    {
        if (isVisible)
        {
            _visibleResourceTypes[resourceType] = true;
        }
        else
        {
            _visibleResourceTypes.TryRemove(resourceType, out _);
        }

        await UpdateResourceGraphResourcesAsync();
        await ClearSelectedResourceAsync();
    }

    private async Task HandleSearchFilterChangedAsync()
    {
        await UpdateResourceGraphResourcesAsync();
        await ClearSelectedResourceAsync();
    }

    private bool? AreAllTypesVisible
    {
        get
        {
            static bool SetEqualsKeys(ConcurrentDictionary<string, bool> left, ConcurrentDictionary<string, bool> right)
            {
                // PERF: This is inefficient since Keys locks and copies the keys.
                var keysLeft = left.Keys;
                var keysRight = right.Keys;

                return keysLeft.Count == keysRight.Count && keysLeft.SequenceEqual(keysRight, StringComparers.ResourceType);
            }

            return SetEqualsKeys(_visibleResourceTypes, _allResourceTypes)
                ? true
                : _visibleResourceTypes.IsEmpty
                    ? false
                    : null;
        }
        set
        {
            static bool UnionWithKeys(ConcurrentDictionary<string, bool> left, ConcurrentDictionary<string, bool> right)
            {
                // .Keys locks and copies the keys so avoid it here.
                foreach (var (key, _) in right)
                {
                    left[key] = true;
                }

                return true;
            }

            if (value is true)
            {
                UnionWithKeys(_visibleResourceTypes, _allResourceTypes);
            }
            else if (value is false)
            {
                _visibleResourceTypes.Clear();
            }

            _ = UpdateResourceGraphResourcesAsync();
        }
    }

    private bool HasResourcesWithCommands => _resourceByName.Any(r => r.Value.Commands.Any());

    private IQueryable<ResourceViewModel>? FilteredResources => _resourceByName.Values.Where(Filter).OrderBy(e => e.ResourceType).ThenBy(e => e.Name).AsQueryable();

    private readonly GridSort<ResourceViewModel> _nameSort = GridSort<ResourceViewModel>.ByAscending(p => p.Name);
    private readonly GridSort<ResourceViewModel> _stateSort = GridSort<ResourceViewModel>.ByAscending(p => p.State);
    private readonly GridSort<ResourceViewModel> _startTimeSort = GridSort<ResourceViewModel>.ByDescending(p => p.CreationTimeStamp);

    protected override async Task OnInitializedAsync()
    {
        PageViewModel = new ResourcesViewModel
        {
            SelectedViewKind = null
        };

        _applicationUnviewedErrorCounts = TelemetryRepository.GetApplicationUnviewedErrorLogsCount();

        if (DashboardClient.IsEnabled)
        {
            await SubscribeResourcesAsync();
        }

        _logsSubscription = TelemetryRepository.OnNewLogs(null, SubscriptionType.Other, async () =>
        {
            var newApplicationUnviewedErrorCounts = TelemetryRepository.GetApplicationUnviewedErrorLogsCount();

            // Only update UI if the error counts have changed.
            if (ApplicationErrorCountsChanged(newApplicationUnviewedErrorCounts))
            {
                _applicationUnviewedErrorCounts = newApplicationUnviewedErrorCounts;
                await InvokeAsync(StateHasChanged);
            }
        });

        _isLoading = false;

        async Task SubscribeResourcesAsync()
        {
            var (snapshot, subscription) = await DashboardClient.SubscribeResourcesAsync(_watchTaskCancellationTokenSource.Token);

            // Apply snapshot.
            foreach (var resource in snapshot)
            {
                var added = _resourceByName.TryAdd(resource.Name, resource);

                _allResourceTypes.TryAdd(resource.ResourceType, true);
                _visibleResourceTypes.TryAdd(resource.ResourceType, true);

                Debug.Assert(added, "Should not receive duplicate resources in initial snapshot data.");
            }

            // Listen for updates and apply.
            _resourceSubscriptionTask = Task.Run(async () =>
            {
                await foreach (var changes in subscription.WithCancellation(_watchTaskCancellationTokenSource.Token).ConfigureAwait(false))
                {
                    foreach (var (changeType, resource) in changes)
                    {
                        if (changeType == ResourceViewModelChangeType.Upsert)
                        {
                            _resourceByName[resource.Name] = resource;

                            _allResourceTypes[resource.ResourceType] = true;
                            _visibleResourceTypes[resource.ResourceType] = true;
                        }
                        else if (changeType == ResourceViewModelChangeType.Delete)
                        {
                            var removed = _resourceByName.TryRemove(resource.Name, out _);
                            Debug.Assert(removed, "Cannot remove unknown resource.");
                        }
                    }

                    await UpdateResourceGraphResourcesAsync();
                    await InvokeAsync(StateHasChanged);
                }
            });
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _resourcesInteropReference = DotNetObjectReference.Create(new ResourcesInterop(this));

            await JS.InvokeVoidAsync("initializeResourcesGraph", _resourcesInteropReference);
            await UpdateResourceGraphResourcesAsync();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await this.InitializeViewModelAsync();
    }

    private async Task UpdateResourceGraphResourcesAsync()
    {
        if (PageViewModel.SelectedViewKind != ResourceViewKind.Graph)
        {
            return;
        }

        var databaseIcon = GetIconPathData(new Icons.Filled.Size24.Database());
        var containerIcon = GetIconPathData(new Icons.Filled.Size24.Box());
        var executableIcon = GetIconPathData(new Icons.Filled.Size24.SettingsCogMultiple());
        var projectIcon = GetIconPathData(new Icons.Filled.Size24.CodeCircle());

        var activeResources = _resourceByName.Values.Where(Filter).ToList();
        var resources = activeResources.Select(MapDto).ToList();
        await JS.InvokeVoidAsync("updateResourcesGraph", resources);

        ResourceDto MapDto(ResourceViewModel r)
        {
            var referencedNames = new List<string>();
            if (r.Environment.SingleOrDefault(e => e.Name == "hack_resource_references") is { } references)
            {
                referencedNames = references.Value!.Split(',').ToList();
            }

            var resolvedNames = new List<string>();
            for (var i = 0; i < referencedNames.Count; i++)
            {
                var name = referencedNames[i];
                foreach (var targetResource in activeResources.Where(r => string.Equals(r.DisplayName, name, StringComparisons.ResourceName)))
                {
                    resolvedNames.Add(targetResource.Name);
                }
            }

            var endpoint = GetDisplayedEndpoints(r, out _).FirstOrDefault();
            var resolvedEndpointText = ResolvedEndpointText(endpoint);
            var resourceName = ResourceViewModel.GetResourceName(r, _resourceByName);
            var color = ColorGenerator.Instance.GetColorHexByKey(resourceName);

            var icon = r.ResourceType switch
            {
                KnownResourceTypes.Executable => executableIcon,
                KnownResourceTypes.Project => projectIcon,
                KnownResourceTypes.Container => containerIcon,
                "PostgresDatabaseResource" => databaseIcon,
                _ => executableIcon
            };

            var stateIcon = StateColumnDisplay.GetStateIcon(r, ColumnsLoc);

            var dto = new ResourceDto
            {
                Name = r.Name,
                ResourceType = r.ResourceType,
                DisplayName = ResourceViewModel.GetResourceName(r, _resourceByName),
                Uid = r.Uid,
                ResourceIcon = new IconDto
                {
                    Path = icon,
                    Color = color,
                    Tooltip = r.ResourceType
                },
                StateIcon = new IconDto
                {
                    Path = GetIconPathData(stateIcon.Icon),
                    Color = stateIcon.Color.ToAttributeValue()!,
                    Tooltip = stateIcon.Tooltip ?? r.State
                },
                ReferencedNames = resolvedNames.ToImmutableArray(),
                EndpointUrl = endpoint?.Url,
                EndpointText = resolvedEndpointText
            };

            return dto;
        }
    }

    private static string ResolvedEndpointText(DisplayedEndpoint? endpoint)
    {
        var text = endpoint?.Text ?? endpoint?.Url;
        if (string.IsNullOrEmpty(text))
        {
            return "No endpoints";
        }

        if (Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            return $"{uri.Host}:{uri.Port}";
        }

        return text;
    }

    private static string GetIconPathData(Icon icon)
    {
        var p = icon.Content;
        var e = XElement.Parse(p);
        return e.Attribute("d")!.Value;
    }

    private class ResourcesInterop(Resources resources)
    {
        [JSInvokable]
        public async Task SelectResource(string id)
        {
            if (resources._resourceByName.TryGetValue(id, out var resource))
            {
                await resources.InvokeAsync(async () =>
                {
                    await resources.ShowResourceDetailsAsync(resource, null!);
                    resources.StateHasChanged();
                });
            }
        }
    }

    private class ResourceDto
    {
        public required string Name { get; init; }
        public required string ResourceType { get; init; }
        public required string DisplayName { get; init; }
        public required string Uid { get; init; }
        public required IconDto ResourceIcon { get; init; }
        public required IconDto StateIcon { get; init; }
        public required string? EndpointUrl { get; init; }
        public required string? EndpointText { get; init; }
        public required ImmutableArray<string> ReferencedNames { get; init; }
    }

    private class IconDto
    {
        public required string Path { get; init; }
        public required string Color { get; init; }
        public required string? Tooltip { get; init; }
    }

    private bool ApplicationErrorCountsChanged(Dictionary<OtlpApplication, int> newApplicationUnviewedErrorCounts)
    {
        if (_applicationUnviewedErrorCounts == null || _applicationUnviewedErrorCounts.Count != newApplicationUnviewedErrorCounts.Count)
        {
            return true;
        }

        foreach (var (application, count) in newApplicationUnviewedErrorCounts)
        {
            if (!_applicationUnviewedErrorCounts.TryGetValue(application, out var oldCount) || oldCount != count)
            {
                return true;
            }
        }

        return false;
    }

    private async Task ShowResourceDetailsAsync(ResourceViewModel resource, string buttonId)
    {
        _elementIdBeforeDetailsViewOpened = buttonId;

        if (SelectedResource == resource)
        {
            await ClearSelectedResourceAsync();
        }
        else
        {
            SelectedResource = resource;
        }
    }

    private async Task ClearSelectedResourceAsync(bool causedByUserAction = false)
    {
        SelectedResource = null;

        if (PageViewModel.SelectedViewKind == ResourceViewKind.Graph)
        {
            await UpdateResourceGraphSelectedAsync();
        }

        if (_elementIdBeforeDetailsViewOpened is not null && causedByUserAction)
        {
            await JS.InvokeVoidAsync("focusElement", _elementIdBeforeDetailsViewOpened);
        }

        _elementIdBeforeDetailsViewOpened = null;
    }

    private string GetResourceName(ResourceViewModel resource) => ResourceViewModel.GetResourceName(resource, _resourceByName);

    private bool HasMultipleReplicas(ResourceViewModel resource)
    {
        var count = 0;
        foreach (var (_, item) in _resourceByName)
        {
            if (item.State == ResourceStates.HiddenState)
            {
                continue;
            }

            if (item.DisplayName == resource.DisplayName)
            {
                count++;
                if (count >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private string? GetRowClass(ResourceViewModel resource)
        => resource == SelectedResource ? "selected-row resource-row" : "resource-row";

    private async Task ExecuteResourceCommandAsync(ResourceViewModel resource, CommandViewModel command)
    {
        if (!string.IsNullOrWhiteSpace(command.ConfirmationMessage))
        {
            var dialogReference = await DialogService.ShowConfirmationAsync(command.ConfirmationMessage);
            var result = await dialogReference.Result;
            if (result.Cancelled)
            {
                return;
            }
        }

        var response = await DashboardClient.ExecuteResourceCommandAsync(resource.Name, resource.ResourceType, command, CancellationToken.None);

        if (response.Kind == ResourceCommandResponseKind.Succeeded)
        {
            ToastService.ShowSuccess(string.Format(CultureInfo.InvariantCulture, Loc[nameof(Dashboard.Resources.Resources.ResourceCommandSuccess)], command.DisplayName));
        }
        else
        {
            ToastService.ShowCommunicationToast(new ToastParameters<CommunicationToastContent>()
            {
                Intent = ToastIntent.Error,
                Title = string.Format(CultureInfo.InvariantCulture, Loc[nameof(Dashboard.Resources.Resources.ResourceCommandFailed)], command.DisplayName),
                PrimaryAction = Loc[nameof(Dashboard.Resources.Resources.ResourceCommandToastViewLogs)],
                OnPrimaryAction = EventCallback.Factory.Create<ToastResult>(this, () => NavigationManager.NavigateTo(DashboardUrls.ConsoleLogsUrl(resource: resource.Name))),
                Content = new CommunicationToastContent()
                {
                    Details = response.ErrorMessage
                }
            });
        }
    }

    private static (string Value, string? ContentAfterValue, string ValueToCopy, string Tooltip)? GetSourceColumnValueAndTooltip(ResourceViewModel resource)
    {
        // NOTE projects are also executables, so we have to check for projects first
        if (resource.IsProject() && resource.TryGetProjectPath(out var projectPath))
        {
            return (Value: Path.GetFileName(projectPath), ContentAfterValue: null, ValueToCopy: projectPath, Tooltip: projectPath);
        }

        if (resource.TryGetExecutablePath(out var executablePath))
        {
            resource.TryGetExecutableArguments(out var arguments);
            var argumentsString = arguments.IsDefaultOrEmpty ? "" : string.Join(" ", arguments);
            var fullCommandLine = $"{executablePath} {argumentsString}";

            return (Value: Path.GetFileName(executablePath), ContentAfterValue: argumentsString, ValueToCopy: fullCommandLine, Tooltip: fullCommandLine);
        }

        if (resource.TryGetContainerImage(out var containerImage))
        {
            return (Value: containerImage, ContentAfterValue: null, ValueToCopy: containerImage, Tooltip: containerImage);
        }

        if (resource.Properties.TryGetValue(KnownProperties.Resource.Source, out var value) && value.HasStringValue)
        {
            return (Value: value.StringValue, ContentAfterValue: null, ValueToCopy: value.StringValue, Tooltip: value.StringValue);
        }

        return null;
    }

    private string GetEndpointsTooltip(ResourceViewModel resource)
    {
        var displayedEndpoints = GetDisplayedEndpoints(resource, out var additionalMessage);

        if (additionalMessage is not null)
        {
            return additionalMessage;
        }

        if (displayedEndpoints.Count == 1)
        {
            return displayedEndpoints.First().Text;
        }

        var maxShownEndpoints = 3;
        var tooltipBuilder = new StringBuilder(string.Join(", ", displayedEndpoints.Take(maxShownEndpoints).Select(endpoint => endpoint.Text)));

        if (displayedEndpoints.Count > maxShownEndpoints)
        {
            tooltipBuilder.Append(CultureInfo.CurrentCulture, $" + {displayedEndpoints.Count - maxShownEndpoints}");
        }

        return tooltipBuilder.ToString();
    }

    private List<DisplayedEndpoint> GetDisplayedEndpoints(ResourceViewModel resource, out string? additionalMessage)
    {
        if (resource.Urls.Length == 0)
        {
            // If we have no endpoints, and the app isn't running anymore or we're not expecting any, then just say None
            additionalMessage = ColumnsLoc[nameof(Columns.EndpointsColumnDisplayNone)];
            return [];
        }

        additionalMessage = null;

        // Make sure that endpoints have a consistent ordering. Show https first, then everything else.
        return [.. GetEndpoints(resource)
            .OrderByDescending(e => e.Url?.StartsWith("https") == true)
            .ThenBy(e=> e.Url ?? e.Text)];
    }

    /// <summary>
    /// A resource has services and endpoints. These can overlap. This method attempts to return a single list without duplicates.
    /// </summary>
    private static List<DisplayedEndpoint> GetEndpoints(ResourceViewModel resource)
    {
        return ResourceEndpointHelpers.GetEndpoints(resource, includeInteralUrls: false);
    }

    private Task OnTabChangeAsync(FluentTab newTab)
    {
        var id = newTab.Id?.Substring("tab-".Length);

        if (id is null
            || !Enum.TryParse(typeof(ResourceViewKind), id, out var o)
            || o is not ResourceViewKind viewKind)
        {
            return Task.CompletedTask;
        }

        return OnViewChangedAsync(viewKind);
    }

    private async Task OnViewChangedAsync(ResourceViewKind newView)
    {
        PageViewModel.SelectedViewKind = newView;
        await this.AfterViewModelChangedAsync();

        if (newView == ResourceViewKind.Graph)
        {
            await UpdateResourceGraphResourcesAsync();
            await UpdateResourceGraphSelectedAsync();
        }
    }

    private async Task UpdateResourceGraphSelectedAsync()
    {
        await JS.InvokeVoidAsync("updateResourcesGraphSelected", SelectedResource?.Name);
    }

    public sealed class ResourcesViewModel
    {
        public required ResourceViewKind? SelectedViewKind { get; set; }
    }

    public class ResourcesPageState
    {
        public required string? ViewKind { get; set; }
    }

    public enum ResourceViewKind
    {
        Table,
        Graph
    }

    public async ValueTask DisposeAsync()
    {
        _resourcesInteropReference?.Dispose();
        _watchTaskCancellationTokenSource.Cancel();
        _watchTaskCancellationTokenSource.Dispose();
        _logsSubscription?.Dispose();

        await TaskHelpers.WaitIgnoreCancelAsync(_resourceSubscriptionTask);
    }

    public void UpdateViewModelFromQuery(ResourcesViewModel viewModel)
    {
        viewModel.SelectedViewKind = Enum.TryParse(typeof(ResourceViewKind), ViewKindName, out var view) && view is ResourceViewKind vk ? vk : null;
    }

    public string GetUrlFromSerializableViewModel(ResourcesPageState serializable)
    {
        return DashboardUrls.ResourcesUrl(view: serializable.ViewKind);
    }

    public ResourcesPageState ConvertViewModelToSerializable()
    {
        return new ResourcesPageState
        {
            ViewKind = PageViewModel.SelectedViewKind?.ToString()
        };
    }
}
