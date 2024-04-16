// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Layout;

public partial class NavMenu : ComponentBase, IDisposable
{
    private static Icon ResourcesIcon(bool active = false) =>
        active ? new Icons.Filled.Size24.AppFolder()
                  : new Icons.Regular.Size24.AppFolder();

    private static Icon ConsoleLogsIcon(bool active = false) =>
        active ? new Icons.Filled.Size24.SlideText()
                  : new Icons.Regular.Size24.SlideText();

    private static Icon StructuredLogsIcon(bool active = false) =>
        active ? new Icons.Filled.Size24.SlideTextSparkle()
                  : new Icons.Regular.Size24.SlideTextSparkle();

    private static Icon TracesIcon(bool active = false) =>
        active ? new Icons.Filled.Size24.GanttChart()
                  : new Icons.Regular.Size24.GanttChart();

    private static Icon MetricsIcon(bool active = false) =>
        active ? new Icons.Filled.Size24.ChartMultiple()
                  : new Icons.Regular.Size24.ChartMultiple();

    [Inject]
    public required NavigationManager NavigationManager { get; init; }

    // NavLink has limited options for matching the current address when highlighting itself as active.
    // Can't use Match.All because of the query string. Can't use Match.Prefix always because it matches every page.
    // Track whether we are on the resource page manually. If we are then change match to prefix to allow the query string.
    private bool _isResources;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        ProcessNavigationUri(NavigationManager.Uri);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        ProcessNavigationUri(e.Location);
    }

    private void ProcessNavigationUri(string location)
    {
        if (Uri.TryCreate(location, UriKind.Absolute, out var result))
        {
            var isResources = result.AbsolutePath.TrimStart('/') == DashboardUrls.ResourcesBasePath;
            if (isResources != _isResources)
            {
                _isResources = isResources;
                StateHasChanged();
            }
        }
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
