using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    public interface IBlobServiceFactory
    {
        IBlobService Create();
    }
}
