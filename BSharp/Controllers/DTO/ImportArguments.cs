using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class ImportArguments
    {
        [ChoiceList("Insert", "Update", "Merge", "Delete")]
        public string Mode { get; set; } = "Insert"; // Default
    }
}
