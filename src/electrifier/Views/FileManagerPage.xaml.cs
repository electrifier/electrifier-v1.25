using electrifier.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace electrifier.Views;

public sealed partial class FileManagerPage : Page
{
    public FileManagerViewModel ViewModel
    {
        get;
    }

    public FileManagerPage()
    {
        ViewModel = App.GetService<FileManagerViewModel>();
        InitializeComponent();
    }
}
