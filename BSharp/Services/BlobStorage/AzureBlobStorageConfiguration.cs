using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    public class AzureBlobStorageConfiguration
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }
    }
}
