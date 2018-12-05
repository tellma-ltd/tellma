using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Shared
{
    /// <summary>
    /// Only for type-safety during development, i.e to prevent the silly 
    /// mistake of passing model entities as DTO entities
    /// </summary>
    public abstract class DtoForSaveBase
    {
    }
}
