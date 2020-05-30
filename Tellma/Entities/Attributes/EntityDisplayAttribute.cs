using System;

namespace Tellma.Entities
{
    /// <summary>
    /// Provides info on what a certain entity type is called
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class EntityDisplayAttribute : Attribute
    {
        /// <summary>
        /// The resource name of the singular display name of the adorned entity class
        /// </summary>
        public string Singular { get; set; }

        /// <summary>
        /// The resource name of the plural display name of the adorned entity class
        /// </summary>
        public string Plural { get; set; }
    }
}
