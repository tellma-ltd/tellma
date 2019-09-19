using System.Collections.Generic;

namespace BSharp.Controllers.Dto
{
    /// <summary>
    /// A structure that stores all definitions of a particular database
    /// </summary>
    public class DefinitionsForClient
    {
        /// <summary>
        /// Mapping from document definition Id to document definition
        /// </summary>
        public Dictionary<string, DocumentDefinitionForClient> Documents { get; set; }

        /// <summary>
        /// Mapping from line type Id to line type
        /// </summary>
        public Dictionary<string, LineTypeForClient> Lines { get; set; }

        /// <summary>
        /// Mapping from resource definition Id to resource definition
        /// </summary>
        public Dictionary<string, ResourceDefinitionForClient> Resources { get; set; }


        /// <summary>
        /// Mapping from resource lookup definition Id to resource definition
        /// </summary>
        public Dictionary<string, ResourceLookupDefinitionForClient> ResourceLookups { get; set; }
    }

    public abstract class DefinitionForClient
    {
        public string TitleSingular { get; set; }
        public string TitleSingular2 { get; set; }
        public string TitleSingular3 { get; set; }
        public string TitlePlural { get; set; }
        public string TitlePlural2 { get; set; }
        public string TitlePlural3 { get; set; }
        public string MainMenuSection { get; set; }
        public string MainMenuIcon { get; set; }
        public decimal MainMenuSortKey { get; set; }
    }

    public class DocumentDefinitionForClient : DefinitionForClient
    {
        // TODO
        public bool IsSourceDocument { get; internal set; }
        public string FinalState { get; internal set; }
    }

    public class LineTypeForClient // related entity for document definition
    {
        // TODO
    }

    public class ResourceDefinitionForClient : DefinitionForClient
    {
        public string MassUnit_Label { get; set; }
        public string MassUnit_Label2 { get; set; }
        public string MassUnit_Label3 { get; set; }
        public byte MassUnit_Visibility { get; set; }
        public string MassUnit_DefaultValue { get; set; }


        public string VolumeUnit_Label { get; set; }
        public string VolumeUnit_Label2 { get; set; }
        public string VolumeUnit_Label3 { get; set; }
        public byte VolumeUnit_Visibility { get; set; }
        public string VolumeUnit_DefaultValue { get; set; }


        public string AreaUnit_Label { get; set; }
        public string AreaUnit_Label2 { get; set; }
        public string AreaUnit_Label3 { get; set; }
        public byte AreaUnit_Visibility { get; set; }
        public string AreaUnit_DefaultValue { get; set; }


        public string LengthUnit_Label { get; set; }
        public string LengthUnit_Label2 { get; set; }
        public string LengthUnit_Label3 { get; set; }
        public byte LengthUnit_Visibility { get; set; }
        public string LengthUnit_DefaultValue { get; set; }


        public string TimeUnit_Label { get; set; }
        public string TimeUnit_Label2 { get; set; }
        public string TimeUnit_Label3 { get; set; }
        public byte TimeUnit_Visibility { get; set; }
        public string TimeUnit_DefaultValue { get; set; }


        public string CountUnit_Label { get; set; }
        public string CountUnit_Label2 { get; set; }
        public string CountUnit_Label3 { get; set; }
        public byte CountUnit_Visibility { get; set; }
        public string CountUnit_DefaultValue { get; set; }


        public string Memo_Label { get; set; }
        public string Memo_Label2 { get; set; }
        public string Memo_Label3 { get; set; }
        public byte Memo_Visibility { get; set; }
        public string Memo_DefaultValue { get; set; }

        public string CustomsReference_Label { get; set; }
        public string CustomsReference_Label2 { get; set; }
        public string CustomsReference_Label3 { get; set; }
        public byte CustomsReference_Visibility { get; set; }
        public string CustomsReference_DefaultValue { get; set; }


        // Resource Lookup 1
        public string ResourceLookup1_Label { get; set; }
        public string ResourceLookup1_Label2 { get; set; }
        public string ResourceLookup1_Label3 { get; set; }
        public byte ResourceLookup1_Visibility { get; set; } // 0, 1, 2 (not visible, visible, visible and required)
        public string ResourceLookup1_DefaultValue { get; set; }
        public string ResourceLookup1_DefinitionId { get; set; }

        // Resource Lookup 2
        public string ResourceLookup2_Label { get; set; }
        public string ResourceLookup2_Label2 { get; set; }
        public string ResourceLookup2_Label3 { get; set; }
        public byte ResourceLookup2_Visibility { get; set; }
        public string ResourceLookup2_DefaultValue { get; set; }
        public string ResourceLookup2_DefinitionId { get; set; }

        // Resource Lookup 3
        public string ResourceLookup3_Label { get; set; }
        public string ResourceLookup3_Label2 { get; set; }
        public string ResourceLookup3_Label3 { get; set; }
        public byte ResourceLookup3_Visibility { get; set; }
        public string ResourceLookup3_DefaultValue { get; set; }
        public string ResourceLookup3_DefinitionId { get; set; }

        // Resource Lookup 4
        public string ResourceLookup4_Label { get; set; }
        public string ResourceLookup4_Label2 { get; set; }
        public string ResourceLookup4_Label3 { get; set; }
        public byte ResourceLookup4_Visibility { get; set; }
        public string ResourceLookup4_DefaultValue { get; set; }
        public string ResourceLookup4_DefinitionId { get; set; }

    }

    public class ResourceLookupDefinitionForClient : DefinitionForClient
    {

    }
}
