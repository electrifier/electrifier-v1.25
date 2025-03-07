using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using electrifier.Controls.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

public sealed partial class ShellNamespaceTreeControl : UserControl
{
    public ShellNamespaceTreeControl()
    {
        this.InitializeComponent();

        this.SelectionChanged = (sender, e) =>
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as BrowserItem;

                if (item != null)
                {
                    var args = new SelectionChangedEventArgs(Array.Empty<object>(), Array.Empty<object>());
                    SelectionChanged(this, args);
                }
            }
        };
    }

    public Action<object, SelectionChangedEventArgs> SelectionChanged
    {
        get;
        internal set;
    }
}
