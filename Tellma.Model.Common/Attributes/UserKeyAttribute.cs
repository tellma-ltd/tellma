using System;

namespace Tellma.Model.Common
{
    /// <summary>
    /// The first <see cref="Entity"/> field adorend with this attribute is designated to be the default user key.
    /// The user key is used to refer to this entity when importing other entities that reference it, since
    /// the sarrogate key "Id", even though much faster, is not user friendly.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UserKeyAttribute : Attribute
    {
    }
}
