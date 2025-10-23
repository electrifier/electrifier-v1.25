using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using electrifier.Controls.Helpers;
using electrifier.Controls.Services;
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

    public readonly ShellNavigationHistory History = new(); // Save Selection , ScrollPosition, etc. to global history
    //private Vanara.Windows.Shell.NavigationLogDirection _navigationLogDirection;
    //private Vanara.Windows.Shell.ShellBrowserViewMode _viewMode = ShellBrowserViewMode.Details;
    /// <summary>Fires when the Items collection changes.</summary>
    public event EventHandler? ItemsChanged;

    /// <summary>Fires when the SelectedItems collection changes.</summary>
    public event EventHandler? SelectionChanged;

    public ExplorerBrowser()
    {
        InitializeComponent();
        DataContext = this;

        Navigated += ExplorerBrowser_Navigated;
        Navigating += ExplorerBrowser_Navigating;
        NavigationFailed += ExplorerBrowser_NavigationFailed;

//        PrimaryShellTreeView.Navigated += PrimaryShellTreeView_Navigated;
//        PrimaryShellListView.Navigated += PrimaryShellListView_Navigated;
    }

    private void ExplorerBrowser_Navigated(object? sender, NavigatedEventArgs e)
    {
        Debug.Print($".ExplorerBrowser_Navigated() to {e.NewLocation.Name}");
    }
    private void ExplorerBrowser_Navigating(object? sender, NavigatingEventArgs e)
    {
        Debug.Print($".ExplorerBrowser_Navigating() to {e.PendingLocation.Name}");
        History.Add(e.PendingLocation);
    }
    private void ExplorerBrowser_NavigationFailed(object? sender, NavigationFailedEventArgs e)
    {
        Debug.Fail($".ExplorerBrowser_NavigationFailed() to {e.FailedLocation?.Name}");
    }

    internal async Task<HRESULT> Navigate(ShellBrowserItem target)
    {
        var shTargetItem = target.ShellItem;

        Debug.WriteLineIf(!shTargetItem.IsFolder, $".WARN: Navigate({target.DisplayName}) => is not a folder!");
        // TODO: If no folder, or drive empty, etc... show empty listview with error message
        // INFO: put this test into ShellNamespaceService, see older implementation

        // TODO: init ShellNamespaceService
        try
        {
            Navigating.Invoke(this, new NavigatingEventArgs(shTargetItem));
            //PrimaryShellListView.SetItemSource(target.ChildItems);

            if (target.ChildItems.Count <= 0)
            {
                using var shFolder = new ShellFolder(target.ShellItem);

                // TODO: See ShellCategorizer for filtering and grouping
                target.ChildItems.Clear();
                foreach (var child in shFolder)
                {
                    var ebItem = new ShellBrowserItem(child);

                    target.ChildItems.Add(ebItem);
                }
            }
            else
            {
                Debug.WriteLine(".Navigate() => Cache hit!");
            }
            //PrimaryShellListView.SetItemSource(target.ChildItems);       // TODO: Optimize to avoid resetting the ItemSource if already set to the same collection

            // TODO: Load folder-open icon and overlays
            // TODO: IconExtractor can extract folder bitmaps with content preview
        }
        catch (COMException comEx)
        {
            Debug.Fail($"[Error] Navigate(<{target}>) failed. COMException: <HResult: {comEx.HResult}>: `{comEx.Message}`");
            NavigationFailed.Invoke(this, new NavigationFailedEventArgs(shTargetItem));
            throw;
        }
        catch (Exception ex)
        {
            Debug.Fail($"[Error] Navigate(<{target}>) failed, reason unknown: {ex.Message}");
            NavigationFailed.Invoke(this, new NavigationFailedEventArgs(shTargetItem));
            throw;
        }
        finally
        {
            //Navigated.Invoke(this, new NavigatedEventArgs(shTargetItem as ShellFolder ?? ShellFolder.Desktop));
        }

        return HRESULT.S_OK;
    }

    // SingleClick => Navigate
    // DoubleClick => Navigate & Expand
    private async void PrimaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".PrimaryShellTreeView_Navigated() to {e.NewLocation.Name}");

        var target = e.NewLocation;

        //var tnode = e.NewLocation;
        //var treeNode = PrimaryShellTreeView.SelectedItem;
        //var cnt = treeNode?.Content;
        //browserItem.IsSelected = true;
        //var shBrowserItem = cnt as ShellBrowserItem;




        var navtask = Navigate(new ShellBrowserItem(e.NewLocation));  // WARN: This is a fire-and-forget call, no await! // WARN: Use existing ShellBrowserItem from TreeView
        await navtask;
    }

    private async void PrimaryShellListView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".PrimaryShellListView_Navigated() to {e.NewLocation.Name}");
        var navtask = Navigate(new ShellBrowserItem(e.NewLocation));  // WARN: This is a fire-and-forget call, no await! // WARN: Use existing ShellBrowserItem from TreeView
        await navtask;
    }

    private void BackAppBarButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (History.CanSeekBackward)
        {
            var shItem = History.SeekBackward();
            Debug.Print($".BackAppBarButtonClick() to {shItem?.Name} (coming from {History.Current})");
            // TODO: Error handling if shItem is null or navigation fails, call NavigationFailed then
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
    public NavigatedEventArgs(ShellBrowserItem browserItem)
    {
        ExplorerBrowserItem = browserItem ?? throw new ArgumentNullException(nameof(browserItem));
        NewLocation = ExplorerBrowserItem.ShellItem;
    }

    /// <summary>Initializes a new instance of the <see cref="NavigatedEventArgs"/> class.</summary>
    /// <param name="folder">The folder.</param>
    //public NavigatedEventArgs(ShellItem folder) => NewLocation = folder ?? throw new ArgumentNullException(nameof(folder));

    public ShellBrowserItem? ExplorerBrowserItem;

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
    public NavigatingEventArgs(ShellItem pendingLocation) => PendingLocation = pendingLocation ?? throw new ArgumentNullException(nameof(pendingLocation));

    /// <summary>The location being navigated to.</summary>
    public ShellItem PendingLocation
    {
        get; private set;
    }
}

/// <summary>Event argument for the NavigatinoFailed event</summary>
public class NavigationFailedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="NavigationFailedEventArgs"/> class.</summary>
    /// <param name="failedLocation">The failed location.</param>
    public NavigationFailedEventArgs(ShellItem failedLocation) => FailedLocation = failedLocation ?? throw new ArgumentNullException(nameof(failedLocation));

    public Exception? CausalException
    {
        get; private set;
    }

    public HRESULT? HResult
    {
        get; private set;
    }

    /// <summary>The location the browser would have navigated to.</summary>
    public ShellItem? FailedLocation
    {
        get; private set;
    }
}
