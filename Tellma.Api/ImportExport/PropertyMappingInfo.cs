using System;
using Tellma.Api.Metadata;
using Tellma.Model.Common;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// Maps a simple property to a column index in the imported/exported sheet.
    /// </summary>
    public class PropertyMappingInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMappingInfo"/> class.
        /// </summary>
        /// <param name="metadata">The <see cref="PropertyMetadata"/> of the mapped property on the read entity.</param>
        /// <param name="metadataForSave">The <see cref="PropertyMetadata"/> of the mapped property on the for-save entity.</param>
        public PropertyMappingInfo(PropertyMetadata metadata, PropertyMetadata metadataForSave)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            MetadataForSave = metadataForSave ?? throw new ArgumentNullException(nameof(metadataForSave));

            Display = metadata.Display;
            GetTerminalEntityForSave = e => e;
            GetTerminalEntityForRead = e => e;
            SelectPrefix = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMappingInfo"/> class. 
        /// Clones everything in <paramref name="original"/> except for <see cref="Index"/>.
        /// </summary>
        /// <param name="original">The <see cref="PropertyMappingInfo"/> to clone.</param>
        public PropertyMappingInfo(PropertyMappingInfo original) : this(original.Metadata, original.MetadataForSave)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            Display = original.Display;
            GetTerminalEntityForSave = original.GetTerminalEntityForSave;
            GetTerminalEntityForRead = original.GetTerminalEntityForRead;
            SelectPrefix = original.SelectPrefix;
        }

        /// <summary>
        /// The <see cref="PropertyMetadata"/> of the mapped property on the read entity.
        /// </summary>
        public PropertyMetadata Metadata { get; }

        /// <summary>
        /// The <see cref="PropertyMetadata"/> of the mapped property on the for-save entity.
        /// </summary>
        public PropertyMetadata MetadataForSave { get; }

        /// <summary>
        /// Syntacic sugar for <see cref="Index"/> + 1.
        /// </summary>
        public int ColumnNumber => Index + 1;

        /// <summary>
        /// For self referencing foreign keys like ParentId (FKs that reference the same entity type),
        /// there should always be an associated index property ParentIndex, to allow for referecing 
        /// another entity in the saved list (may not have an ID yet) while saving such entities in bulk.
        /// This method sets that property on <paramref name="entity"/> to the value <paramref name="index"/>.
        /// </summary>
        public void SetIndexProperty(Entity entity, int index) => MetadataForSave.Descriptor.SetIndexProperty(entity, index);

        /////////////////// These you can override

        /// <summary>
        /// The display label of this property.
        /// </summary>
        public Func<string> Display { get; set; }

        /// <summary>
        /// Takes the base entity being hydrated and returns the terminal entity where the property is found. 
        /// Useful when the same line in the imported file is hydrating several entities referenced by the same 
        /// base entity (e.g. Line and Entries).
        /// </summary>
        public Func<Entity, Entity> GetTerminalEntityForSave { get; set; }

        /// <summary>
        /// Takes the base entity being exported and returns the terminal entity where the property is found. 
        /// Useful when several entities referenced by the same base entity are being exported to the same line 
        /// (e.g. Line and Entries).
        /// </summary>
        public Func<Entity, Entity> GetTerminalEntityForRead { get; set; }

        /// <summary>
        /// Adds a prefix to the auto-constructed select of the this property (useful for 
        /// scenarios like document tab header properties).
        /// </summary>
        public string SelectPrefix { get; set; }

        /// <summary>
        /// The column index this property is mapped to in the imported data file (Whether CSV or Excel).
        /// </summary>
        public int Index { get; set; }
    }
}
