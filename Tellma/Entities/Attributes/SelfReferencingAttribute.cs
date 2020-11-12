using System;

namespace Tellma.Entities
{
    /// <summary>
    /// For self referencing foreign keys like ParentId (FKs that reference the same entity type),
    /// there should always be an associated index property ParentIndex, to allow for referencing 
    /// another entity in the saved list (which may not have an ID yet) while saving such entities in bulk.
    /// Such self referencing foreign keys should be adorned with this attribute
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class SelfReferencingAttribute : Attribute
    {
        public SelfReferencingAttribute(string indexPropName)
        {
            IndexPropertyName = indexPropName ?? throw new ArgumentNullException(nameof(indexPropName));
        }

        public string IndexPropertyName { get; }
    }
}
