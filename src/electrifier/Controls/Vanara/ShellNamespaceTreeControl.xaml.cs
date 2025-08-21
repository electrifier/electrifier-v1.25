using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.Collections;
using electrifier.Controls.Vanara.Contracts;
using electrifier.Controls.Vanara.Helpers;
using electrifier.Controls.Vanara.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vanara.PInvoke;

// todo: For EnumerateChildren-Calls, add HWND handle
// todo: See ShellItemCollection, perhaps use this instead of ObservableCollection
// https://github.com/dahall/Vanara/blob/master/Windows.Shell.Common/ShellObjects/ShellItemArray.cs

namespace electrifier.Controls.Vanara;

public partial class ShellNamespaceTreeControl : UserControl
{
    public TreeView NativeTreeView => TreeView;
    public ObservableCollection<BrowserItem> TreeItems;
    internal readonly AdvancedCollectionView AdvancedCollectionView;
    public static ShellNamespaceService NamespaceService => App.GetService<ShellNamespaceService>();

    public ShellNamespaceTreeControl()
    {
        InitializeComponent();
        DataContext = this;
        TreeItems = [];
        AdvancedCollectionView = new AdvancedCollectionView(TreeItems, true);
        NativeTreeView.ItemsSource = AdvancedCollectionView;

        Loading += ShellNamespaceTreeControl_Loading;
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

    // TODO: public object ItemFromContainer => NativeTreeView.ItemFromContainer()
}
