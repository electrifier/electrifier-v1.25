using System.Collections.ObjectModel;
using electrifier.Controls.Vanara.Helpers;
using Microsoft.UI.Xaml.Controls;
namespace electrifier.Controls.Vanara;
public sealed partial class ShellBreadcrumbBar : UserControl
{
    /// <summary><seealso href="https://learn.microsoft.com/en-us/windows/apps/design/controls/breadcrumbbar">
    /// BreadcrumbBar control</seealso> for navigating through the Windows Shell32 namespace.</summary>
    public ShellBreadcrumbBar()
    {
        InitializeComponent();
        NativeBreadcrumbBar.ItemsSource = new ObservableCollection<BrowserItem> { };
        NativeBreadcrumbBar.ItemClicked += NativeBreadcrumbBar_ItemClicked;
    }

    /// <summary><see cref="ItemClickEventHandler"/> for <see cref="ShellBreadcrumbBar"/></summary>
    /// <param name="sender">the owner of this event</param>
    /// <param name="args">event args</param>
    private void NativeBreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (NativeBreadcrumbBar.ItemsSource is ObservableCollection<BrowserItem> items)
        {
            for (var i = items.Count - 1; i >= args.Index + 1; i--)
            {
                items.RemoveAt(i);
            }
        }
    }
}
