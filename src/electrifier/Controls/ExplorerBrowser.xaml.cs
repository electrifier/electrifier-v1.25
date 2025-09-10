using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using electrifier.Controls.Helpers;
using Microsoft.UI.Xaml.Controls;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

public sealed partial class ExplorerBrowser : UserControl
{
    public event EventHandler<NavigatedEventArgs> Navigated;
    public event EventHandler<NavigatingEventArgs> Navigating;
    public event EventHandler<NavigationFailedEventArgs> NavigationFailed;

    public ShellItem? CurrentFolder => History.Current;

    public readonly ShellNavigationHistory History = new();
    //private Vanara.Windows.Shell.NavigationLogDirection _navigationLogDirection;
    //private Vanara.Windows.Shell.ShellBrowserViewMode _viewMode = ShellBrowserViewMode.Details;
    /// <summary>Fires when the Items collection changes.</summary>
    public event EventHandler? ItemsChanged;


    /// <summary>Fires when the SelectedItems collection changes.</summary>
    public event EventHandler? SelectionChanged;


    /*
     public enum NavigationLogDirection
{
	/// <summary>Navigates forward through the navigation log</summary>
	Forward,

	/// <summary>Navigates backward through the travel log</summary>
	Backward
}

     
     
     *      var e = new CurrentChangingEventArgs();
            OnCurrentChanging(e);
            if (e.Cancel)
            {
                return false;
            }

            CurrentPosition = i;
            OnCurrentChanged(null!);
            return true;
     */

    private Task<HRESULT>? _currentNavigationTask;
    private bool _isLoading;


    public ExplorerBrowser()
    {
        InitializeComponent();
        DataContext = this;

        PrimaryShellTreeView.Navigated += PrimaryShellTreeView_Navigated;
        PrimaryShellListView.Navigated += PrimaryShellTreeView_Navigated;
        SecondaryShellTreeView.Navigated += SecondaryShellTreeView_Navigated;
        SecondaryShellListView.Navigated += SecondaryShellTreeView_Navigated;
    }

    internal async Task<HRESULT> Navigate(ShellBrowserItem target)
    {
        var shTargetItem = target.ShellItem;

        Debug.WriteLineIf(!shTargetItem.IsFolder, $".WARN: Navigate({target.DisplayName}) => is not a folder!");
        // TODO: If no folder, or drive empty, etc... show empty listview with error message

        // TODO: init ShellNamespaceService
        try
        {
            History.Add(target.ShellItem);
            if (_currentNavigationTask is { IsCompleted: false })
            {
                Debug.Print("ERROR! <_currentNavigationTask> already running");
                // cancel current task
                //CurrentNavigationTask
            }

            PrimaryShellListView.SetItemSource(target.ChildItems);

            if (target.ChildItems.Count <= 0)
            {
                using var shFolder = new ShellFolder(target.ShellItem);

                target.ChildItems.Clear();
                //PrimaryShellListView.ClearItems();
                //DispatcherQueue.TryEnqueue(() =>
                //{
                foreach (var child in shFolder)
                {
                    var ebItem = new ShellBrowserItem(child);

                    target.ChildItems.Add(ebItem);
                    //PrimaryShellListView.AddItem(ebItem);
                }
                //});
            }
            else
            {
                Debug.WriteLine(".Navigate() => Cache hit!");
                //PrimaryShellListView.ClearItems();
                //PrimaryShellListView.AddItems(target.ChildItems);
            }
            PrimaryShellListView.SetItemSource(target.ChildItems);

            // TODO: Load folder-open icon and overlays
        }
        catch (COMException comEx)
        {
            Debug.Fail(
                $"[Error] Navigate(<{target}>) failed. COMException: <HResult: {comEx.HResult}>: `{comEx.Message}`");

            return new HRESULT(comEx.HResult);
        }
        catch (Exception ex)
        {
            Debug.Fail($"[Error] Navigate(<{target}>) failed, reason unknown: {ex.Message}");
            throw;
        }
        finally
        {
            //IsLoading = false;
        }

        return HRESULT.S_OK;
    }


