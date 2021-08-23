using System;

namespace Tellma.Api.Metadata
{
    public class EntityMetadataOverrides
    {
        /// <summary>
        /// Overrides the default singualr display function of the entity.
        /// </summary>
        public Func<string> SingularDisplay { get; set; }

        /// <summary>
        /// Overrides the default plural display function of the entity.
        /// </summary>
        public Func<string> PluralDisplay { get; set; }
    }
}
