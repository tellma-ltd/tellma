using Microsoft.Extensions.Localization;
using System;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Model.Common;

namespace Tellma.Api.Behaviors
{
    /// <summary>
    /// An implementation of <see cref="IMetadataOverridesProvider"/> for the application entities.
    /// </summary>
    public class ApplicationMetadataOverridesProvider : IMetadataOverridesProvider
    {
        private readonly IStringLocalizer _localizer;
        private readonly DefinitionsForClient _definitions;
        private readonly SettingsForClient _settings;

        public DefinitionsForClient Definitions => _definitions;
        public SettingsForClient Settings => _settings;

        /// <summary>
        /// Creates a new instance of the <see cref="ApplicationMetadataOverridesProvider"/> class.
        /// </summary>
        /// <param name="localizer">Used to localize some error messages.</param>
        /// <param name="definitions">The <see cref="DefinitionsForClient"/> to use for creating the overrides.</param>
        /// <param name="settings">The <see cref="SettingsForClient"/> to use for creating the overrides.</param>
        public ApplicationMetadataOverridesProvider(IStringLocalizer localizer, DefinitionsForClient definitions, SettingsForClient settings)
        {
            _localizer = localizer;
            _definitions = definitions;
            _settings = settings;
        }

        public EntityMetadataOverrides EntityOverrides(TypeDescriptor typeDesc, int? definitionId, Func<string> singularDisplay, Func<string> pluralDisplay)
        {
            // Definition Override
            if (definitionId != null)
            {
                MasterDetailDefinitionForClient def = typeDesc.Name switch
                {
                    nameof(Document) => DocumentDefinition(definitionId.Value),
                    nameof(DocumentForSave) => DocumentDefinition(definitionId.Value),

                    nameof(Resource) => ResourceDefinition(definitionId.Value),
                    nameof(ResourceForSave) => ResourceDefinition(definitionId.Value),

                    nameof(Relation) => RelationDefinition(definitionId.Value),
                    nameof(RelationForSave) => RelationDefinition(definitionId.Value),

                    nameof(RelationAttachment) => null, // Weak entity, no point overriding anything here
                    nameof(RelationAttachmentForSave) => null, // Weak entity, no point overriding anything here

                    nameof(Lookup) => LookupDefinition(definitionId.Value),
                    nameof(LookupForSave) => LookupDefinition(definitionId.Value),

                    _ => throw new InvalidOperationException($"Bug: Unaccounted type in definition overrides {typeDesc.Name}")
                };

                if (def != null)
                {
                    var defaultSingular = singularDisplay;
                    singularDisplay = () => _settings.Localize(def.TitleSingular, def.TitleSingular2, def.TitleSingular3) ?? defaultSingular();

                    var defaultPlural = pluralDisplay;
                    pluralDisplay = () => _settings.Localize(def.TitlePlural, def.TitlePlural2, def.TitlePlural3) ?? defaultPlural();
                }

                return new EntityMetadataOverrides
                {
                    SingularDisplay = singularDisplay,
                    PluralDisplay = pluralDisplay,
                };
            }
            else
            {
                return null;
            }
        }

