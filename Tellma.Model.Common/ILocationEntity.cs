namespace Tellma.Model.Common
{
    public interface ILocationEntityForSave
    {
        /// <summary>
        /// The entity location in the GeoJSON Specification (RFC 7946).
        /// </summary>
        public string LocationJson { get; set; }

        /// <summary>
        /// The entity location in the Open Geospatial Consortium (OGC) Well-Known Binary (WKB) representation.
        /// This property is not visible to clients, it is auto populated from <see cref="LocationJson"/>
        /// </summary>
        public byte[] LocationWkb { get; set; }
    }

    public interface ILocationEntity : ILocationEntityForSave
    {
        /// <summary>
        /// The entity location in the SQL Server geography spatial datatype.
        /// This property is never populated, but can be used for filtering and ordering entity queries.
        /// </summary>
        public Geography Location { get; set; }
    }
}
