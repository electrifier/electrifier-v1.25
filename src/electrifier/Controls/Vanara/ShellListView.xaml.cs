using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.WinUI.Collections;
using electrifier.Controls.Vanara.Helpers;
using Microsoft.UI.Xaml.Controls;

// todo: For EnumerateChildren-Calls, add HWND handle
// todo: See ShellItemCollection, perhaps use this instead of ObservableCollection
// https://github.com/dahall/Vanara/blob/master/Windows.Shell.Common/ShellObjects/ShellItemArray.cs

namespace electrifier.Controls.Vanara;

public partial class ShellListView : UserControl
{
    public ItemsView NativeItemsView => ItemsView;
    public ObservableCollection<BrowserItem> Items = [];
    public readonly AdvancedCollectionView AdvancedCollectionView;

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
}