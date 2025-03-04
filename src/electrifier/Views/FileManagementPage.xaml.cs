using electrifier.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace electrifier.Views;

public sealed partial class FileManagementPage : Page
{
    public FileManagementViewModel ViewModel
    {
        get;
    }

    public FileManagementPage()
    {
        ViewModel = App.GetService<FileManagementViewModel>();
        InitializeComponent();
    }
}
