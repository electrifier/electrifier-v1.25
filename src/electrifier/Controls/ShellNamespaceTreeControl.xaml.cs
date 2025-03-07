using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
using static Vanara.PInvoke.Shell32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

public sealed partial class ShellNamespaceTreeControl : UserControl
{
    public TreeView NativeTreeView => TreeView;
    public ObservableCollection<BrowserItem> Items;
    //    internal readonly AdvancedCollectionView AdvancedCollectionView;
    //    public static ShellNamespaceService NamespaceService => App.GetService<ShellNamespaceService>();

    public ShellNamespaceTreeControl()
    {
        InitializeComponent();
        DataContext = this;
        Items = [];
        //        AdvancedCollectionView = new AdvancedCollectionView(TreeItems, true);
        //        NativeTreeView.ItemsSource = AdvancedCollectionView;

        //SelectionChanged = (sender, e) =>
        //{
        //    if (e.AddedItems.Count > 0)
        //    {
        //        if (e.AddedItems[0] is BrowserItem item)
        //        {
        //            var args = new SelectionChangedEventArgs(Array.Empty<object>(), Array.Empty<object>());
        //            SelectionChanged(this, args);
        //        }
        //    }
        //};

        Loading += ShellNamespaceTreeControl_Loading;

        //TreeView.SelectionChanged += (sender, e) =>
        //{
        //    if (e.AddedItems.Count > 0)
        //    {
        //        if (e.AddedItems[0] is BrowserItem item)
        //        {
        //            SelectionChanged(this, e);
        //        }
        //    }
        //};
    }

    public Action<object, TreeViewSelectionChangedEventArgs> SelectionChanged
    {
        get;
        internal set;
    }
    public TreeViewNode SelectedItem
    {
        get;
        internal set;
    }

    private void ShellNamespaceTreeControl_Loading(FrameworkElement sender, object args)
    {
        // TODO: Raise event, and let the parent decide which folders to use as root
        var rootItem = new BrowserItem(ShellFolder.Desktop.PIDL, true);
        rootItem.TreeViewItemIsSelected = true;
        Items.Add(rootItem);

        //        var homeItem = BrowserItemFactory.FromShellFolder(IExplorerBrowser.HomeShellFolder);
        //        homeItem.TreeViewItemIsSelected = true;
        //        Items.Add(homeItem);
        //        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_SkyDrive));
        //        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Desktop));
        //        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Downloads));
        //        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Documents));
        //        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Pictures));
        //        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Music));
        //        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Videos));
    }

    // TODO: Bind to Property
    public void Navigate(BrowserItem item)
    {
        if (item == null)
        {
            return;
        }
    }
}