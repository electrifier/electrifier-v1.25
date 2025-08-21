using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.WinUI.Collections;
using electrifier.Controls.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Vanara.Windows.Shell;

// todo: For EnumerateChildren-Calls, add HWND handle
// todo: See ShellItemCollection, perhaps use this instead of ObservableCollection
// https://github.com/dahall/Vanara/blob/master/Windows.Shell.Common/ShellObjects/ShellItemArray.cs

namespace electrifier.Controls;

public partial class ShellListView : UserControl
{
    public ItemsView NativeItemsView => ItemsView;
    public ObservableCollection<BrowserItem> Items = [];
    public readonly AdvancedCollectionView AdvancedCollectionView;

    public delegate void NavigatedEventHandler(object sender, NavigatedEventArgs e);
    public event NavigatedEventHandler? Navigated;

    public ShellListView()
    {
        InitializeComponent();
        DataContext = this;
        AdvancedCollectionView = new AdvancedCollectionView(Items, true);
        //  TODO: Add custom ItemComparer, which uses Shell32 Comparison
        AdvancedCollectionView.SortDescriptions.Add(new SortDescription(SortDirection.Ascending,
            new DefaultBrowserItemComparer()));
        Debug.Assert(NativeItemsView != null, nameof(NativeItemsView) + " != null");
        NativeItemsView.ItemsSource = AdvancedCollectionView;
    }

    public void AddItem(BrowserItem shellBrowserItem) => Items.Add(shellBrowserItem);

    public void AddItems(IEnumerable<BrowserItem> shellBrowserItems)
    {
        using (AdvancedCollectionView.DeferRefresh())
        {
            foreach (var item in shellBrowserItems)
            {
                Items.Add(item);
            }
        }
    }

    public void ClearItems()
    {
        using (AdvancedCollectionView.DeferRefresh())
        {
            Items.Clear();
        }
    }

    /// <summary>
    /// Default sort of <see cref="BrowserItem"/>s.
    /// <b>WARN: This is not</b> the exact Comparison Windows File Explorer uses.
    /// </summary>
    public class DefaultBrowserItemComparer : IComparer
    {
        public int Compare(object? x, object? y)
        {
            if (x is not BrowserItem left || y is not BrowserItem right)
            {
                return new Comparer(CultureInfo.InvariantCulture).Compare(x, y);
            }

            return left.IsFolder switch
            {
                true when right.IsFolder == false => -1,
                false when right.IsFolder == true => 1,
                _ => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase)
            };
        }
    }

    private void ItemsView_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (!e.Handled)
        {
            try
            {
                var shellBrowserItem = (NativeItemsView.SelectedItem) as BrowserItem;
                var shellItem = shellBrowserItem?.ShellItem;
                Debug.Assert(shellItem != null, nameof(shellItem) + " != null");
                if (shellItem.IsFolder)
                {
                    var shFolder = new ShellFolder(shellItem);
                    if (shFolder != null)
                    {
                        Navigated?.Invoke(this, new NavigatedEventArgs(shFolder));
                        //Navigated?.BeginInvoke(this, item, null, null);
                        e.Handled = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.Fail(exception.ToString());
                throw;
            }
        }
    }
}