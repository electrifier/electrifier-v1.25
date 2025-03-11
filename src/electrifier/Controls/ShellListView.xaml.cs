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
using Vanara.Windows.Shell;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

public sealed partial class ShellListView : UserControl
{
    internal ItemsView NativeItemsView => ItemsView;
    public ObservableCollection<ShellBrowserItem> Items = [];
//    public readonly AdvancedCollectionView AdvancedCollectionView;

    public ShellListView()
    {
        this.InitializeComponent();

//        this.OnGotFocus += ShellListView_GotFocus;
//        this.OnLostFocus += ShellListView_LostFocus;
    }

    public void Navigate(ShellBrowserItem shellBrowserItem)
    {
        Items.Clear();
        Items.Add(shellBrowserItem);
    }
}
