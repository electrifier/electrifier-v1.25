using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace electrifier.Controls.Services;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class Shel32NamespaceService
{
    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
