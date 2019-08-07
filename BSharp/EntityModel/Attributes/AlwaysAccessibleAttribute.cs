using System;

namespace BSharp.EntityModel
{
    /// <summary>
    /// Entity fields adorend with this attribute are always accessible regardless of user permissions, 
    /// this is usually applied to necessary fields such as Name, Code and IsActive
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class AlwaysAccessibleAttribute : Attribute
    {
    }
}
