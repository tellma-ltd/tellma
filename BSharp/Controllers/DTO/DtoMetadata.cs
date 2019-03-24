using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class DtoMetadata : Dictionary<string, FieldMetadata>
    {
        public const string ALL_FIELDS_KEYWORD = "AllFields";
    }

    public enum FieldMetadata
    {
        Restricted = 1,
        Loaded = 2
    }
}
