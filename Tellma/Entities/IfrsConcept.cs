using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "IfrsConcept", Plural = "IfrsConcepts")]
    public class IfrsConcept : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [Required]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcept_Documentation", Language = Language.Primary)]
        [AlwaysAccessible]
        public string Documentation { get; set; }

        [MultilingualDisplay(Name = "IfrsConcept_Documentation", Language = Language.Secondary)]
        [AlwaysAccessible]
        public string Documentation2 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcept_Documentation", Language = Language.Ternary)]
        [AlwaysAccessible]
        public string Documentation3 { get; set; }
    }
}
