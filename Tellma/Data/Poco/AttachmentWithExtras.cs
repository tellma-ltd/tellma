using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Entities;

namespace Tellma.Data
{
    public class AttachmentWithExtras : AttachmentForSave
    {
        public long Size { get; set; }
        public string FileId { get; set; }
        public int DocumentIndex { get; set; }
    }
}
