// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Aspire.Dashboard.Model;
using Aspire.Dashboard.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Aspire.Dashboard.Components.ResourcesGridColumns;

public partial class StateColumnDisplay
{
    public static StateIconInfo GetStateIcon(ResourceViewModel resource, IStringLocalizer loc)
    {
        if (resource is { State: ResourceStates.ExitedState /* containers */ or ResourceStates.FinishedState /* executables */ or ResourceStates.FailedToStartState })
        {
            if (resource.TryGetExitCode(out int exitCode) && exitCode is not 0)
            {
                // process completed unexpectedly, hence the non-zero code. this is almost certainly an error, so warn users
                return new StateIconInfo
                {
                    Tooltip = string.Format(CultureInfo.InvariantCulture, loc[Columns.StateColumnResourceExitedUnexpectedly], resource.ResourceType, exitCode),
                    Icon = new Icons.Filled.Size16.ErrorCircle(),
                    Color = Color.Error,
                };
            }
            else
            {
                // process completed, which may not have been unexpected
                return new StateIconInfo
                {
                    Tooltip = string.Format(CultureInfo.InvariantCulture, loc[Columns.StateColumnResourceExited], resource.ResourceType),
                    Icon = new Icons.Filled.Size16.Warning(),
                    Color = Color.Warning,
                };
            }
        }
        else if (resource is { State: ResourceStates.StartingState })
        {
            return new StateIconInfo
            {
                Icon = new Icons.Filled.Size16.CircleHint(),
                Color = Color.Info,
            };
        }
        else if (resource is { State: /* unknown */ null or { Length: 0 } })
        {
            return new StateIconInfo
            {
                Icon = new Icons.Filled.Size16.Circle(),
                Color = Color.Neutral,
            };
        }
        else if (!string.IsNullOrEmpty(resource.StateStyle))
        {
            Icon icon;
            Color color;
            switch (resource.StateStyle)
            {
                case "warning":
                    icon = new Icons.Filled.Size16.Warning();
                    color = Color.Warning;
                    break;
                case "error":
                    icon = new Icons.Filled.Size16.ErrorCircle();
                    color = Color.Error;
                    break;
                case "success":
                    icon = new Icons.Filled.Size16.CheckmarkCircle();
                    color = Color.Success;
                    break;
                case "info":
                    icon = new Icons.Filled.Size16.Info();
                    color = Color.Info;
                    break;
                default:
                    icon = new Icons.Filled.Size16.Circle();
                    color = Color.Neutral;
                    break;
            }

            return new StateIconInfo
            {
                Icon = icon,
                Color = color,
            };
        }
        else
        {
            return new StateIconInfo
            {
                Icon = new Icons.Filled.Size16.CheckmarkCircle(),
                Color = Color.Success,
            };
        }
    }
}

public sealed class StateIconInfo
{
    public string? Tooltip { get; init; }
    public required Icon Icon { get; init; }
    public required Color Color { get; init; }
}
