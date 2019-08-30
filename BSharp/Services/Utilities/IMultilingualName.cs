using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Utilities
{
    // TODO: Delete
    /// <summary>
    /// This interface comes in handy when all we need is a name value that is culture dependent
    /// </summary>
    public interface IMultilingualName
    {
        string Name { get; set; }
        string Name2 { get; set; }
    }
}
