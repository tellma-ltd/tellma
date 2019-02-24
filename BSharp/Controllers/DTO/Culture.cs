using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    [CollectionName("Cultures")]
    public class CultureForSave : DtoKeyBase<string>
    {
    }

    public class Culture : CultureForSave
    {
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public string NeutralName { get; set; }
        public bool IsActive { get; set; }
    }
}
