using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Utilities.Blobs
{
    public interface IBlobServiceFactory
    {
        IBlobService Create();
    }
}
