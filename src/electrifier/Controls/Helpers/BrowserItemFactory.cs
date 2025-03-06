using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace electrifier.Controls.Helpers
{
    class BrowserItemFactory
    {
    }

    /// <summary>Abstract base class BrowserItem of Type <typeparam name="T"/>.</summary>
    /// <typeparam name="T">The derived Type of this abstract class.</typeparam>
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public abstract class AbstractBrowserItem<T> // TODO: IDisposable
    {
        public readonly List<AbstractBrowserItem<T>> ChildItems;
        public readonly bool? IsFolder;
        public readonly bool IsRootItem;

        /// <summary>Abstract base class BrowserItem of Type <typeparam name="T"/>.</summary>
        /// <typeparam name="T">The derived Type of this abstract class.</typeparam>
        /// <param name="isFolder" >
        /// <value>true</value>
        /// Default: False.</param>
        /// <param name="childItems">Default: Create new empty List of child items <typeparam name="T">childItems</typeparam>.</param>
        protected AbstractBrowserItem(bool? isFolder, List<AbstractBrowserItem<T>>? childItems)
        {
            ChildItems = childItems ?? [];
            IsFolder = isFolder;
        }

        public virtual Task EnumChildItems() => Task.CompletedTask;

        //internal void async IconUpdate(int Index, SoftwareBitmapSource bmpSrc);
        //internal void async StockIconUpdate(STOCKICONID id, SoftwareBitmapSource bmpSrc);
        //internal void async ChildItemsIconUpdate();
        public new string ToString() => $"AbstractBrowserItem(<{typeof(T)}>(isFolder {IsFolder}, childItems {ChildItems})";
    }

    // TODO: IDisposable
    // TODO: IComparable
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public partial class BrowserItem : AbstractBrowserItem<ShellItem>, INotifyPropertyChanged
    {
        public string DisplayName => ShellItem.GetDisplayName(ShellItemDisplayString.NormalDisplay) ?? ShellItem.ToString();
        public readonly Shell32.PIDL PIDL;
        public ShellItem ShellItem;
        public SoftwareBitmapSource? SoftwareBitmap;
        private bool _treeViewItemIsSelected;

        public bool TreeViewItemIsSelected
        {
            get => _treeViewItemIsSelected;
            set
            {
                if (value == _treeViewItemIsSelected)
                {
                    return;
                }

                _treeViewItemIsSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// HasUnrealizedChildren checks for flag ´SFGAO_HASSUBFOLDER´.
        ///
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/shell/sfgao"/>
        /// The specified folders have subfolders. The SFGAO_HASSUBFOLDER attribute is only advisory and might be returned by Shell folder implementations even if they do not contain subfolders. Note, however, that the converse—failing to return SFGAO_HASSUBFOLDER—definitively states that the folder objects do not have subfolders.
        /// Returning SFGAO_HASSUBFOLDER is recommended whenever a significant amount of time is required to determine whether any subfolders exist. For example, the Shell always returns SFGAO_HASSUBFOLDER when a folder is located on a network drive.
        /// </summary>
        public bool HasUnrealizedChildren => ShellItem.Attributes.HasFlag(ShellItemAttribute.HasSubfolder);

        // TODO: Listen for ShellItem Property changes
        public BrowserItem(Shell32.PIDL pidl, bool? isFolder,
            List<AbstractBrowserItem<ShellItem>>? childItems = default) : base(isFolder, childItems ?? [])
        {
            PIDL = new Shell32.PIDL(pidl);
            ShellItem = new ShellItem(pidl);
            //SoftwareBitmap = ConfiguredTaskAwaitable GetStockIconBitmapSource()
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

}