    private async void PrimaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".PrimaryShellTreeView_Navigated() to {e.NewLocation.Name}");

        //var tnode = e.NewLocation;
        var treeNode = PrimaryShellTreeView.SelectedItem;
        var cnt = treeNode?.Content;
        //treeNode.IsSelected = true;




        _ = Navigate(new ShellBrowserItem(e.NewLocation));  // WARN: This is a fire-and-forget call, no await! // WARN: Use existing ShellBrowserItem from TreeView
    }

    private async void SecondaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".SecondaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        SecondaryShellListView.ClearItems();

        try
        {
            var rootItem = new ShellFolder(e.NewLocation.PIDL);

            var childItems = rootItem?.EnumerateChildren(FolderItemFilter.Folders | FolderItemFilter.NonFolders | FolderItemFilter.IncludeHidden);
            if (childItems == null)
            {
                Debug.Fail($"[Error] Navigate(<{e.NewLocation.Name}>) failed. No items found.");
                return;
            }

            var newBrowserItems = new List<ShellBrowserItem>();
            foreach (var item in childItems)
            {
                newBrowserItems.Add(new ShellBrowserItem(new(item.PIDL)));
            }

            SecondaryShellListView.AddItems(newBrowserItems);
        }
        catch (COMException comEx)
        {
            Debug.Fail(
                $"[Error] Navigate(<{e.NewLocation.Name}>) failed. COMException: <HResult: {comEx.HResult}>: `{comEx.Message}`");
            //NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(comEx));
        }
        catch (Exception ex)
        {
            Debug.Fail($"[Error] Navigate(<{e.NewLocation.Name}>) failed, reason unknown: {ex.Message}");
            throw;
        }
    }

    private void BackAppBarButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (History.CanSeekBackward)
        {
            var shItem = History.SeekBackward();
            Debug.Print($".BackAppBarButtonClick() to {shItem?.Name} (coming from {History.Current})");
            var newItem = new ShellBrowserItem(shItem);
            _ = Navigate(newItem);
        }
    }

    private void ForwardAppBarButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (History.CanSeekForward)
        {
            var shItem = History.SeekForward();
            Debug.Print($".ForwardAppBarButtonClick() to {shItem?.Name} (coming from {History.Current})");
            var newItem = new ShellBrowserItem(shItem);
            _ = Navigate(newItem);
        }
    }

    private void UpParentAppBarButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if ((History.Count > 0) && (History.Current != null))
        {
            var prnt = History.Current.Parent;
            Debug.Print($".UpParentAppBarButtonClick() to {prnt?.Name} (coming from {History.Current})");
            if (prnt == null)
            {
                Debug.Print(".UpParentAppBarButtonClick() => No parent, at root?");
                return;
            }
            var parentItem = new ShellBrowserItem(prnt);
            _ = Navigate(parentItem);
        }
    }
}

/// <summary>Event argument for The Navigated event</summary>
public class NavigatedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="NavigatedEventArgs"/> class.</summary>
    /// <param name="folder">The folder.</param>
    public NavigatedEventArgs(ShellFolder folder) => NewLocation = folder;

    /// <summary>The new location of the explorer browser</summary>
    public ShellItem NewLocation
    {
        get; private set;
    }
}

/// <summary>Event argument for The Navigating event</summary>
public class NavigatingEventArgs : CancelEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="NavigatingEventArgs"/> class.</summary>
    /// <param name="pendingLocation">The pending location.</param>
    public NavigatingEventArgs(ShellItem pendingLocation) => PendingLocation = pendingLocation;

    /// <summary>The location being navigated to.</summary>
    public ShellItem PendingLocation
    {
        get; private set;
    }
}

/// <summary>Event argument for the NavigatinoFailed event</summary>
public class NavigationFailedEventArgs : EventArgs
{
    /// <summary>The location the browser would have navigated to.</summary>
    public ShellItem? FailedLocation
    {
        get; set;
    }
}
