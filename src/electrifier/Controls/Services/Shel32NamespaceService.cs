using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace electrifier.Controls.Services;

// TODO:
// WARN: Add Shell Log-Writer class, for logging Shell32 operations

/// <summary>
/// A service for interacting with the Shell32 namespace.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class Shel32NamespaceService
{


    /// <summary>
    /// Gets the debugger display.
    /// </summary>
    /// <returns>A <see cref="string" represnteation of this object for debugging purposes./></returns>
    private string GetDebuggerDisplay()
    {
        return ToString() ?? string.Empty;
    }
}
