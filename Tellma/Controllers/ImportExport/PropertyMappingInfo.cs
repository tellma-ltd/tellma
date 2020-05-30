namespace Tellma.Controllers.ImportExport
{
    public class PropertyMappingInfo
    {
        /// <summary>
        /// The index in the CSV data row
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The mapped property metadata
        /// </summary>
        public PropertyMetadata Metadata { get; set; }

        /// <summary>
        /// Set to true for the placeholder property #
        /// </summary>
        public bool Ignore { get; set; }

        public int ColumnNumber => Index + 1;
    }
}
