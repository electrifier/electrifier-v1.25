/* todo: Use Visual States for Errors, Empty folders, Empty Drives */

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using electrifier.Controls.Vanara.Helpers;
using electrifier.Controls.Vanara.Services;
using Microsoft.UI.Xaml.Controls;
using Vanara.PInvoke;
using Vanara.Windows.Shell;
using IExplorerBrowser = electrifier.Controls.Vanara.Contracts.IExplorerBrowser;

namespace electrifier.Controls.Vanara;

/// <summary>
/// Replacement for <see href="https://github.com/dahall/Vanara/blob/master/Windows.Forms/Controls/ExplorerBrowser.cs">Windows.Forms/Controls/ExplorerBrowser.cs</see>
/// </summary>
public sealed partial class ExplorerBrowser : INotifyPropertyChanged
{
    public int ItemCount;

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (value == _isLoading)
            {
                return;
            }

            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public event EventHandler<NavigatedEventArgs> Navigated;
    public event EventHandler<NavigationFailedEventArgs> NavigationFailed;

    public static ShellNamespaceService ShellNamespaceService => App.GetService<ShellNamespaceService>();

    /// <summary>ExplorerBrowser Implementation for WinUI3.</summary>
    public ExplorerBrowser()
    {
        InitializeComponent();
        DataContext = this;

        ShellTreeView.NativeTreeView.SelectionChanged += NativeTreeView_SelectionChanged;
    }

    public void Navigate(ShellItem? shellItem,
        IExplorerBrowser.ExplorerBrowser.ExplorerBrowserNavigationItemCategory category =
            IExplorerBrowser.ExplorerBrowser.ExplorerBrowserNavigationItemCategory.Default)
    {
        Debug.Assert(shellItem != null);

        Debug.WriteLineIf(!shellItem.IsFolder, $"Navigate({shellItem.ToString()}) => is not a folder!");
        // TODO: If no folder, or drive empty, etc... show empty listview with error message

        // TODO: Find TreeItem here!
        BrowserItem targetItem = new(shellItem.PIDL, null, null);
        _currentNavigationTask = Navigate(targetItem);
    }

    private Task<HRESULT>? _currentNavigationTask;

    public async Task<HRESULT> Navigate(ShellItem shItem)
    {
        throw new NotImplementedException();
    }

    internal async Task<HRESULT> Navigate(BrowserItem target)
    {
        var shTargetItem = target.ShellItem;

        Debug.WriteLineIf(!shTargetItem.IsFolder, $".WARN: Navigate({target.DisplayName}) => is not a folder!");
        // TODO: If no folder, or drive empty, etc... show empty listview with error message

        // TODO: init ShellNamespaceService
        try
        {
            if (_currentNavigationTask is { IsCompleted: false })
            {
                Debug.Print("ERROR! <_currentNavigationTask> already running");
                // cancel current task
                //CurrentNavigationTask
            }

            IsLoading = true;

            if (target.ChildItems.Count <= 0)
            {
                using var shFolder = new ShellFolder(target.ShellItem);

                target.ChildItems.Clear();
                ShellListView.Items.Clear();
                foreach (var child in shFolder)
                {
                    var shStockIconId = child.IsFolder
                        ? Shell32.SHSTOCKICONID.SIID_FOLDER
                        : Shell32.SHSTOCKICONID.SIID_DOCASSOC;
                    // SHSTOCKICONID.Link and SHSTOCKICONID.SlowFile have to be used as overlay

                    var softBitmap = await StockIconFactory.GetStockIconBitmapSource(shStockIconId);

                    var ebItem = new BrowserItem(child.PIDL, child.IsFolder)
                    {
                        SoftwareBitmap = softBitmap
                    };

                    // TODO: if(child.IsLink) => Add Link-Overlay

                    target.ChildItems.Add(ebItem);
                    ShellListView.Items.Add(ebItem);
                }
            }
            else
            {
                Debug.WriteLine(".Navigate() => Cache hit!");
                ShellListView.Items.Clear();
                foreach (var child in target.ChildItems)
                {
                    var ebItem = child as BrowserItem;
                    if (ebItem is not null)
                    {
                        ShellListView.Items.Add(ebItem);
                    }
                }
            }

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
            IsLoading = false;
        }

        return HRESULT.S_OK;
    }

