using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace electrifier.Controls.Vanara.Services;

/// <summary>The navigation log is a history of the locations visited by the explorer browser.</summary>
public class NavigationLog
{
    private readonly ExplorerBrowser? parent = null;

    /// <summary>The pending navigation log action. null if the user is not navigating via the navigation log.</summary>
    private PendingNavigation? pendingNavigation;

    internal NavigationLog(ExplorerBrowser parent)
    {
        // Hook navigation events from the parent to distinguish between navigation log induced navigation, and other navigations.
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        this.parent.Navigated += OnNavigated;
        this.parent.NavigationFailed += OnNavigationFailed;
    }

    /// <summary>Fires when the navigation log changes or the current navigation position changes</summary>
    public event EventHandler<NavigationLogEventArgs>? NavigationLogChanged;

    /// <summary>Indicates the presence of locations in the log that can be reached by calling Navigate(Backward)</summary>
    public bool CanNavigateBackward => CurrentLocationIndex > 0;

    /// <summary>Indicates the presence of locations in the log that can be reached by calling Navigate(Forward)</summary>
    public bool CanNavigateForward => CurrentLocationIndex < Locations.Count - 1;

    /// <summary>Gets the shell object in the Locations collection pointed to by CurrentLocationIndex.</summary>
    public ShellItem? CurrentLocation => CurrentLocationIndex < 0 ? null : Locations[CurrentLocationIndex];

    /// <summary>
    /// An index into the Locations collection. The ShellItem pointed to by this index is the current location of the ExplorerBrowser.
    /// </summary>
    public int CurrentLocationIndex { get; set; } = -1;

    /// <summary>The navigation log</summary>
    public List<ShellItem> Locations { get; } = new List<ShellItem>();

    /// <summary>Clears the contents of the navigation log.</summary>
    public void Clear()
    {
        if (Locations.Count == 0) return;

        var oldCanNavigateBackward = CanNavigateBackward;
        var oldCanNavigateForward = CanNavigateForward;

        Locations.Clear();
        CurrentLocationIndex = -1;

        var args = new NavigationLogEventArgs
        {
            LocationsChanged = true,
            CanNavigateBackwardChanged = oldCanNavigateBackward != CanNavigateBackward,
            CanNavigateForwardChanged = oldCanNavigateForward != CanNavigateForward
        };
        NavigationLogChanged?.Invoke(this, args);
    }

    internal bool NavigateLog(NavigationLogDirection direction)
    {
        // determine proper index to navigate to
        int locationIndex;
        switch (direction)
        {
            case NavigationLogDirection.Backward when CanNavigateBackward:
                locationIndex = CurrentLocationIndex - 1;
                break;

            case NavigationLogDirection.Forward when CanNavigateForward:
                locationIndex = CurrentLocationIndex + 1;
                break;

            default:
                return false;
        }

        // initiate traversal request
        var location = Locations[locationIndex];
        pendingNavigation = new PendingNavigation(location, locationIndex);
        parent?.Navigate(location);
        return true;
    }

    internal bool NavigateLog(int index)
    {
        // can't go anywhere
        if (index >= Locations.Count || index < 0) return false;

        // no need to re navigate to the same location
        if (index == CurrentLocationIndex) return false;

        // initiate traversal request
        var location = Locations[index];
        pendingNavigation = new PendingNavigation(location, index);
        parent?.Navigate(location);
        return true;
    }

    private void OnNavigated(object? sender, NavigatedEventArgs args)
    {
        var eventArgs = new NavigationLogEventArgs();
        var oldCanNavigateBackward = CanNavigateBackward;
        var oldCanNavigateForward = CanNavigateForward;

        if (pendingNavigation != null)
        {
            // navigation log traversal in progress

            // determine if new location is the same as the traversal request
            var shellItemsEqual = pendingNavigation.Location.IShellItem.Compare(args.NewLocation?.IShellItem, Shell32.SICHINTF.SICHINT_ALLFIELDS, out var i).Succeeded && i == 0;
            if (!shellItemsEqual)
            {
                // new location is different than traversal request, behave is if it never happened! remove history following
                // currentLocationIndex, append new item
                if (CurrentLocationIndex < Locations.Count - 1)
                {
                    Locations.RemoveRange(CurrentLocationIndex + 1, Locations.Count - (CurrentLocationIndex + 1));
                }
                if (args.NewLocation is not null) Locations.Add(args.NewLocation);
                CurrentLocationIndex = Locations.Count - 1;
                eventArgs.LocationsChanged = true;
            }
            else
            {
                // log traversal successful, update index
                CurrentLocationIndex = pendingNavigation.Index;
                eventArgs.LocationsChanged = false;
            }
            pendingNavigation = null;
        }
        else
        {
            // remove history following currentLocationIndex, append new item
            if (CurrentLocationIndex < Locations.Count - 1)
            {
                Locations.RemoveRange(CurrentLocationIndex + 1, Locations.Count - (CurrentLocationIndex + 1));
            }
            if (args.NewLocation is not null) Locations.Add(args.NewLocation);
            CurrentLocationIndex = Locations.Count - 1;
            eventArgs.LocationsChanged = true;
        }

        // update event args
        eventArgs.CanNavigateBackwardChanged = oldCanNavigateBackward != CanNavigateBackward;
        eventArgs.CanNavigateForwardChanged = oldCanNavigateForward != CanNavigateForward;

        NavigationLogChanged?.Invoke(this, eventArgs);
    }

    private void OnNavigationFailed(object? sender, NavigationFailedEventArgs args) => pendingNavigation = null;

    /// <summary>A navigation traversal request</summary>
    private class PendingNavigation
    {
        internal PendingNavigation(ShellItem location, int index)
        {
            Location = location;
            Index = index;
        }

        internal int Index
        {
            get; set;
        }

        internal ShellItem Location
        {
            get; set;
        }
    }
}