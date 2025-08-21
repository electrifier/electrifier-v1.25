using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.WinUI.Collections;
using electrifier.Controls.Contracts;
using electrifier.Controls.Helpers;
using electrifier.Controls.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

// todo: For EnumerateChildren-Calls, add HWND handle
// todo: See ShellItemCollection, perhaps use this instead of ObservableCollection
// https://github.com/dahall/Vanara/blob/master/Windows.Shell.Common/ShellObjects/ShellItemArray.cs

namespace electrifier.Controls;

public partial class ShellNamespaceTreeControl : UserControl
{
    public TreeView NativeTreeView => TreeView;
    public ObservableCollection<BrowserItem> TreeItems;
    internal readonly AdvancedCollectionView AdvancedCollectionView;
    public static ShellNamespaceService NamespaceService => App.GetService<ShellNamespaceService>();

    public delegate void NavigatedEventHandler(object sender, NavigatedEventArgs e);
    public event NavigatedEventHandler? Navigated;


    public ShellNamespaceTreeControl()
    {
        InitializeComponent();
        DataContext = this;
        TreeItems = [];
        AdvancedCollectionView = new AdvancedCollectionView(TreeItems, true);
        NativeTreeView.ItemsSource = AdvancedCollectionView;

        Loading += ShellNamespaceTreeControl_Loading;
        NativeTreeView.SelectionChanged += OnSelectionChanged;

    }

    private void ShellNamespaceTreeControl_Loading(FrameworkElement sender, object args)
    {
        // TODO: Raise event, and let the parent decide which folders to use as root
        var homeItem = BrowserItemFactory.FromShellFolder(IExplorerBrowser.HomeShellFolder);
        homeItem.TreeViewItemIsSelected = true;
        TreeItems.Add(homeItem);
        TreeItems.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_SkyDrive));
        TreeItems.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Desktop));
        TreeItems.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Downloads));
        TreeItems.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Documents));
        TreeItems.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Pictures));
        TreeItems.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Music));
        TreeItems.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Videos));
    }

    private void OnSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs e)
    {
        Debug.WriteIf((e.AddedItems.Count < 1 && e.RemovedItems.Count < 1), "None or less Items added nor removed", ".OnSelectionChanged() parameter mismatch.");
        if (e.AddedItems[0] is not BrowserItem shellBrowserItem)
        {
            Debug.Fail(".OnSelectionChanged(): Invalid item");
            return;
        }
        Navigated?.Invoke(this, new NavigatedEventArgs(new ShellFolder(shellBrowserItem.ShellItem)));
    }

    // TODO: public object ItemFromContainer => NativeTreeView.ItemFromContainer()
}
