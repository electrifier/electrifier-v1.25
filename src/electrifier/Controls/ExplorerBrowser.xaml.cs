using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using electrifier.Controls.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Vanara.PInvoke;
using Vanara.Windows.Shell;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;
public sealed partial class ExplorerBrowser : UserControl
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
//            OnPropertyChanged();
        }
    }

    public event EventHandler<NavigatedEventArgs> Navigated;
//    public event EventHandler<NavigationFailedEventArgs> NavigationFailed;

//    public static ShellNamespaceService ShellNamespaceService => App.GetService<ShellNamespaceService>();


    public ExplorerBrowser()
    {
        InitializeComponent();
        DataContext = this;

        PrimaryShellTreeView.SelectionChanged += NativeTreeView_SelectionChanged;
        SecondaryShellTreeView.SelectionChanged += NativeTreeView_SelectionChanged;

        PrimaryShellTreeView.Items.Add(new BrowserItem(ShellFolder.Desktop.PIDL, true));
        SecondaryShellTreeView.Items.Add(new BrowserItem(ShellFolder.Desktop.PIDL, true));
    }

    private void NativeTreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

        var addedItems = e.AddedItems;
        if (addedItems.Count < 1)
        {
            Debug.Fail(".NativeTreeView_SelectionChanged() failed.", "No Items added!");
        }

        Debug.Assert(addedItems.Count == 1);
        var selectedFolder = addedItems[0] as BrowserItem;

        var treeView = sender as TreeView;
        var currentTreeNode = treeView?.SelectedItem as TreeViewNode;
        if (currentTreeNode != null) {
            Debug.Print($".NativeTreeView_SelectionChanged(`{selectedFolder?.DisplayName}`, treeNode: {currentTreeNode?.ToString()}).");
        }

        //        var currentTreeNode = PrimaryShellTreeView.NativeTreeView.SelectedItem;
        //         Items.Add(rootItem);


        Debug.Print($".NativeTreeView_SelectionChanged(`{selectedFolder?.DisplayName}`, treeNode: {currentTreeNode?.ToString()}).");

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

    internal async Task<HRESULT> Navigate(BrowserItem target)
    {
        var shTargetItem = target.ShellItem;

        //Debug.WriteLineIf(!target.IsFolder, $"Navigate({target.DisplayName}) => is not a folder!");
        // TODO: If no folder, or drive empty, etc... show empty listview with error message


        // TODO: init ShellNamespaceService
        try
        {
//            if (_currentNavigationTask is { IsCompleted: false })
//          {
//              Debug.Print("ERROR! <_currentNavigationTask> already running");
//              // cancel current task
//              //CurrentNavigationTask
//          }

            IsLoading = true;

            if (target.ChildItems.Count <= 0)
            {
                using var shFolder = new ShellFolder(target.ShellItem);

                target.ChildItems.Clear();
//                ShellListView.Items.Clear();
                foreach (var child in shFolder)
                {
                    var shStockIconId = child.IsFolder
                        ? Shell32.SHSTOCKICONID.SIID_FOLDER
                        : Shell32.SHSTOCKICONID.SIID_DOCASSOC;
                    // SHSTOCKICONID.Link and SHSTOCKICONID.SlowFile have to be used as overlay

//                    var softBitmap = await StockIconFactory.GetStockIconBitmapSource(shStockIconId);

                    var ebItem = new BrowserItem(child.PIDL, child.IsFolder)
                    {
//                        SoftwareBitmap = softBitmap
                    };

                    // TODO: if(child.IsLink) => Add Link-Overlay

                    target.ChildItems.Add(ebItem);
//                    ShellListView.Items.Add(ebItem);
                }
            }
            else
            {
                Debug.WriteLine(".Navigate() => Cache hit!");
//                ShellListView.Items.Clear();
                foreach (var child in target.ChildItems)
                {
                    var ebItem = child as BrowserItem;
                    if (ebItem is not null)
                    {
//                        ShellListView.Items.Add(ebItem);
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

}
