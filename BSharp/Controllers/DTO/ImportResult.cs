using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class ImportResult
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public decimal Seconds { get; set; }
        public decimal AttributeValidationInCSharp { get; set; }
        public decimal ParsingToDtosForSave { get; set; }
        public decimal ValidatingAndSaving { get; set; }
    }
}
