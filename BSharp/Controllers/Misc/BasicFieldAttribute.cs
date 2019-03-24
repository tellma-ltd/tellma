using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// DTO fields adorend with this attribute are always included in the DTO in the response, 
    /// regardless of the requester preferences and regardless of user's permissions, this is usually applied
    /// to necessary fields such as Name, Code and IsActive
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BasicFieldAttribute : Attribute
    {
    }
}
