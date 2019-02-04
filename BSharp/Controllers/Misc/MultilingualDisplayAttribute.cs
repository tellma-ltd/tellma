using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MultilingualDisplayAttribute : Attribute
    {
        public string Name { get; set; }
        public Language Language { get; set; } = Language.Primary;
    }

    public enum Language
    {
        Primary, Secondary
    }
}
