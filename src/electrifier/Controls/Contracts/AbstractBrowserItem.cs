using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace electrifier.Controls.Contracts
{
    class AbstractBrowserItem
    {
    }

    /// <summary>Abstract base class ShellBrowserItem of Type <typeparam name="T"/>.</summary>
    /// <typeparam name="T">The derived Type of this abstract class.</typeparam>
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public abstract class AbstractBrowserItem<T> // TODO: IDisposable
    {
        public readonly List<AbstractBrowserItem<T>> ChildItems;
        public readonly bool? IsFolder;
        public readonly bool IsRootItem;

        /// <summary>Abstract base class ShellBrowserItem of Type <typeparam name="T"/>.</summary>
        /// <typeparam name="T">The derived Type of this abstract class.</typeparam>
        /// <param name="isFolder" >
        /// <value>true</value>
        /// Default: False.</param>
        /// <param name="childItems">Default: Create new empty List of child items <typeparam name="T">childItems</typeparam>.</param>
        protected AbstractBrowserItem(bool? isFolder, List<AbstractBrowserItem<T>>? childItems)
        {
            ChildItems = childItems ?? [];
            if (childItems is null)
            {
                IsFolder = isFolder;
            }
            else
            {
                IsFolder = true;    // We have child items, so we are a folder.
                EnumChildItems();   // Enumerate child items.
            }

            //todo: var propertyBag = new ArrayList<object owner, string key, object value>();
            //todo: var pb = new PropertyBag();
        }

        public virtual Task EnumChildItems() => Task.CompletedTask;

        //internal void async IconUpdate(int Index, SoftwareBitmapSource bmpSrc);
        //internal void async StockIconUpdate(STOCKICONID id, SoftwareBitmapSource bmpSrc);
        //internal void async ChildItemsIconUpdate();
        public new string ToString() => $"AbstractBrowserItem(<{typeof(T)}>(isFolder {IsFolder}, childItems {ChildItems})";
    }

}
