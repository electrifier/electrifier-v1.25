using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using electrifier.Controls.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace electrifier.Controls.Services;

// TODO:
// WARN: Add Shell Log-Writer class, for logging Shell32 operations

/// <summary>
/// A service for interacting with the Shell32 namespace.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class Shel32NamespaceService
{
    static Shel32NamespaceService() => Shell32.FileIconInit(false);

    private static readonly Dictionary<Shell32.SHSTOCKICONID, SoftwareBitmapSource> StockIconDictionary = [];

    /// <summary>The default text that is displayed when an empty folder is shown</summary>
    [Category("Appearance"), DefaultValue("This folder is empty."), Description("The default text that is displayed when an empty folder is shown.")]
    public string EmptyFolderText { get; set; } = "This folder is empty.";
    /// <summary>The default text that is displayed when an empty group is shown</summary>
    [Category("Appearance"), DefaultValue("This group is empty."), Description("The default text that is displayed when an empty group is shown.")]
    public string EmptyGroupText { get; set; } = "This group is empty.";

    public ObservableCollection<ShellBrowserItem> Enumerate(ShellItem shellItem)
    {
        var result = new ObservableCollection<ShellBrowserItem>();
        // Enumerate child items
        var shFolder = new ShellFolder(shellItem);

        foreach (var item in shFolder.EnumerateChildren(FolderItemFilter.Storage))
        {
            // Create a new ShellBrowserItem for each child item
            var browserItem = BrowserItemFactory.FromPIDL(item.PIDL);
            result.Add(browserItem);
        }
        return result;
    }

    public static async Task<SoftwareBitmapSource> GetStockIconBitmapSource(Shell32.SHSTOCKICONID shStockIconId)
    {
        if (StockIconDictionary.TryGetValue(shStockIconId, out var source))
        {
            return source;
        }

        var siFlags = Shell32.SHGSI.SHGSI_LARGEICON | Shell32.SHGSI.SHGSI_ICON;
        var icninfo = Shell32.SHSTOCKICONINFO.Default;
        Shell32.SHGetStockIconInfo(shStockIconId, siFlags, ref icninfo)
            .ThrowIfFailed($"SHGetStockIconInfo({shStockIconId})");

        var hIcon = icninfo.hIcon;
        var icnHandle = hIcon.ToIcon();
        var bmpSource = GetWinUi3BitmapSourceFromIcon(icnHandle);        // TODO: bmpSource.SetBitmapAsync(softwareBitmap); // TODO: SetBitmapAsync(softwareBitmap);
        await bmpSource;
        var softBitmap = bmpSource.Result;

        if (softBitmap != null)
        {
            _ = StockIconDictionary.TryAdd(shStockIconId, softBitmap); // WARN: Is this thread safe?
            return softBitmap;
        }

        throw new ArgumentOutOfRangeException($"Can't get StockIcon for SHSTOCKICONID: {shStockIconId.ToString()}");
    }

    /// <summary>Get associated <seealso cref="SoftwareBitmapSource"/> for given <param name="bitmapIcon">Icon</param></summary>
    /// <remarks>TODO: INFO: Investigate <seealso href="https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.writeablebitmap?view=winrt-26100">uwp/api/windows.ui.xaml.media.imaging.WriteableBitmap (WARN: Links to UWP)</seealso></remarks>
    /// <param name="bitmapIcon">The <seealso cref="WinUIEx.Icon">Icon</seealso>.</param>
    /// <returns>Task&lt;SoftwareBitmapSource?&gt;</returns>
    public static async Task<SoftwareBitmapSource?> GetWinUi3BitmapSourceFromIcon(System.Drawing.Icon bitmapIcon)
    {
        ArgumentNullException.ThrowIfNull(bitmapIcon);

        return await GetWinUi3BitmapSourceFromGdiBitmap(bitmapIcon.ToBitmap());
    }

    /// <summary>Get associated <seealso cref="SoftwareBitmapSource"/> for given <param name="gdiBitmap">gdiBitmap</param></summary>
    /// <remarks>TODO: INFO: Investigate <seealso href="https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.writeablebitmap?view=winrt-26100">uwp/api/windows.ui.xaml.media.imaging.WriteableBitmap (WARN: Links to UWP)</seealso></remarks>
    /// <param name="gdiBitmap">The <seealso cref="Bitmap">GDI+ bitmap</seealso>.</param>
    /// <returns>Task&lt;SoftwareBitmapSource?&gt;</returns>
    public static async Task<SoftwareBitmapSource?> GetWinUi3BitmapSourceFromGdiBitmap(Bitmap gdiBitmap)
    {
        ArgumentNullException.ThrowIfNull(gdiBitmap);

        // get pixels as an array of bytes
        // TODO: See in vanara IconExtractor in terms of getting byte data array
        var data = gdiBitmap.LockBits(new Rectangle(0, 0, gdiBitmap.Width, gdiBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, gdiBitmap.PixelFormat);
        var bytes = new byte[data.Stride * data.Height];
        Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
        gdiBitmap.UnlockBits(data);

        // get WinRT SoftwareBitmap
        var softwareBitmap = new Windows.Graphics.Imaging.SoftwareBitmap(
            Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
            gdiBitmap.Width,
            gdiBitmap.Height,
            Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
        softwareBitmap.CopyFromBuffer(bytes.AsBuffer());

        // build WinUI3 SoftwareBitmapSource
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(softwareBitmap);
        return source;
    }

    private string GetDebuggerDisplay() => $"{nameof(Shel32NamespaceService)}>";
}