    //    /// <summary>
    //    /// Clears the Explorer Browser of existing content, fills it with content from the specified container, and adds a new point to the
    //    /// Travel Log.
    //    /// </summary>
    //    /// <param name="shellItem">The shell container to navigate to.</param>
    //    /// <param name="category">The category of the <paramref name="shellItem"/>.</param>
    //    public void Navigate(ShellItem? shellItem, ExplorerBrowserNavigationItemCategory category = ExplorerBrowserNavigationItemCategory.Default)
    //    {
    //        if (explorerBrowserControl is null)
    //        {
    //            antecreationNavigationTarget = (shellItem, category);
    //        }
    //        else if (shellItem is not null)
    //        {
    //            var hr = explorerBrowserControl.BrowseToObject(shellItem.IShellItem, (SBSP)category);
    //            if (hr == HRESULT_RESOURCE_IN_USE || hr == HRESULT_CANCELLED)
    //                OnNavigationFailed(new NavigationFailedEventArgs { FailedLocation = shellItem });
    //            else if (hr.Failed)
    //                throw new ArgumentException("Unable to browse to this shell item.", nameof(shellItem), hr.GetException());
    //        }
    //    }
    //
    //    /// <summary>
    //    /// Navigates to the last item in the navigation history list. This does not change the set of locations in the navigation log.
    //    /// </summary>
    //    public bool NavigateBack()
    //    {
    //        if (History.CanNavigateBackward) { Navigate(null, ExplorerBrowserNavigationItemCategory.NavigateBack); return true; }
    //        return false;
    //    }
    //
    //    /// <summary>
    //    /// Navigates to the next item in the navigation history list. This does not change the set of locations in the navigation log.
    //    /// </summary>
    //    /// <returns>True if the navigation succeeded, false if it failed for any reason.</returns>
    //    public bool NavigateForward()
    //    {
    //        if (History.CanNavigateForward) { Navigate(null, ExplorerBrowserNavigationItemCategory.NavigateForward); return true; }
    //        return false;
    //    }
    //
    //    /// <summary>
    //    /// Navigate within the navigation log in a specific direciton. This does not change the set of locations in the navigation log.
    //    /// </summary>
    //    /// <param name="direction">The direction to navigate within the navigation logs collection.</param>
    //    /// <returns>True if the navigation succeeded, false if it failed for any reason.</returns>
    //    public bool NavigateFromHistory(NavigationLogDirection direction) => History.NavigateLog(direction);
    //
    //    /// <summary>Navigate within the navigation log. This does not change the set of locations in the navigation log.</summary>
    //    /// <param name="historyIndex">An index into the navigation logs Locations collection.</param>
    //    /// <returns>True if the navigation succeeded, false if it failed for any reason.</returns>
    //    public bool NavigateToHistoryIndex(int historyIndex) => History.NavigateLog(historyIndex);
    //
    //    /// <summary>Selects all items in the current view.</summary>
    //    public void SelectAll()
    //    {
    //        var fv2 = GetFolderView2();
    //        if (fv2 is null) return;
    //        for (var i = 0; i < fv2.ItemCount(SVGIO.SVGIO_ALLVIEW); i++)
    //            fv2.SelectItem(i, SVSIF.SVSI_SELECT);
    //    }
    //
    //    /// <summary>Unselects all items in the current view.</summary>
    //    public void UnselectAll()
    //    {
    //        var fv2 = GetFolderView2();
    //        fv2?.SelectItem(-1, SVSIF.SVSI_DESELECTOTHERS);
    //    }


    private void NativeTreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        var addedItems = args.AddedItems;
        if (addedItems.Count < 1)
        {
            Debug.Fail(".NativeTreeView_SelectionChanged() failed.", "No Items added!");
        }

        Debug.Assert(addedItems.Count == 1);
        var selectedFolder = addedItems[0] as BrowserItem;
        var currentTreeNode = ShellTreeView.NativeTreeView.SelectedItem;
        Debug.Print($".NativeTreeView_SelectionChanged(`{selectedFolder?.DisplayName}`, treeNode: {currentTreeNode}).");

        // check sender!
        // TODO: ShellTreeView.NativeTreeView.SelectedItem = newTreeNode(find TreeNode

        if (selectedFolder?.PIDL is null)
        {
            Debug.Print(".NativeTreeView_SelectionChanged(): selectedFolder.PIDL is null!");
            return;
        }

        // => TODO: currentTreeNode as TreeViewNode ;
        _ = Navigate(selectedFolder);
    }

    #region Property stuff

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion Property stuff
}