        public PropertyMetadataOverrides PropertyOverrides(TypeDescriptor typeDesc, int? definitionId, PropertyDescriptor propDesc, Func<string> display)
        {
            string propName = propDesc.Name;
            PropertyMetadataOverrides result = null;

            #region Definitions
            
            if (definitionId != null)
            {
                result = typeDesc.Name switch
                {
                    nameof(Document) => DocumentPropertyOverrides(definitionId.Value, propName, display),
                    nameof(DocumentForSave) => DocumentPropertyOverrides(definitionId.Value, propName, display),

                    nameof(Resource) => ResourcePropertyOverrides(definitionId.Value, propName, display),
                    nameof(ResourceForSave) => ResourcePropertyOverrides(definitionId.Value, propName, display),

                    nameof(Relation) => RelationPropertyOverrides(definitionId.Value, propName, display),
                    nameof(RelationForSave) => RelationPropertyOverrides(definitionId.Value, propName, display),

                    nameof(RelationAttachment) => RelationAttachmentPropertyOverrides(definitionId.Value, propName, display),
                    nameof(RelationAttachmentForSave) => RelationAttachmentPropertyOverrides(definitionId.Value, propName, display),

                    // No property on lookup has its metadata affected by definitions
                    nameof(Lookup) => null,
                    nameof(LookupForSave) => null,

                    _ => throw new InvalidOperationException($"Bug: Unaccounted type in definition overrides {typeDesc.Name}.")
                };
            }

            if (result != null && result.Display == null)
            {
                if (result.Display == null)
                {
                    // Property is hidden by the definition so no need to proceed
                    return result;
                }
                else
                {
                    // Capture the display from the definition
                    display = result.Display;
                }
            }

            #endregion

            #region  Multilingual Props
            // (e.g. Name, Name2 and Name3)

            // (1) Calculate isPrimary, isSecondary and isTernary
            bool isPrimary = typeDesc.HasProperty(propName + "2") && typeDesc.HasProperty(propName + "3");
            bool isSecondary = false;
            bool isTernary = false;
            if (!isPrimary)
            {
                var lastChar = propName[^1];
                if (lastChar == '2')
                {
                    string withoutLastChar = propName[0..^1];
                    isSecondary = typeDesc.HasProperty(withoutLastChar) && typeDesc.HasProperty(withoutLastChar + "3");
                }

                if (!isSecondary && lastChar == '3')
                {
                    string withoutLastChar = propName[0..^1];
                    isTernary = typeDesc.HasProperty(withoutLastChar) && typeDesc.HasProperty(withoutLastChar + "2");
                }
            }

            // (2) Use them
            if (isPrimary || isSecondary || isTernary)
            {
                bool secondaryEnabled = _settings.SecondaryLanguageId != null;
                bool ternaryEnabled = _settings.TernaryLanguageId != null;
                if (secondaryEnabled || ternaryEnabled)
                {
                    // Bi-lingual or tri-lingual company
                    var originalDisplay = display;
                    display =
                        isPrimary ? () => $"{originalDisplay()} ({_settings.PrimaryLanguageSymbol})" :
                        isSecondary ? (secondaryEnabled ? () => $"{originalDisplay()} ({_settings.SecondaryLanguageSymbol})" : (Func<string>)null) :
                        isTernary ? (ternaryEnabled ? () => $"{originalDisplay()} ({_settings.TernaryLanguageSymbol})" : (Func<string>)null) :
                        display;
                }
                else
                {
                    // uni-lingual company
                    if (isSecondary || isTernary || propName == nameof(MarkupTemplate.SupportsPrimaryLanguage))
                    {
                        display = null; // Remove those properties entirely
                    }
                }

                result ??= new PropertyMetadataOverrides();
                result.Display = display;
            }

            #endregion

            return result;
        }

        #region Definition Override

