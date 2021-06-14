using System;
using Tellma.Model.Application;

namespace Tellma.Controllers.ImportExport
{
    public class PropertyMappingInfo
    {
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
        /// Clones everything in <paramref name="original"/> except for <see cref="PropertyMappingInfo.Index"/>.
        /// </summary>
        /// <param name="original"></param>
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
        /// The mapped property metadata
        /// </summary>
        public PropertyMetadata Metadata { get; }

        /// <summary>
        /// The mapped property metadata for save
        /// </summary>
        public PropertyMetadata MetadataForSave { get; }

        public int ColumnNumber => Index + 1;

        public void SetIndexProperty(Entity entity, int index)
        {
            MetadataForSave.Descriptor.SetIndexProperty(entity, index);
        }

        /////////////////// These you can override

        public Func<string> Display { get; set; }

        public Func<Entity, Entity> GetTerminalEntityForSave { get; set; } // Takes base entity and returns terminal entity

        public Func<Entity, Entity> GetTerminalEntityForRead { get; set; }

        /// <summary>
        /// Adds a prefix to the auto-constructed select of the this property (useful for scenarios like document tab header properties)
        /// </summary>
        public string SelectPrefix { get; set; }

        /// <summary>
        /// The index in the CSV data row
        /// </summary>
        public int Index { get; set; }
    }
}
