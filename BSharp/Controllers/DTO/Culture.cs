using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    [StrongDto]
    public class CultureForSave : DtoKeyBase<string>
    {
    }

    public class Culture : CultureForSave
    {
        [BasicField]
        public string Name { get; set; }

        [BasicField]
        public string EnglishName { get; set; }

        [BasicField]
        public string NeutralName { get; set; }

        [BasicField]
        public bool IsActive { get; set; }
    }
}