        /// <summary>
        /// Specifies any overriding changes to a <see cref="Resource"/> property metadata that come from the definition. 
        /// In particular: the property display, whether it's visible or not, whether it's required or not, 
        /// and - if it's a navigation property - the target definitionId
        /// </summary>
        private PropertyMetadataOverrides ResourcePropertyOverrides(int definitionId, string propName, Func<string> display)
        {
            // (1) Get the definition
            var def = ResourceDefinition(definitionId);

            // (2) Use it to calculate the overrides
            bool isRequired = false;
            switch (propName)
            {
                case nameof(Resource.Description):
                case nameof(Resource.Description2):
                case nameof(Resource.Description3):
                    display = PropertyDisplay(def.DescriptionVisibility, display);
                    isRequired = def.DescriptionVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Location):
                case nameof(Resource.LocationJson):
                case nameof(Resource.LocationWkb):
                    display = PropertyDisplay(def.LocationVisibility, display);
                    isRequired = def.LocationVisibility == Visibility.Required;
                    break;
                case nameof(Resource.FromDate):
                    display = PropertyDisplay(def.FromDateVisibility, def.FromDateLabel, def.FromDateLabel2, def.FromDateLabel3, display);
                    isRequired = def.FromDateVisibility == Visibility.Required;
                    break;
                case nameof(Resource.ToDate):
                    display = PropertyDisplay(def.ToDateVisibility, def.ToDateLabel, def.ToDateLabel2, def.ToDateLabel3, display);
                    isRequired = def.ToDateVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Decimal1):
                    display = PropertyDisplay(def.Decimal1Visibility, def.Decimal1Label, def.Decimal1Label2, def.Decimal1Label3, display);
                    isRequired = def.Decimal1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Decimal2):
                    display = PropertyDisplay(def.Decimal2Visibility, def.Decimal2Label, def.Decimal2Label2, def.Decimal2Label3, display);
                    isRequired = def.Decimal2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Int1):
                    display = PropertyDisplay(def.Int1Visibility, def.Int1Label, def.Int1Label2, def.Int1Label3, display);
                    isRequired = def.Int1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Int2):
                    display = PropertyDisplay(def.Int2Visibility, def.Int2Label, def.Int2Label2, def.Int2Label3, display);
                    isRequired = def.Int2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Text1):
                    display = PropertyDisplay(def.Text1Visibility, def.Text1Label, def.Text1Label2, def.Text1Label3, display);
                    isRequired = def.Text1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Text2):
                    display = PropertyDisplay(def.Text2Visibility, def.Text2Label, def.Text2Label2, def.Text2Label3, display);
                    isRequired = def.Text2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Currency):
                case nameof(Resource.CurrencyId):
                    display = PropertyDisplay(def.CurrencyVisibility, display);
                    isRequired = def.CurrencyVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Center):
                case nameof(Resource.CenterId):
                    display = PropertyDisplay(def.CenterVisibility, display);
                    isRequired = def.CenterVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup1):
                case nameof(Resource.Lookup1Id):
                    display = PropertyDisplay(def.Lookup1Visibility, def.Lookup1Label, def.Lookup1Label2, def.Lookup1Label3, display);
                    isRequired = def.Lookup1Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup2):
                case nameof(Resource.Lookup2Id):
                    display = PropertyDisplay(def.Lookup2Visibility, def.Lookup2Label, def.Lookup2Label2, def.Lookup2Label3, display);
                    isRequired = def.Lookup2Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup3):
                case nameof(Resource.Lookup3Id):
                    display = PropertyDisplay(def.Lookup3Visibility, def.Lookup3Label, def.Lookup3Label2, def.Lookup3Label3, display);
                    isRequired = def.Lookup3Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Lookup4):
                case nameof(Resource.Lookup4Id):
                    display = PropertyDisplay(def.Lookup4Visibility, def.Lookup4Label, def.Lookup4Label2, def.Lookup4Label3, display);
                    isRequired = def.Lookup4Visibility == Visibility.Required;
                    break;
                case nameof(Resource.Identifier):
                    display = PropertyDisplay(def.IdentifierVisibility, def.IdentifierLabel, def.IdentifierLabel2, def.IdentifierLabel3, display);
                    isRequired = def.IdentifierVisibility == Visibility.Required;
                    break;
                case nameof(Resource.VatRate):
                    display = PropertyDisplay(def.VatRateVisibility, display);
                    if (def.VatRateVisibility == null)
                    {
                        display = null;
                    }
                    else if (def.DefaultVatRate == null)
                    {
                        isRequired = true;
                    }
                    break;
                case nameof(Resource.Units):
                    if (def.UnitCardinality != Cardinality.Multiple)
                    {
                        display = null;
                    }
                    break;
                case nameof(Resource.ReorderLevel):
                    display = PropertyDisplay(def.ReorderLevelVisibility, display);
                    isRequired = def.ReorderLevelVisibility == Visibility.Required;
                    break;
                case nameof(Resource.EconomicOrderQuantity):
                    display = PropertyDisplay(def.EconomicOrderQuantityVisibility, display);
                    isRequired = def.EconomicOrderQuantityVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Unit):
                case nameof(Resource.UnitId):
                    if (def.UnitCardinality == null)
                    {
                        display = null;
                    }
                    else if (def.DefaultUnitId == null)
                    {
                        isRequired = true;
                    }
                    break;
                case nameof(Resource.UnitMass):
                    display = PropertyDisplay(def.UnitMassVisibility, display);
                    isRequired = def.UnitMassVisibility == Visibility.Required;
                    break;
                case nameof(Resource.UnitMassUnitId):
                case nameof(Resource.UnitMassUnit):
                    display = PropertyDisplay(def.UnitMassVisibility, display);
                    isRequired = def.UnitMassVisibility == Visibility.Required && def.DefaultUnitMassUnitId == null;
                    break;
                case nameof(Resource.MonetaryValue):
                    display = PropertyDisplay(def.MonetaryValueVisibility, display);
                    isRequired = def.MonetaryValueVisibility == Visibility.Required;
                    break;
                case nameof(Resource.Participant):
                case nameof(Resource.ParticipantId):
                    if (def.ParticipantDefinitionId != null && _definitions.Relations.TryGetValue(def.ParticipantDefinitionId.Value, out RelationDefinitionForClient relationDef))
                    {
                        // By default takes the singular title of the definition (e.g. "Customer")
                        display = PropertyDisplay(def.ParticipantVisibility, relationDef.TitleSingular, relationDef.TitleSingular2, relationDef.TitleSingular3, display);
                    }
                    else
                    {
                        display = PropertyDisplay(def.ParticipantVisibility, display);
                    }
                    isRequired = def.ParticipantVisibility == Visibility.Required;
                    break;
            }

            int? targetDefId = propName switch
            {
                nameof(Resource.Lookup1) => def.Lookup1DefinitionId,
                nameof(Resource.Lookup2) => def.Lookup2DefinitionId,
                nameof(Resource.Lookup3) => def.Lookup3DefinitionId,
                nameof(Resource.Lookup4) => def.Lookup4DefinitionId,
                //nameof(Resource.Lookup5) =>  def.Lookup5DefinitionId,
                nameof(Resource.Participant) => def.ParticipantDefinitionId,
                _ => null,
            };

            return new PropertyMetadataOverrides
            {
                Display = display,
                IsRequired = isRequired,
                DefinitionId = targetDefId,
            };
        }

        /// <summary>
        /// Specifies any overriding changes to a <see cref="RelationAttachment"/> property metadata that come from the definition. 
        /// In particular: the property display, whether it's visible or not, whether it's required or not, 
        /// and - if it's a navigation property - the target definitionId
        /// </summary>
        private PropertyMetadataOverrides RelationAttachmentPropertyOverrides(int definitionId, string propName, Func<string> display)
        {
            // (1) Get the definition
            var def = RelationDefinition(definitionId);

            // (2) Use it to calculate the overrides
            bool isRequired = false;
            int? targetDefId = null;

            switch (propName)
            {
                case nameof(RelationAttachment.CategoryId):
                case nameof(RelationAttachment.Category):
                    if (def.AttachmentsCategoryDefinitionId == null)
                    {
                        display = null;
                    }
                    else
                    {
                        isRequired = true;
                        targetDefId = def.AttachmentsCategoryDefinitionId;
                    }
                    break;
            }

            return new PropertyMetadataOverrides
            {
                Display = display,
                IsRequired = isRequired,
                DefinitionId = targetDefId,
            };
        }

        /// <summary>
        /// Specifies any overriding changes to a <see cref="Relation"/> property metadata that come from the definition. 
        /// In particular: the property display, whether it's visible or not, whether it's required or not, 
        /// and - if it's a navigation property - the target definitionId
        /// </summary>
        private PropertyMetadataOverrides RelationPropertyOverrides(int definitionId, string propName, Func<string> display)
        {
            // (1) Get the definition
            var def = RelationDefinition(definitionId);

            // (2) Use it to calculate the overrides
            bool isRequired = false;
            switch (propName)
            {
                // Common with Resources

                case nameof(Relation.Description):
                case nameof(Relation.Description2):
                case nameof(Relation.Description3):
                    display = PropertyDisplay(def.DescriptionVisibility, display);
                    isRequired = def.DescriptionVisibility == Visibility.Required;
                    break;
                case nameof(Relation.Location):
                case nameof(Relation.LocationJson):
                case nameof(Relation.LocationWkb):
                    display = PropertyDisplay(def.LocationVisibility, display);
                    isRequired = def.LocationVisibility == Visibility.Required;
                    break;
                case nameof(Relation.FromDate):
                    display = PropertyDisplay(def.FromDateVisibility, def.FromDateLabel, def.FromDateLabel2, def.FromDateLabel3, display);
                    isRequired = def.FromDateVisibility == Visibility.Required;
                    break;
                case nameof(Relation.ToDate):
                    display = PropertyDisplay(def.ToDateVisibility, def.ToDateLabel, def.ToDateLabel2, def.ToDateLabel3, display);
                    isRequired = def.ToDateVisibility == Visibility.Required;
                    break;
                case nameof(Relation.DateOfBirth):
                    display = PropertyDisplay(def.DateOfBirthVisibility, display);
                    isRequired = def.DateOfBirthVisibility == Visibility.Required;
                    break;
                case nameof(Relation.ContactEmail):
                    display = PropertyDisplay(def.ContactEmailVisibility, display);
                    isRequired = def.ContactEmailVisibility == Visibility.Required;
                    break;
                case nameof(Relation.ContactMobile):
                case nameof(Relation.NormalizedContactMobile):
                    display = PropertyDisplay(def.ContactMobileVisibility, display);
                    isRequired = def.ContactMobileVisibility == Visibility.Required;
                    break;
                case nameof(Relation.ContactAddress):
                    display = PropertyDisplay(def.ContactAddressVisibility, display);
                    isRequired = def.ContactAddressVisibility == Visibility.Required;
                    break;
                case nameof(Relation.Date1):
                    display = PropertyDisplay(def.Date1Visibility, def.Date1Label, def.Date1Label2, def.Date1Label3, display);
                    isRequired = def.Date1Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Date2):
                    display = PropertyDisplay(def.Date2Visibility, def.Date2Label, def.Date2Label2, def.Date2Label3, display);
                    isRequired = def.Date2Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Date3):
                    display = PropertyDisplay(def.Date3Visibility, def.Date3Label, def.Date3Label2, def.Date3Label3, display);
                    isRequired = def.Date3Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Date4):
                    display = PropertyDisplay(def.Date4Visibility, def.Date4Label, def.Date4Label2, def.Date4Label3, display);
                    isRequired = def.Date4Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Decimal1):
                    display = PropertyDisplay(def.Decimal1Visibility, def.Decimal1Label, def.Decimal1Label2, def.Decimal1Label3, display);
                    isRequired = def.Decimal1Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Decimal2):
                    display = PropertyDisplay(def.Decimal2Visibility, def.Decimal2Label, def.Decimal2Label2, def.Decimal2Label3, display);
                    isRequired = def.Decimal2Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Int1):
                    display = PropertyDisplay(def.Int1Visibility, def.Int1Label, def.Int1Label2, def.Int1Label3, display);
                    isRequired = def.Int1Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Int2):
                    display = PropertyDisplay(def.Int2Visibility, def.Int2Label, def.Int2Label2, def.Int2Label3, display);
                    isRequired = def.Int2Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Currency):
                case nameof(Relation.CurrencyId):
                    display = PropertyDisplay(def.CurrencyVisibility, display);
                    isRequired = def.CurrencyVisibility == Visibility.Required;
                    break;
                case nameof(Relation.Center):
                case nameof(Relation.CenterId):
                    display = PropertyDisplay(def.CenterVisibility, display);
                    isRequired = def.CenterVisibility == Visibility.Required;
                    break;
                case nameof(Relation.Lookup1):
                case nameof(Relation.Lookup1Id):
                    display = PropertyDisplay(def.Lookup1Visibility, def.Lookup1Label, def.Lookup1Label2, def.Lookup1Label3, display);
                    isRequired = def.Lookup1Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Lookup2):
                case nameof(Relation.Lookup2Id):
                    display = PropertyDisplay(def.Lookup2Visibility, def.Lookup2Label, def.Lookup2Label2, def.Lookup2Label3, display);
                    isRequired = def.Lookup2Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Lookup3):
                case nameof(Relation.Lookup3Id):
                    display = PropertyDisplay(def.Lookup3Visibility, def.Lookup3Label, def.Lookup3Label2, def.Lookup3Label3, display);
                    isRequired = def.Lookup3Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Lookup4):
                case nameof(Relation.Lookup4Id):
                    display = PropertyDisplay(def.Lookup4Visibility, def.Lookup4Label, def.Lookup4Label2, def.Lookup4Label3, display);
                    isRequired = def.Lookup4Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Lookup5):
                case nameof(Relation.Lookup5Id):
                    display = PropertyDisplay(def.Lookup5Visibility, def.Lookup5Label, def.Lookup5Label2, def.Lookup5Label3, display);
                    isRequired = def.Lookup5Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Lookup6):
                case nameof(Relation.Lookup6Id):
                    display = PropertyDisplay(def.Lookup6Visibility, def.Lookup6Label, def.Lookup6Label2, def.Lookup6Label3, display);
                    isRequired = def.Lookup6Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Lookup7):
                case nameof(Relation.Lookup7Id):
                    display = PropertyDisplay(def.Lookup7Visibility, def.Lookup7Label, def.Lookup7Label2, def.Lookup7Label3, display);
                    isRequired = def.Lookup7Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Lookup8):
                case nameof(Relation.Lookup8Id):
                    display = PropertyDisplay(def.Lookup8Visibility, def.Lookup8Label, def.Lookup8Label2, def.Lookup8Label3, display);
                    isRequired = def.Lookup8Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Text1):
                    display = PropertyDisplay(def.Text1Visibility, def.Text1Label, def.Text1Label2, def.Text1Label3, display);
                    isRequired = def.Text1Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Text2):
                    display = PropertyDisplay(def.Text2Visibility, def.Text2Label, def.Text2Label2, def.Text2Label3, display);
                    isRequired = def.Text2Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Text3):
                    display = PropertyDisplay(def.Text3Visibility, def.Text3Label, def.Text3Label2, def.Text3Label3, display);
                    isRequired = def.Text3Visibility == Visibility.Required;
                    break;
                case nameof(Relation.Text4):
                    display = PropertyDisplay(def.Text4Visibility, def.Text4Label, def.Text4Label2, def.Text4Label3, display);
                    isRequired = def.Text4Visibility == Visibility.Required;
                    break;
                case nameof(Relation.ExternalReference):
                    display = PropertyDisplay(def.ExternalReferenceVisibility, def.ExternalReferenceLabel, def.ExternalReferenceLabel2, def.ExternalReferenceLabel3, display);
                    isRequired = def.ExternalReferenceVisibility == Visibility.Required;
                    break;

                // Relations Only
                case nameof(Relation.Relation1):
                case nameof(Relation.Relation1Id):
                    display = PropertyDisplay(def.Relation1Visibility, def.Relation1Label, def.Relation1Label2, def.Relation1Label3, display);
                    isRequired = def.Relation1Visibility == Visibility.Required;
                    break;

                case nameof(Relation.AgentId):
                case nameof(Relation.Agent):
                    display = PropertyDisplay(def.AgentVisibility, display);
                    isRequired = def.AgentVisibility == Visibility.Required;
                    break;
                case nameof(Relation.TaxIdentificationNumber):
                    display = PropertyDisplay(def.TaxIdentificationNumberVisibility, display);
                    isRequired = def.TaxIdentificationNumberVisibility == Visibility.Required;
                    break;
                case nameof(Relation.BankAccountNumber):
                    display = PropertyDisplay(def.BankAccountNumberVisibility, display);
                    isRequired = def.BankAccountNumberVisibility == Visibility.Required;
                    break;
                case nameof(Relation.Users):
                    if (def.UserCardinality == null)
                    {
                        display = null;
                    }
                    break;
                case nameof(Relation.Attachments):
                    if (!(def.HasAttachments ?? false))
                    {
                        display = null;
                    }
                    break;
            }

            int? targetDefId = propName switch
            {
                nameof(Relation.Lookup1) => def.Lookup1DefinitionId,
                nameof(Relation.Lookup2) => def.Lookup2DefinitionId,
                nameof(Relation.Lookup3) => def.Lookup3DefinitionId,
                nameof(Relation.Lookup4) => def.Lookup4DefinitionId,
                nameof(Relation.Lookup5) => def.Lookup5DefinitionId,
                nameof(Relation.Lookup6) => def.Lookup6DefinitionId,
                nameof(Relation.Lookup7) => def.Lookup7DefinitionId,
                nameof(Relation.Lookup8) => def.Lookup8DefinitionId,
                nameof(Relation.Relation1) => def.Relation1DefinitionId,
                nameof(Relation.Attachments) => definitionId,
                _ => null,
            };

            return new PropertyMetadataOverrides
            {
                Display = display,
                IsRequired = isRequired,
                DefinitionId = targetDefId,
            };
        }

        /// <summary>
        /// Specifies any overriding changes to a <see cref="Document"/> property metadata that come from the definition. 
        /// In particular: the property display, whether it's visible or not, whether it's required or not, 
        /// and - if it's a navigation property - the target definitionId
        /// </summary>
        private PropertyMetadataOverrides DocumentPropertyOverrides(int definitionId, string propName, Func<string> display)
        {
            // (1) Get the definition
            var def = DocumentDefinition(definitionId);

            // (2) Use it to calculate the overrides
            bool isRequired = false;
            switch (propName)
            {
                case nameof(Document.PostingDate):
                    display = PropertyDisplay(def.PostingDateVisibility, def.PostingDateLabel, def.PostingDateLabel2, def.PostingDateLabel3, display);
                    isRequired = def.PostingDateRequiredState == 0 || def.PostingDateVisibility == Visibility.Required;
                    break;
                case nameof(Document.PostingDateIsCommon):
                    display = PropertyDisplay(def.PostingDateIsCommonVisibility, def.PostingDateLabel, def.PostingDateLabel2, def.PostingDateLabel3, display);
                    break;

                case nameof(Document.CenterId):
                case nameof(Document.Center):
                    display = PropertyDisplay(def.CenterVisibility, def.CenterLabel, def.CenterLabel2, def.CenterLabel3, display);
                    isRequired = (def.CenterRequiredState == 0 || def.CenterVisibility == Visibility.Required) && (_settings.SingleBusinessUnitId == null);
                    break;
                case nameof(Document.CenterIsCommon):
                    display = PropertyDisplay(def.CenterIsCommonVisibility, def.CenterLabel, def.CenterLabel2, def.CenterLabel3, display);
                    break;

                case nameof(Document.Memo):
                    display = PropertyDisplay(def.MemoVisibility, def.MemoLabel, def.MemoLabel2, def.MemoLabel3, display);
                    isRequired = def.MemoRequiredState == 0 || def.MemoVisibility == Visibility.Required;
                    break;
                case nameof(Document.MemoIsCommon):
                    display = PropertyDisplay(def.MemoIsCommonVisibility, def.MemoLabel, def.MemoLabel2, def.MemoLabel3, display);
                    break;

                case nameof(Document.CurrencyId):
                case nameof(Document.Currency):
                    display = PropertyDisplay(def.CurrencyVisibility, def.CurrencyLabel, def.CurrencyLabel2, def.CurrencyLabel3, display);
                    isRequired = def.CurrencyRequiredState == 0;
                    break;
                case nameof(Document.CurrencyIsCommon):
                    display = PropertyDisplay(def.CurrencyVisibility, def.CurrencyLabel, def.CurrencyLabel2, def.CurrencyLabel3, display);
                    break;

                case nameof(Document.CustodianId):
                case nameof(Document.Custodian):
                    display = PropertyDisplay(def.CustodianVisibility, def.CustodianLabel, def.CustodianLabel2, def.CustodianLabel3, display);
                    isRequired = def.CustodianRequiredState == 0;
                    break;
                case nameof(Document.CustodianIsCommon):
                    display = PropertyDisplay(def.CustodianVisibility, def.CustodianLabel, def.CustodianLabel2, def.CustodianLabel3, display);
                    break;

                case nameof(Document.RelationId):
                case nameof(Document.Relation):
                    display = PropertyDisplay(def.RelationVisibility, def.RelationLabel, def.RelationLabel2, def.RelationLabel3, display);
                    isRequired = def.RelationRequiredState == 0;
                    break;
                case nameof(Document.RelationIsCommon):
                    display = PropertyDisplay(def.RelationVisibility, def.RelationLabel, def.RelationLabel2, def.RelationLabel3, display);
                    break;

                case nameof(Document.ResourceId):
                case nameof(Document.Resource):
                    display = PropertyDisplay(def.ResourceVisibility, def.ResourceLabel, def.ResourceLabel2, def.ResourceLabel3, display);
                    isRequired = def.ResourceRequiredState == 0;
                    break;
                case nameof(Document.ResourceIsCommon):
                    display = PropertyDisplay(def.ResourceVisibility, def.ResourceLabel, def.ResourceLabel2, def.ResourceLabel3, display);
                    break;

                case nameof(Document.NotedRelationId):
                case nameof(Document.NotedRelation):
                    display = PropertyDisplay(def.NotedRelationVisibility, def.NotedRelationLabel, def.NotedRelationLabel2, def.NotedRelationLabel3, display);
                    isRequired = def.NotedRelationRequiredState == 0;
                    break;
                case nameof(Document.NotedRelationIsCommon):
                    display = PropertyDisplay(def.NotedRelationVisibility, def.NotedRelationLabel, def.NotedRelationLabel2, def.NotedRelationLabel3, display);
                    break;

                case nameof(Document.Quantity):
                    display = PropertyDisplay(def.QuantityVisibility, def.QuantityLabel, def.QuantityLabel2, def.QuantityLabel3, display);
                    isRequired = def.QuantityRequiredState == 0;
                    break;
                case nameof(Document.QuantityIsCommon):
                    display = PropertyDisplay(def.QuantityVisibility, def.QuantityLabel, def.QuantityLabel2, def.QuantityLabel3, display);
                    break;

                case nameof(Document.UnitId):
                case nameof(Document.Unit):
                    display = PropertyDisplay(def.UnitVisibility, def.UnitLabel, def.UnitLabel2, def.UnitLabel3, display);
                    isRequired = def.UnitRequiredState == 0;
                    break;
                case nameof(Document.UnitIsCommon):
                    display = PropertyDisplay(def.UnitVisibility, def.UnitLabel, def.UnitLabel2, def.UnitLabel3, display);
                    break;

                case nameof(Document.Time1):
                    display = PropertyDisplay(def.Time1Visibility, def.Time1Label, def.Time1Label2, def.Time1Label3, display);
                    isRequired = def.Time1RequiredState == 0;
                    break;
                case nameof(Document.Time1IsCommon):
                    display = PropertyDisplay(def.Time1Visibility, def.Time1Label, def.Time1Label2, def.Time1Label3, display);
                    break;

                case nameof(Document.Duration):
                    display = PropertyDisplay(def.DurationVisibility, def.DurationLabel, def.DurationLabel2, def.DurationLabel3, display);
                    isRequired = def.DurationRequiredState == 0;
                    break;
                case nameof(Document.DurationIsCommon):
                    display = PropertyDisplay(def.DurationVisibility, def.DurationLabel, def.DurationLabel2, def.DurationLabel3, display);
                    break;

                case nameof(Document.DurationUnitId):
                case nameof(Document.DurationUnit):
                    display = PropertyDisplay(def.DurationUnitVisibility, def.DurationUnitLabel, def.DurationUnitLabel2, def.DurationUnitLabel3, display);
                    isRequired = def.DurationUnitRequiredState == 0;
                    break;
                case nameof(Document.DurationUnitIsCommon):
                    display = PropertyDisplay(def.DurationUnitVisibility, def.DurationUnitLabel, def.DurationUnitLabel2, def.DurationUnitLabel3, display);
                    break;

                case nameof(Document.Time2):
                    display = PropertyDisplay(def.Time2Visibility, def.Time2Label, def.Time2Label2, def.Time2Label3, display);
                    isRequired = def.Time2RequiredState == 0;
                    break;
                case nameof(Document.Time2IsCommon):
                    display = PropertyDisplay(def.Time2Visibility, def.Time2Label, def.Time2Label2, def.Time2Label3, display);
                    break;

                case nameof(Document.ExternalReference):
                    display = PropertyDisplay(def.ExternalReferenceVisibility, def.ExternalReferenceLabel, def.ExternalReferenceLabel2, def.ExternalReferenceLabel3, display);
                    isRequired = def.ExternalReferenceRequiredState == 0;
                    break;
                case nameof(Document.ExternalReferenceIsCommon):
                    display = PropertyDisplay(def.ExternalReferenceVisibility, def.ExternalReferenceLabel, def.ExternalReferenceLabel2, def.ExternalReferenceLabel3, display);
                    break;

                case nameof(Document.ReferenceSourceId):
                case nameof(Document.ReferenceSource):
                    display = PropertyDisplay(def.ReferenceSourceVisibility, def.ReferenceSourceLabel, def.ReferenceSourceLabel2, def.ReferenceSourceLabel3, display);
                    isRequired = def.ReferenceSourceRequiredState == 0;
                    break;
                case nameof(Document.ReferenceSourceIsCommon):
                    display = PropertyDisplay(def.ReferenceSourceVisibility, def.ReferenceSourceLabel, def.ReferenceSourceLabel2, def.ReferenceSourceLabel3, display);
                    break;

                case nameof(Document.InternalReference):
                    display = PropertyDisplay(def.InternalReferenceVisibility, def.InternalReferenceLabel, def.InternalReferenceLabel2, def.InternalReferenceLabel3, display);
                    isRequired = def.InternalReferenceRequiredState == 0;
                    break;
                case nameof(Document.InternalReferenceIsCommon):
                    display = PropertyDisplay(def.InternalReferenceVisibility, def.InternalReferenceLabel, def.InternalReferenceLabel2, def.InternalReferenceLabel3, display);
                    break;

                case nameof(Document.Clearance):
                    display = PropertyDisplay(def.ClearanceVisibility, display);
                    isRequired = def.ClearanceVisibility == Visibility.Required;
                    break;
            }

            int? targetDefId = propName switch
            {
                nameof(Document.Custodian) => def.CustodianDefinitionIds.Count == 1 ? def.CustodianDefinitionIds[0] : null,
                nameof(Document.Relation) => def.RelationDefinitionIds.Count == 1 ? def.RelationDefinitionIds[0] : null,
                nameof(Document.Resource) => def.ResourceDefinitionIds.Count == 1 ? def.ResourceDefinitionIds[0] : null,
                nameof(Document.NotedRelation) => def.NotedRelationDefinitionIds.Count == 1 ? def.NotedRelationDefinitionIds[0] : null,
                _ => null,
            };

            return new PropertyMetadataOverrides
            {
                Display = display,
                IsRequired = isRequired,
                DefinitionId = targetDefId,
            };
        }

        #region Helper Methods

        /// <summary>
        /// Retrieve the <see cref="Resource"/> definition or throw an exception if none is found.
        /// </summary>
        /// <exception cref="ServiceException"></exception>
        private ResourceDefinitionForClient ResourceDefinition(int definitionId)
        {
            if (!_definitions.Resources.TryGetValue(definitionId, out ResourceDefinitionForClient def))
            {
                var msg = _localizer[$"Error_ResourceDefinition0CouldNotBeFound", definitionId];
                throw new ServiceException(msg);
            }

            return def;
        }

        /// <summary>
        /// Retrieve the <see cref="Relation"/> definition or throw an exception if none is found.
        /// </summary>
        /// <exception cref="ServiceException"></exception>
        private RelationDefinitionForClient RelationDefinition(int definitionId)
        {
            if (!_definitions.Relations.TryGetValue(definitionId, out RelationDefinitionForClient def))
            {
                var msg = _localizer[$"Error_RelationDefinition0CouldNotBeFound"];
                throw new ServiceException(msg);
            }

            return def;
        }

        /// <summary>
        /// Retrieve the <see cref="Lookup"/> definition or throw an exception if none is found.
        /// </summary>
        /// <exception cref="ServiceException"></exception>
        private LookupDefinitionForClient LookupDefinition(int definitionId)
        {
            if (!_definitions.Lookups.TryGetValue(definitionId, out LookupDefinitionForClient def))
            {
                var msg = _localizer[$"Error_LookupDefinition0CouldNotBeFound"];
                throw new ServiceException(msg);
            }

            return def;
        }

        /// <summary>
        /// Retrieve the <see cref="Document"/> definition or throw an exception if none is found.
        /// </summary>
        /// <exception cref="ServiceException"></exception>
        private DocumentDefinitionForClient DocumentDefinition(int definitionId)
        {
            if (!_definitions.Documents.TryGetValue(definitionId, out DocumentDefinitionForClient def))
            {
                var msg = _localizer[$"Error_DocumentDefinition0CouldNotBeFound"];
                throw new ServiceException(msg);
            }

            return def;
        }

        /// <summary>
        /// Returns null if the visibility or null, returns the same display function otherwise.
        /// </summary>
        private static Func<string> PropertyDisplay(
            string visibility,
            Func<string> defaultDisplay)
        {
            if (visibility == null)
            {
                return null;
            }
            else
            {
                return defaultDisplay;
            }
        }

        /// <summary>
        /// Returns null if the visibility is null, otherwise returns a new display function
        /// that relies on the supplied labels, and falls back to the default function if the
        /// labels are null.
        /// </summary>
        private Func<string> PropertyDisplay(
            string visibility,
            string label,
            string label2,
            string label3,
            Func<string> defaultDisplay)
        {
            if (visibility != null && defaultDisplay != null)
            {
                return () => _settings.Localize(label, label2, label3) ?? defaultDisplay();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns null if the visibility is false, otherwise returns a new display function
        /// that relies on the supplied labels, and falls back to the default function if the
        /// labels are null.
        /// </summary>
        private static Func<string> PropertyDisplay(
            bool isVisible,
            Func<string> defaultDisplay)
        {
            if (!isVisible)
            {
                return null;
            }
            else
            {
                return defaultDisplay;
            }
        }

        /// <summary>
        /// Returns null if the visibility is false, otherwise returns a new display function
        /// that relies on the supplied labels, and falls back to the default function if the
        /// labels are null.
        /// </summary>
        private Func<string> PropertyDisplay(
            bool isVisible,
            string label,
            string label2,
            string label3,
            Func<string> defaultDisplay)
        {
            if (isVisible && defaultDisplay != null)
            {
                return () => _settings.Localize(label, label2, label3) ?? defaultDisplay();
            }
            else
            {
                return null;
            }
        }

        #endregion

        #endregion
    }
}
