using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using electrifier.Controls;
using electrifier.Controls.Helpers;
using electrifier.Controls.Services;
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
using static Vanara.PInvoke.ComCtl32;
using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.Shell32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

public sealed partial class ExplorerBrowser : UserControl
{
    public ObservableCollection<BrowserItem> CurrentItems;
//    public event EventHandler<Vanara.Windows.Shell.NavigatedEventArgs> Navigated;
//    public event EventHandler<Vanara.Windows.Shell.NavigationFailedEventArgs> NavigationFailed;
//
    private Task<HRESULT>? _currentNavigationTask;
    private bool _isLoading;


    public ExplorerBrowser()
    {
        InitializeComponent();
        DataContext = this;

        PrimaryShellTreeView.Navigated += PrimaryShellTreeView_Navigated;
        //PrimaryShellListView.Navigated += PrimaryShellTreeView_Navigated;
        //SecondaryShellTreeView.Navigated += SecondaryShellTreeView_Navigated;
        //SecondaryShellListView.Navigated += SecondaryShellTreeView_Navigated;
    }

    internal async Task<HRESULT> Navigate(BrowserItem target)
    {
        var shTargetItem = target.ShellItem;

        Debug.WriteLineIf(!shTargetItem.IsFolder, $".WARN: Navigate({target.DisplayName}) => is not a folder!");
        // TODO: If no folder, or drive empty, etc... show empty listview with error message

        // TODO: init ShellNamespaceService
        try
        {
            if (_currentNavigationTask is { IsCompleted: false })
            {
                Debug.Print("ERROR! <_currentNavigationTask> already running");
                // cancel current task
                //CurrentNavigationTask
            }

            // IsLoading = true;

            if (target.ChildItems.Count <= 0)
            {
                using var shFolder = new ShellFolder(target.ShellItem);

                target.ChildItems.Clear();
                PrimaryShellListView.Items.Clear();
                foreach (var child in shFolder)
                {
                    var ebItem = new BrowserItem(child.PIDL, null, null);

                    target.ChildItems.Add(ebItem);
                    PrimaryShellListView.Items.Add(ebItem);
                }
            }
            else
            {
                Debug.WriteLine(".Navigate() => Cache hit!");
                PrimaryShellListView.Items.Clear();
                foreach (var child in target.ChildItems)
                {
//                    PrimaryShellListView.Items.Add(child);
                }
            }

            // TODO: Load folder-open icon and overlays
        }
        catch (COMException comEx)
        {
            Debug.Fail(
                $"[Error] Navigate(<{target}>) failed. COMException: <HResult: {comEx.HResult}>: `{comEx.Message}`");

            return new HRESULT(comEx.HResult);
        }
        catch (Exception ex)
        {
            Debug.Fail($"[Error] Navigate(<{target}>) failed, reason unknown: {ex.Message}");
            throw;
        }
        finally
        {
            //IsLoading = false;
        }

        return HRESULT.S_OK;
    }


    private async void PrimaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".PrimaryShellTreeView_Navigated() to {e.NewLocation.Name}");

        _ = Navigate(new BrowserItem(e.NewLocation.PIDL, null, null));  // WARN: This is a fire-and-forget call, no await! // WARN: Use existing ShellBrowserItem from TreeView
    }

    private async void SecondaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".SecondaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        SecondaryShellListView.ClearItems();

        try
        {
            var rootItem = new ShellFolder(e.NewLocation.PIDL);

            var childItems = rootItem?.EnumerateChildren(FolderItemFilter.Folders | FolderItemFilter.NonFolders | FolderItemFilter.IncludeHidden);
            if (childItems == null)
            {
                Debug.Fail($"[Error] Navigate(<{e.NewLocation.Name}>) failed. No items found.");
                return;
            }

            var newBrowserItems = new List<BrowserItem>();
            foreach (var item in childItems)
            {
                newBrowserItems.Add(new BrowserItem(item.PIDL, null, null));
            }

            SecondaryShellListView.AddItems(newBrowserItems);
        }
        catch (COMException comEx)
        {
            Debug.Fail(
                $"[Error] Navigate(<{e.NewLocation.Name}>) failed. COMException: <HResult: {comEx.HResult}>: `{comEx.Message}`");
            //NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(comEx));
        }
        catch (Exception ex)
        {
            Debug.Fail($"[Error] Navigate(<{e.NewLocation.Name}>) failed, reason unknown: {ex.Message}");
            throw;
        }
    }

//    /// <summary>Event argument for The Navigated event</summary>
//    public class NavigatedEventArgs : EventArgs
//    {
//        /// <summary>The new location of the explorer browser</summary>
//        public ShellItem? NewLocation
//        {
//            get; set;
//        }
//    }
//
//    /// <summary>Event argument for The Navigating event</summary>
//    public class NavigatingEventArgs : EventArgs
//    {
//        /// <summary>Set to 'True' to cancel the navigation.</summary>
//        public bool Cancel
//        {
//            get; set;
//        }
//
//        /// <summary>The location being navigated to</summary>
//        public ShellItem? PendingLocation
//        {
//            get; set;
//        }
//    }
//
//    /// <summary>Event argument for the NavigatinoFailed event</summary>
//    public class NavigationFailedEventArgs : EventArgs
//    {
//        /// <summary>The location the browser would have navigated to.</summary>
//        public ShellItem? FailedLocation
//        {
//            get; set;
//        }
//    }

}
