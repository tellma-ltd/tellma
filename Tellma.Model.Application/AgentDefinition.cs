using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AgentDefinition", GroupName = "AgentDefinitions")]
    public class AgentDefinitionForSave<TReportDefinition> : EntityWithKey<int>
    {
        #region Title & Code

        [Display(Name = "Code")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Code { get; set; }

        [Display(Name = "TitleSingular")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string TitleSingular { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(255)]
        public string TitleSingular2 { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(255)]
        public string TitleSingular3 { get; set; }

        [Display(Name = "TitlePlural")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string TitlePlural { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(255)]
        public string TitlePlural2 { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(255)]
        public string TitlePlural3 { get; set; }

        #endregion

        #region Common with Resources

        [DefinitionLabelDisplay(Name = "Entity_Identifier")]
        [StringLength(50)]
        public string IdentifierLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Identifier")]
        [StringLength(50)]
        public string IdentifierLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Identifier")]
        [StringLength(50)]
        public string IdentifierLabel3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Identifier"), VisibilityChoiceList]
        [Required]
        public string IdentifierVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Currency"), VisibilityChoiceList]
        [Required]
        public string CurrencyVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Center"), VisibilityChoiceList]
        [Required]
        public string CenterVisibility { get; set; }

        [VisibilityDisplay(Name = "Image"), VisibilityChoiceList]
        [Required]
        public string ImageVisibility { get; set; }

        [VisibilityDisplay(Name = "Description"), VisibilityChoiceList]
        [Required]
        public string DescriptionVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Location"), VisibilityChoiceList]
        [Required]
        public string LocationVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate")]
        [StringLength(50)]
        public string FromDateLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate")]
        [StringLength(50)]
        public string FromDateLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate")]
        [StringLength(50)]
        public string FromDateLabel3 { get; set; }

        [VisibilityDisplay(Name = "Entity_FromDate"), VisibilityChoiceList]
        [Required]
        public string FromDateVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate")]
        [StringLength(50)]
        public string ToDateLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate")]
        [StringLength(50)]
        public string ToDateLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate")]
        [StringLength(50)]
        public string ToDateLabel3 { get; set; }

        [VisibilityDisplay(Name = "Entity_ToDate"), VisibilityChoiceList]
        [Required]
        public string ToDateVisibility { get; set; }

        [VisibilityDisplay(Name = "Agent_DateOfBirth"), VisibilityChoiceList]
        [Required]
        public string DateOfBirthVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_ContactEmail"), VisibilityChoiceList]
        [Required]
        public string ContactEmailVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_ContactMobile"), VisibilityChoiceList]
        [Required]
        public string ContactMobileVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_ContactAddress"), VisibilityChoiceList]
        [Required]
        public string ContactAddressVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date1")]
        [StringLength(50)]
        public string Date1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date1")]
        [StringLength(50)]
        public string Date1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date1")]
        [StringLength(50)]
        public string Date1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Date1"), VisibilityChoiceList]
        [Required]
        public string Date1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date2")]
        [StringLength(50)]
        public string Date2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date2")]
        [StringLength(50)]
        public string Date2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date2")]
        [StringLength(50)]
        public string Date2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Date2"), VisibilityChoiceList]
        [Required]
        public string Date2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date3")]
        [StringLength(50)]
        public string Date3Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date3")]
        [StringLength(50)]
        public string Date3Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date3")]
        [StringLength(50)]
        public string Date3Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Date3"), VisibilityChoiceList]
        [Required]
        public string Date3Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date4")]
        [StringLength(50)]
        public string Date4Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date4")]
        [StringLength(50)]
        public string Date4Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date4")]
        [StringLength(50)]
        public string Date4Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Date4"), VisibilityChoiceList]
        [Required]
        public string Date4Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1")]
        [StringLength(50)]
        public string Decimal1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1")]
        [StringLength(50)]
        public string Decimal1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1")]
        [StringLength(50)]
        public string Decimal1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Decimal1"), VisibilityChoiceList]
        [Required]
        public string Decimal1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2")]
        [StringLength(50)]
        public string Decimal2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2")]
        [StringLength(50)]
        public string Decimal2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2")]
        [StringLength(50)]
        public string Decimal2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Decimal2"), VisibilityChoiceList]
        [Required]
        public string Decimal2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1")]
        [StringLength(50)]
        public string Int1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1")]
        [StringLength(50)]
        public string Int1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1")]
        [StringLength(50)]
        public string Int1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Int1"), VisibilityChoiceList]
        [Required]
        public string Int1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2")]
        [StringLength(50)]
        public string Int2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2")]
        [StringLength(50)]
        public string Int2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2")]
        [StringLength(50)]
        public string Int2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Int2"), VisibilityChoiceList]
        [Required]
        public string Int2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1")]
        [StringLength(50)]
        public string Lookup1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1")]
        [StringLength(50)]
        public string Lookup1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1")]
        [StringLength(50)]
        public string Lookup1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup1"), VisibilityChoiceList]
        [Required]
        public string Lookup1Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup1")]
        public int? Lookup1DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2")]
        [StringLength(50)]
        public string Lookup2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2")]
        [StringLength(50)]
        public string Lookup2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2")]
        [StringLength(50)]
        public string Lookup2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup2"), VisibilityChoiceList]
        [Required]
        public string Lookup2Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup2")]
        public int? Lookup2DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup3")]
        [StringLength(50)]
        public string Lookup3Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup3")]
        [StringLength(50)]
        public string Lookup3Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup3")]
        [StringLength(50)]
        public string Lookup3Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup3"), VisibilityChoiceList]
        [Required]
        public string Lookup3Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup3")]
        public int? Lookup3DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4")]
        [StringLength(50)]
        public string Lookup4Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4")]
        [StringLength(50)]
        public string Lookup4Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4")]
        [StringLength(50)]
        public string Lookup4Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup4"), VisibilityChoiceList]
        [Required]
        public string Lookup4Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup4")]
        public int? Lookup4DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup5")]
        [StringLength(50)]
        public string Lookup5Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup5")]
        [StringLength(50)]
        public string Lookup5Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup5")]
        [StringLength(50)]
        public string Lookup5Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup5"), VisibilityChoiceList]
        [Required]
        public string Lookup5Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup5")]
        public int? Lookup5DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup6")]
        [StringLength(50)]
        public string Lookup6Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup6")]
        [StringLength(50)]
        public string Lookup6Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup6")]
        [StringLength(50)]
        public string Lookup6Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup6"), VisibilityChoiceList]
        [Required]
        public string Lookup6Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup6")]
        public int? Lookup6DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup7")]
        [StringLength(50)]
        public string Lookup7Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup7")]
        [StringLength(50)]
        public string Lookup7Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup7")]
        [StringLength(50)]
        public string Lookup7Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup7"), VisibilityChoiceList]
        [Required]
        public string Lookup7Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup7")]
        public int? Lookup7DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup8")]
        [StringLength(50)]
        public string Lookup8Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup8")]
        [StringLength(50)]
        public string Lookup8Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup8")]
        [StringLength(50)]
        public string Lookup8Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup8"), VisibilityChoiceList]
        [Required]
        public string Lookup8Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup8")]
        public int? Lookup8DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1")]
        [StringLength(50)]
        public string Text1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1")]
        [StringLength(50)]
        public string Text1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1")]
        [StringLength(50)]
        public string Text1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text1"), VisibilityChoiceList]
        [Required]
        public string Text1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text2"), VisibilityChoiceList]
        [Required]
        public string Text2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text3")]
        [StringLength(50)]
        public string Text3Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text3")]
        [StringLength(50)]
        public string Text3Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text3")]
        [StringLength(50)]
        public string Text3Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text3"), VisibilityChoiceList]
        [Required]
        public string Text3Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text4")]
        [StringLength(50)]
        public string Text4Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text4")]
        [StringLength(50)]
        public string Text4Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text4")]
        [StringLength(50)]
        public string Text4Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text4"), VisibilityChoiceList]
        [Required]
        public string Text4Visibility { get; set; }

        [Required]
        [Display(Name = "Definition_HasAddress")]
        public bool HasAddress { get; set; }

        [Display(Name = "Definition_PreprocessScript")]
        public string PreprocessScript { get; set; }

        [Display(Name = "Definition_ValidateScript")]
        public string ValidateScript { get; set; }

        #endregion

        #region Agent Only

        [DefinitionLabelDisplay(Name = "Entity_Agent1")]
        [StringLength(50)]
        public string Agent1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Agent1")]
        [StringLength(50)]
        public string Agent1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Agent1")]
        [StringLength(50)]
        public string Agent1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Agent1"), VisibilityChoiceList]
        [Required]
        public string Agent1Visibility { get; set; }

        [NotMapped]
        public int? Agent1DefinitionIndex { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Agent1")]
        [SelfReferencing(nameof(Agent1DefinitionIndex))]
        public int? Agent1DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Agent2")]
        [StringLength(50)]
        public string Agent2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Agent2")]
        [StringLength(50)]
        public string Agent2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Agent2")]
        [StringLength(50)]
        public string Agent2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Agent2"), VisibilityChoiceList]
        [Required]
        public string Agent2Visibility { get; set; }

        [NotMapped]
        public int? Agent2DefinitionIndex { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Agent2")]
        [SelfReferencing(nameof(Agent2DefinitionIndex))]
        public int? Agent2DefinitionId { get; set; }

        [VisibilityDisplay(Name = "Agent_TaxIdentificationNumber"), VisibilityChoiceList]
        [Required]
        public string TaxIdentificationNumberVisibility { get; set; }

        [VisibilityDisplay(Name = "Agent_BankAccountNumber"), VisibilityChoiceList]
        [Required]
        public string BankAccountNumberVisibility { get; set; }

        [VisibilityDisplay(Name = "Agent_ExternalReference"), VisibilityChoiceList]
        [Required]
        public string ExternalReferenceVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Agent_ExternalReference")]
        [StringLength(50)]
        public string ExternalReferenceLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Agent_ExternalReference")]
        [StringLength(50)]
        public string ExternalReferenceLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Agent_ExternalReference")]
        [StringLength(50)]
        public string ExternalReferenceLabel3 { get; set; }

        [Display(Name = "AgentDefinition_UserCardinality")]
        [Required]
        [ChoiceList(new object[] { "None", "Single", "Multiple" },
            new string[] { "Cardinality_None", "Cardinality_Single", "Cardinality_Multiple" })]
        public string UserCardinality { get; set; }

        [Display(Name = "Definition_HasAttachments")]
        [Required]
        public bool? HasAttachments { get; set; }

        [Display(Name = "Definition_AttachmentsCategoryDefinition")]
        public int? AttachmentsCategoryDefinitionId { get; set; }

        #endregion

        #region Main Menu

        [Display(Name = "MainMenuIcon")]
        [StringLength(50)]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSection")]
        [StringLength(50)]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuSortKey")]
        public decimal? MainMenuSortKey { get; set; }

        #endregion

        [Display(Name = "Definition_ReportDefinitions")]
        [ForeignKey(nameof(AgentDefinitionReportDefinition.AgentDefinitionId))]
        public List<TReportDefinition> ReportDefinitions { get; set; }
    }

    public class AgentDefinitionForSave : AgentDefinitionForSave<AgentDefinitionReportDefinitionForSave>
    {
    }

    public class AgentDefinition : AgentDefinitionForSave<AgentDefinitionReportDefinition>
    {
        [Display(Name = "Definition_State")]
        [Required]
        [ChoiceList(new object[] {
                DefStates.Hidden,
                DefStates.Testing,
                DefStates.Visible,
                DefStates.Archived },
            new string[] {
                DefStateNames.Hidden,
                DefStateNames.Testing,
                DefStateNames.Visible,
                DefStateNames.Archived })]
        public string State { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? SavedAt { get; set; }

        // For Query

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup1")]
        [ForeignKey(nameof(Lookup1DefinitionId))]
        public LookupDefinition Lookup1Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup2")]
        [ForeignKey(nameof(Lookup2DefinitionId))]
        public LookupDefinition Lookup2Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup3")]
        [ForeignKey(nameof(Lookup3DefinitionId))]
        public LookupDefinition Lookup3Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup4")]
        [ForeignKey(nameof(Lookup4DefinitionId))]
        public LookupDefinition Lookup4Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup5")]
        [ForeignKey(nameof(Lookup5DefinitionId))]
        public LookupDefinition Lookup5Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup6")]
        [ForeignKey(nameof(Lookup6DefinitionId))]
        public LookupDefinition Lookup6Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup7")]
        [ForeignKey(nameof(Lookup7DefinitionId))]
        public LookupDefinition Lookup7Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup8")]
        [ForeignKey(nameof(Lookup8DefinitionId))]
        public LookupDefinition Lookup8Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Agent1")]
        [ForeignKey(nameof(Agent1DefinitionId))]
        public AgentDefinition Agent1Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Agent2")]
        [ForeignKey(nameof(Agent2DefinitionId))]
        public AgentDefinition Agent2Definition { get; set; }

        [Display(Name = "Definition_AttachmentsCategoryDefinition")]
        [ForeignKey(nameof(AttachmentsCategoryDefinitionId))]
        public LookupDefinition AttachmentsCategoryDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
