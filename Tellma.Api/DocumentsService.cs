using Azure;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Api.Metadata;
using Tellma.Api.Notifications;
using Tellma.Integration.Zatca;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Blobs;
using Tellma.Utilities.Calendars;
using Tellma.Utilities.Common;
using Tellma.Utilities.Email;
using Tellma.Utilities.Sms;

namespace Tellma.Api
{
    public class DocumentsService : CrudServiceBase<DocumentForSave, Document, int, DocumentsResult, DocumentResult>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer<Strings> _localizer;
        private readonly DataParser _parser;
        private readonly IBlobService _blobService;
        private readonly IClientProxy _clientProxy;
        private readonly NotificationsQueue _notificationsQueue;
        private readonly ZatcaService _zatcaService;
        private readonly MetadataProvider _metadata;

        /// <summary>
        /// This <see cref="ExpressionSelect"/> replaces any occurrece of the shorthand 
        /// "$Details" in the query select parameter. We cache it here since it is rather big.
        /// </summary>
        private static readonly ExpressionSelect _detailsSelectExpression =
            ExpressionSelect.Parse(string.Join(',', DocDetails.DocumentPaths()));

        // Used across multiple methods
        private List<(string, byte[])> _blobsToSave;
        private List<string> _blobsToDelete;
        private List<InboxStatus> _notificationInfos;

        public DocumentsService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps,
            IBlobService blobService,
            IClientProxy clientProxy,
            NotificationsQueue notificationsQueue,
            ZatcaService zatcaService) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
            _parser = deps.Parser;
            _blobService = blobService;
            _clientProxy = clientProxy;
            _notificationsQueue = notificationsQueue;
            _zatcaService = zatcaService;
            _metadata = deps.Metadata;
        }

        #region Default Tab Entry

        /// <summary>
        /// This is used in preprocessing and validation when a tab entry is null.
        /// </summary>
        private static readonly DocumentLineDefinitionEntryForSave DefaultTabEntryForSave = MakeDefaultTabEntryForSave();

        /// <summary>
        /// This is used in preprocessing and validation when a tab entry is null.
        /// </summary>
        private static readonly DocumentLineDefinitionEntry DefaultTabEntry = MakeDefaultTabEntry();

        /// <summary>
        /// This is used in preprocessing and validation when a tab entry is null.
        /// </summary>
        private static DocumentLineDefinitionEntryForSave MakeDefaultTabEntryForSave() => new()
        {
            PostingDateIsCommon = true,
            MemoIsCommon = true,
            CurrencyIsCommon = true,
            CenterIsCommon = true,

            AgentIsCommon = true,
            ResourceIsCommon = true,
            NotedAgentIsCommon = true,
            NotedResourceIsCommon = true,

            QuantityIsCommon = true,
            UnitIsCommon = true,
            Time1IsCommon = true,
            DurationIsCommon = true,
            DurationUnitIsCommon = true,
            Time2IsCommon = true,
            NotedDateIsCommon = true,

            ExternalReferenceIsCommon = true,
            ReferenceSourceIsCommon = true,
            InternalReferenceIsCommon = true,
        };

        /// <summary>
        /// This is used in preprocessing and validation when a tab entry is null.
        /// </summary>
        private static DocumentLineDefinitionEntry MakeDefaultTabEntry()
        {
            var entry = new DocumentLineDefinitionEntry
            {
                PostingDateIsCommon = true,
                MemoIsCommon = true,
                CurrencyIsCommon = true,
                CenterIsCommon = true,

                AgentIsCommon = true,
                ResourceIsCommon = true,
                NotedAgentIsCommon = true,
                NotedResourceIsCommon = true,

                QuantityIsCommon = true,
                UnitIsCommon = true,
                Time1IsCommon = true,
                DurationIsCommon = true,
                DurationUnitIsCommon = true,
                Time2IsCommon = true,
                NotedDateIsCommon = true,

                ExternalReferenceIsCommon = true,
                ReferenceSourceIsCommon = true,
                InternalReferenceIsCommon = true,
            };

            foreach (var prop in TypeDescriptor.Get<DocumentLineDefinitionEntry>().Properties)
            {
                entry.EntityMetadata[prop.Name] = FieldMetadata.Loaded;
            }

            return entry;
        }

        /// <summary>
        /// Cached.
        /// </summary>
        private static readonly TypeDescriptor lineDefEntryForSaveDescriptor = TypeDescriptor.Get<DocumentLineDefinitionEntryForSave>();

        /// <summary>
        /// Checks if the supplied DocumentLineDefinitionEntryForSave is equivalent 
        /// to the default one (ignoring Id, LineDefinitionId and EntryIndex properties).
        /// </summary>
        private static bool EqualsDefaultTabEntry(DocumentLineDefinitionEntryForSave tabEntry)
        {
            return lineDefEntryForSaveDescriptor.Properties.All(p =>
            {
                switch (p.Name)
                {
                    case nameof(DocumentLineDefinitionEntryForSave.Id):
                    case nameof(DocumentLineDefinitionEntryForSave.LineDefinitionId):
                    case nameof(DocumentLineDefinitionEntryForSave.EntryIndex):
                        return true; // Those properties don't matter for the comparison
                    default:
                        // Everything else must match
                        var expected = p.GetValue(DefaultTabEntryForSave);
                        var actual = p.GetValue(tabEntry);

                        return (expected == null && actual == null) ||
                            (expected != null && actual != null && expected.Equals(actual));
                }
            });
        }

        #endregion

        #region Include Required Signature

        public DocumentsService SetIncludeRequiredSignatures(bool val)
        {
            IncludeRequiredSignatures = val;
            return this;
        }

        private bool IncludeRequiredSignatures { get; set; }

        #endregion

        protected override string View => $"documents/{DefinitionId}";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        /// <summary>
        /// The current <see cref="DefinitionId"/>, if null throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private new int DefinitionId => base.DefinitionId ??
            throw new InvalidOperationException($"DefinitionId was not set in {nameof(DocumentsService)}.");

        /// <summary>
        /// The current TenantId.
        /// </summary>
        private new int TenantId => _behavior.TenantId;

        /// <summary>
        /// Helper method for retrieving the <see cref="DocumentDefinitionForClient"/> 
        /// that corresponds to the current <see cref="DefinitionId"/>.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        private async Task<DocumentDefinitionForClient> Definition(CancellationToken cancellation = default)
        {
            var defs = await _behavior.Definitions(cancellation);
            var docDef = defs.Documents.GetValueOrDefault(DefinitionId) ??
                throw new InvalidOperationException($"Document definition with Id = {DefinitionId} is missing from the cache.");

            return docDef;
        }

        /// <summary>
        /// Helper method for retrieving the <see cref="LineDefinitionForClient"/> 
        /// that corresponds to a certain <paramref name="lineDefId"/>. 
        /// Throws an exception if one is not found.
        /// </summary>
        /// <param name="lineDefId">The id of the line definition to retrieve.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<LineDefinitionForClient> LineDefinition(int lineDefId, CancellationToken cancellation = default)
        {
            var defs = await _behavior.Definitions(cancellation);
            var lineDef = defs.Lines.GetValueOrDefault(lineDefId) ??
                throw new InvalidOperationException($"Line Definition with Id = {lineDefId} is missing from the cache");

            return lineDef;
        }

        #region State & Workflow

        public async Task<DocumentResult> UpdateAssignment(UpdateAssignmentArguments args)
        {
            await Initialize();

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            (InboxStatusOutput output, int documentId) = await _behavior.Repository
                .Documents__UpdateAssignment(
                assignmentId: args.Id,
                comment: args.Comment,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            var getbyIdArgs = new GetByIdArguments { Select = args.Select, Expand = args.Expand };
            var result = args.ReturnEntities ?? false ? await GetById(documentId, getbyIdArgs, cancellation: default) :
                DocumentResult.Empty();

            _clientProxy.UpdateInboxStatuses(TenantId, output.InboxStatuses);

            trx.Complete();
            return result;
        }

        public async Task<DocumentsResult> Assign(List<int> ids, AssignArguments args)
        {
            await Initialize();

            if (ids == null || !ids.Any())
            {
                throw new ServiceException("No ids were supplied.");
            }

            // Check user permissions
            var action = PermissionActions.Read;
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // C# Validation 
            if (args.AssigneeId == 0)
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, nameof(args.AssigneeId)]);
            }

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            AssignOutput output = await _behavior.Repository
                .Documents__Assign(
                ids: ids,
                assigneeId: args.AssigneeId,
                comment: args.Comment,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = args.ReturnEntities ?? false ?
                await GetByIds(ids, args, action, cancellation: default) :
                DocumentsResult.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            // Actual Assignment
            var inboxStatuses = output.InboxStatuses;
            var assigneeInfo = output.AssigneeInfo;
            var serial = output.DocumentSerial;

            // Notify relevant parties
            _clientProxy.UpdateInboxStatuses(TenantId, inboxStatuses);

            // If assignee is not the same user, notify them by Email/SMS/Push
            if (UserId != args.AssigneeId)
            {
                var settings = await _behavior.Settings();
                var userSettings = await _behavior.UserSettings();
                var def = await Definition();
                var preferredLang = assigneeInfo.PreferredLanguage ?? settings.PrimaryLanguageId;

                var emails = new List<EmailToSend>();
                var smses = new List<SmsToSend>();
                var pushes = new List<PushToSend>();

                var culture = CultureInfo.GetCultureInfo(preferredLang);
                using (var _ = new CultureScope(culture))
                {
                    var singularTitle = settings.Localize(def.TitleSingular, def.TitleSingular2, def.TitleSingular3);
                    var pluralTitle = settings.Localize(def.TitlePlural, def.TitlePlural2, def.TitlePlural3);
                    var senderName = settings.Localize(userSettings.Name, userSettings.Name2, userSettings.Name3);

                    var notifyArgs = new NotifyDocumentAssignmentArguments
                    {
                        DefinitionId = DefinitionId,
                        SingularTitle = singularTitle,
                        PluralTitle = pluralTitle,
                        DocumentCount = ids.Count,
                        DocumentId = ids.First(),
                        FormattedSerial = FormatSerial(serial, def.Prefix, def.CodeWidth),
                        SenderName = senderName,
                        SenderComment = args.Comment
                    };

                    if ((assigneeInfo.EmailNewInboxItem ?? false) && !string.IsNullOrWhiteSpace(assigneeInfo.ContactEmail))
                    {
                        emails.Add(_clientProxy.MakeDocumentAssignmentEmail(TenantId, assigneeInfo.ContactEmail, notifyArgs));
                    }

                    if ((assigneeInfo.SmsNewInboxItem ?? false) && !string.IsNullOrWhiteSpace(assigneeInfo.ContactMobile))
                    {
                        smses.Add(_clientProxy.MakeDocumentAssignmentSms(TenantId, assigneeInfo.ContactMobile, notifyArgs));
                    }

                    if (assigneeInfo.PushNewInboxItem ?? false)
                    {
                        pushes.Add(_clientProxy.MakeDocumentAssignmentPush(TenantId, notifyArgs));
                    }
                }

                await _notificationsQueue.Enqueue(TenantId, emails, smses, pushes);
            }

            trx.Complete();
            return result;
        }

        public async Task<DocumentsResult> SignLines(List<int> lineIds, SignArguments args)
        {
            await Initialize();
            var def = await Definition();
            var returnEntities = args.ReturnEntities ?? false;

            // C# Validation 
            if (string.IsNullOrWhiteSpace(args.RuleType))
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, nameof(args.RuleType)]);
            }

            // Action
            try
            {
                using var trx = TransactionFactory.ReadCommitted();
                SignOutput result = await _behavior.Repository.Lines__Sign(
                    lineIds,
                    args.ToState,
                    args.ReasonId,
                    args.ReasonDetails,
                    args.OnBehalfOfUserId,
                    args.RuleType,
                    args.RoleId,
                    args.SignedAt ?? DateTimeOffset.Now,
                    returnIds: returnEntities,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

                // Validation
                AddErrorsAndThrowIfInvalid(result.Errors);

                var documentIds = result.DocumentIds;
                if (returnEntities)
                {
                    var response = await GetByIds(documentIds.ToList(), args, cancellation: default);

                    trx.Complete();
                    return response;
                }
                else
                {
                    trx.Complete();
                    return DocumentsResult.Empty();
                }
            }
            catch (CustomScriptException ex) when (ex.IsScriptBug && def.State == DefStates.Visible)
            {
                string lineDefName = "(Unkown)";
                if (ex.LineDefinitionId != null)
                {
                    var lineDef = await LineDefinition(ex.LineDefinitionId.Value);
                    lineDefName = lineDef.TitleSingular;
                }

                await _behavior.LogCustomScriptBug(
                                ex,
                                collection: nameof(Line),
                                definitionId: ex.LineDefinitionId,
                                defTitle: lineDefName,
                                scriptName: "Validate Sign Script",
                                entityIds: lineIds
                            );

                throw; // Bubble up to the client
            }
        }

        public async Task<DocumentsResult> UnsignLines(List<int> signatureIds, ActionArguments args)
        {
            await Initialize();
            var def = await Definition();
            var returnEntities = args.ReturnEntities ?? false;

            // C# Validation 
            // Goes here

            // Action
            try
            {
                using var trx = TransactionFactory.ReadCommitted();
                SignOutput result = await _behavior.Repository.LineSignatures__Delete(
                    ids: signatureIds,
                    returnIds: returnEntities,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

                // Validation
                AddErrorsAndThrowIfInvalid(result.Errors);

                // Load Result
                var documentIds = result.DocumentIds;
                if (returnEntities)
                {
                    var response = await GetByIds(documentIds.ToList(), args, cancellation: default);

                    trx.Complete();
                    return response;
                }
                else
                {
                    trx.Complete();
                    return DocumentsResult.Empty();
                }
            }
            catch (CustomScriptException ex) when (ex.IsScriptBug && def.State == DefStates.Visible)
            {
                string lineDefName = "(Unkown)";
                if (ex.LineDefinitionId != null)
                {
                    var lineDef = await LineDefinition(ex.LineDefinitionId.Value);
                    lineDefName = lineDef.TitleSingular;
                }

                await _behavior.LogCustomScriptBug(
                                ex,
                                collection: nameof(Line),
                                definitionId: ex.LineDefinitionId,
                                defTitle: lineDefName,
                                scriptName: "Validate Unsign Script",
                                entityIds: Enumerable.Empty<int>() // we don't know the line Ids unfortunately
                            );

                throw; // Bubble up to the client
            }
        }

        public async Task<DocumentsResult> Close(List<int> ids, ActionArguments args)
        {
            try
            {
                return await UpdateDocumentState(ids, args, nameof(Close));
            }
            catch (CustomScriptException ex) when (ex.IsScriptBug)
            {
                var def = await Definition();
                if (def.State == DefStates.Visible)
                {
                    // The stored procedure runs both document and line scripts
                    // We can tell by checking if this property is set
                    if (ex.LineDefinitionId != null)
                    {
                        var lineDef = await LineDefinition(ex.LineDefinitionId.Value);
                        await _behavior.LogCustomScriptBug(
                            ex,
                            collection: nameof(Line),
                            definitionId: ex.LineDefinitionId.Value,
                            defTitle: lineDef.TitleSingular,
                            scriptName: "Sign Script",
                            entityIds: Enumerable.Empty<int>()
                            );
                    }
                    else
                    {
                        await _behavior.LogCustomScriptBug(
                            ex,
                            collection: nameof(Document),
                            definitionId: DefinitionId,
                            defTitle: def.TitleSingular,
                            scriptName: "Validate Close Script",
                            entityIds: ids
                            );
                    }
                }

                throw; // Bubble up to the client
            }
        }

        public async Task<DocumentsResult> Open(List<int> ids, ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Open));
        }

        public async Task<DocumentsResult> Cancel(List<int> ids, ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Cancel));
        }

        public async Task<DocumentsResult> Uncancel(List<int> ids, ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Uncancel));
        }

        #region ZATCA Mapping

        public static Invoice MapInvoice(ZatcaInvoice inv, SettingsForClient settings, int previousCounterValue, string previousInvoiceHash)
        {
            return new Invoice
            {
                InvoiceNumber = inv.InvoiceNumber,
                UniqueInvoiceIdentifier = inv.UniqueInvoiceIdentifier,
                InvoiceIssueDateTime = inv.InvoiceIssueDateTime,
                InvoiceType = (InvoiceType)inv.InvoiceType,
                InvoiceTypeTransactions = ToInvoiceTransaction(inv),
                InvoiceNotes = string.IsNullOrWhiteSpace(inv.InvoiceNote) ? new() : new() { inv.InvoiceNote },
                InvoiceCurrency = inv.InvoiceCurrency,
                PurchaseOrderId = inv.PurchaseOrderId,
                BillingReferenceId = inv.BillingReferenceId,
                ContractId = inv.ContractId,
                InvoiceCounterValue = previousCounterValue + 1,
                PreviousInvoiceHash = previousInvoiceHash,
                Seller = new Party
                {
                    Id = new(PartyIdScheme.CommercialRegistration, settings.CommercialRegistrationNumber),
                    Address = new Address
                    {
                        Street = settings.Street,
                        AdditionalStreet = null,
                        BuildingNumber = settings.BuildingNumber,
                        AdditionalNumber = settings.SecondaryNumber,
                        District = settings.District,
                        City = settings.City,
                        PostalCode = settings.PostalCode,
                        Province = null,
                        CountryCode = "SA",
                    },
                    VatNumber = settings.TaxIdentificationNumber,
                    Name = settings.CompanyName
                },
                Buyer = new Party
                {
                    Id = inv.BuyerIdScheme == "VAT" ? null : new(ToPartyIdSchema(inv.BuyerIdScheme), inv.BuyerId),
                    Address = new Address
                    {
                        Street = inv.BuyerAddressStreet,
                        AdditionalStreet = inv.BuyerAddressAdditionalStreet,
                        BuildingNumber = inv.BuyerAddressBuildingNumber,
                        AdditionalNumber = inv.BuyerAddressAdditionalNumber,
                        District = inv.BuyerAddressDistrict,
                        City = inv.BuyerAddressCity,
                        PostalCode = inv.BuyerAddressPostalCode,
                        Province = inv.BuyerAddressProvince,
                        CountryCode = inv.BuyerAddressCountryCode,
                    },
                    VatNumber = inv.BuyerIdScheme == "VAT" ? inv.BuyerId : null,
                    Name = inv.BuyerName
                },
                SupplyDate = inv.SupplyDate,
                SupplyEndDate = inv.SupplyEndDate,
                PaymentMeans = (PaymentMeans)inv.PaymentMeans,
                ReasonsForIssuanceOfCreditDebitNote = string.IsNullOrWhiteSpace(inv.ReasonForIssuanceOfCreditDebitNote) ? new() : new() { inv.ReasonForIssuanceOfCreditDebitNote },
                PaymentTerms = inv.PaymentTerms,
                PaymentAccountId = inv.PaymentAccountId,
                AllowanceCharges = inv.AllowanceCharges.Select(ac => new AllowanceCharge
                {
                    Indicator = ac.IsCharge ? AllowanceChargeType.Charge : AllowanceChargeType.Allowance,
                    Amount = ac.Amount,
                    Reason = ac.Reason,
                    ReasonCode = ac.ReasonCode,
                    VatCategory = ToVatCategory(ac.VatCategory),
                    VatRate = ac.VatRate,
                }).ToList(),
                InvoiceTotalVatAmountInAccountingCurrency = inv.InvoiceTotalVatAmountInAccountingCurrency,
                PrepaidAmount = inv.PrepaidAmount,
                RoundingAmount = inv.RoundingAmount,
                //VatCategoryTaxableAmount = inv.VatCategoryTaxableAmount,
                //VatCategory = ToVatCategory(inv.VatCategory),
                //VatRate = inv.VatRate,
                //VatExemptionReason = inv.VatExemptionReason,
                //VatExemptionReasonCode = inv.VatExemptionReasonCode,
                Lines = inv.Lines.Select((line, index) => new InvoiceLine
                {
                    Identifier = index + 1,
                    PrepaymentId = line.PrepaymentId,
                    PrepaymentUuid = line.PrepaymentUuid,
                    PrepaymentIssueDateTime = line.PrepaymentIssueDateTime,
                    Quantity = line.Quantity,
                    QuantityUnit = line.QuantityUnit,
                    NetAmount = line.NetAmount,
                    AllowanceCharge = new LineAllowanceCharge
                    {
                        Indicator = line.AllowanceChargeIsCharge ? AllowanceChargeType.Charge : AllowanceChargeType.Allowance,
                        Amount = line.AllowanceChargeAmount,
                        Reason = line.AllowanceChargeReason,
                        ReasonCode = line.AllowanceChargeReasonCode,
                    },
                    VatAmount = line.VatAmount,
                    PrepaymentVatCategoryTaxableAmount = line.PrepaymentVatCategoryTaxableAmount,
                    ItemName = line.ItemName,
                    ItemBuyerIdentifier = line.ItemBuyerIdentifier,
                    ItemSellerIdentifier = line.ItemSellerIdentifier,
                    ItemStandardIdentifier = line.ItemStandardIdentifier,
                    ItemNetPrice = line.ItemNetPrice,
                    ItemVatCategory = ToVatCategory(line.ItemVatCategory),
                    ItemVatRate = line.ItemVatRate,
                    ItemVatExemptionReasonCode = ToVatExemptionReason(line.ItemVatExemptionReasonCode),
                    ItemVatExemptionReasonText = line.ItemVatExemptionReasonText,
                    PrepaymentVatCategory = ToVatCategory(line.PrepaymentVatCategory),
                    PrepaymentVatRate = line.PrepaymentVatRate,
                    ItemPriceBaseQuantity = line.ItemPriceBaseQuantity,
                    ItemPriceBaseQuantityUnit = line.ItemPriceBaseQuantityUnit,
                    ItemPriceDiscount = line.ItemPriceDiscount,
                    ItemGrossPrice = line.ItemGrossPrice,
                }).ToList(),
            };
        }

        private static InvoiceTransaction ToInvoiceTransaction(ZatcaInvoice inv)
        {
            InvoiceTransaction result = 0;

            result |= inv.IsSimplified ? InvoiceTransaction.Simplified : InvoiceTransaction.Standard;
            if (inv.IsThirdParty)
                result |= InvoiceTransaction.ThirdParty;
            if (inv.IsNominal)
                result |= InvoiceTransaction.Nominal;
            if (inv.IsExports)
                result |= InvoiceTransaction.Exports;
            if (inv.IsSummary)
                result |= InvoiceTransaction.Summary;
            if (inv.IsSelfBilled)
                result |= InvoiceTransaction.SelfBilled;

            return result;
        }

        private static PartyIdScheme ToPartyIdSchema(string scheme)
        {
            return scheme switch
            {
                "TIN" => PartyIdScheme.TaxIdentificationNumber,
                "CRN" => PartyIdScheme.CommercialRegistration,
                "MOM" => PartyIdScheme.Momrah,
                "MLS" => PartyIdScheme.Mhrsd,
                "700" => PartyIdScheme.Number700,
                "SAG" => PartyIdScheme.Misa,
                "NAT" => PartyIdScheme.NationalId,
                "GCC" => PartyIdScheme.GccId,
                "IQA" => PartyIdScheme.IqamaNumber,
                "PAS" => PartyIdScheme.PassportId,
                "OTH" => PartyIdScheme.OtherId,
                _ => throw new InvalidOperationException($"Unrecognized Party ID scheme {scheme}"),
            };
        }

        private static VatCategory ToVatCategory(string category)
        {
            return category switch
            {
                "E" => VatCategory.ExemptFromTax,
                "S" => VatCategory.StandardRate,
                "Z" => VatCategory.ZeroRatedGoods,
                "O" => VatCategory.NotSubjectToTax,
                _ => throw new InvalidOperationException($"Unrecognized VAT Category {category}"),
            };
        }

        private static VatExemptionReason ToVatExemptionReason(string reasonCode)
        {
            return reasonCode switch
            {
                // E
                "VATEX-SA-29" => VatExemptionReason.VATEX_SA_29,
                "VATEX-SA-29-7" => VatExemptionReason.VATEX_SA_29_7,
                "VATEX-SA-30" => VatExemptionReason.VATEX_SA_30,

                // Z
                "VATEX-SA-32" => VatExemptionReason.VATEX_SA_32,
                "VATEX-SA-33" => VatExemptionReason.VATEX_SA_33,
                "VATEX-SA-34-1" => VatExemptionReason.VATEX_SA_34_1,
                "VATEX-SA-34-2" => VatExemptionReason.VATEX_SA_34_2,
                "VATEX-SA-34-3" => VatExemptionReason.VATEX_SA_34_3,
                "VATEX-SA-34-4" => VatExemptionReason.VATEX_SA_34_4,
                "VATEX-SA-34-5" => VatExemptionReason.VATEX_SA_34_5,
                "VATEX-SA-35" => VatExemptionReason.VATEX_SA_35,
                "VATEX-SA-36" => VatExemptionReason.VATEX_SA_36,
                "VATEX-SA-EDU" => VatExemptionReason.VATEX_SA_EDU,
                "VATEX-SA-HEA" => VatExemptionReason.VATEX_SA_HEA,
                "VATEX-SA-MLTRY" => VatExemptionReason.VATEX_SA_MLTRY,

                // O
                "VATEX-SA-OOS" => VatExemptionReason.VATEX_SA_OOS,
                _ => throw new InvalidOperationException($"Unrecognized VAT Exemption reason code {reasonCode}"),
            };
        }

        private static string InvoiceBlobName(string guid, bool isSandbox)
        {
            string stage = isSandbox ? "Sandbox" : "Production";
            return $"Zatca/{stage}/{guid[0..2]}/{guid[2..4]}/{guid}";
        }

        #endregion

        #region Helpers

        #endregion

        private async Task<DocumentsResult> UpdateDocumentState(List<int> ids, ActionArguments args, string transition)
        {
            await Initialize();

            // Check user permissions
            var action = "State";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // C# Validation 
            var def = await Definition();
            if (transition == nameof(Open))
            {
                // ZATCA documents cannot be reopened
                if (!string.IsNullOrWhiteSpace(def.ZatcaDocumentType))
                {
                    // ModelState.AddError("[0]", _localizer["Error_CannotOpenAZatcaDocument"]);
                }
            }
            ModelState.ThrowIfInvalid();

            // Transaction
            using var trx = TransactionFactory.ReadCommitted();

            CloseDocumentOutput dcOutput = null;
            InboxStatusOutput output = transition switch
            {
                nameof(Close) => dcOutput = await _behavior.Repository.Documents__Close(DefinitionId, ids, ModelState.IsError, ModelState.RemainingErrors, UserId),
                nameof(Open) => await _behavior.Repository.Documents__Open(DefinitionId, ids, ModelState.IsError, ModelState.RemainingErrors, UserId),
                nameof(Cancel) => await _behavior.Repository.Documents__Cancel(DefinitionId, ids, ModelState.IsError, ModelState.RemainingErrors, UserId),
                nameof(Uncancel) => await _behavior.Repository.Documents__Uncancel(DefinitionId, ids, ModelState.IsError, ModelState.RemainingErrors, UserId),
                _ => throw new InvalidOperationException($"Unknown transition {transition}"),
            };

            // Validation
            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = args.ReturnEntities ?? false ?
                await GetByIds(ids, args, action, cancellation: default) :
                DocumentsResult.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            // Non-transactional stuff
            var statuses = output.InboxStatuses;
            _clientProxy.UpdateInboxStatuses(TenantId, statuses);

            // ZATCA integration goes here ...
            if (dcOutput != null && dcOutput.Invoices.Any())
            {
                if (dcOutput.Invoices.Count() != 1)
                {
                    throw new ZatcaException("Closing multiple ZATCA invoices is not currently supported");
                }

                var settings = await _behavior.Settings();
                var useSandbox = settings.ZatcaUseSandbox;

                if (!useSandbox && string.IsNullOrWhiteSpace(settings.ZatcaEncryptedSecurityToken))
                {
                    throw new ZatcaException(_localizer["Error_NotOnboardedWithZatca"]);
                }

                var inv = dcOutput.Invoices.Single();
                var invoice = MapInvoice(inv, settings, dcOutput.PreviousCounterValue, dcOutput.PreviousInvoiceHash);

                var secrets = new ZatcaSecrets(
                    encryptedSecurityToken: settings.ZatcaEncryptedSecurityToken,
                    encryptedSecret: settings.ZatcaEncryptedSecret,
                    encryptedPrivateKey: settings.ZatcaEncryptedPrivateKey,
                    keyIndex: settings.ZatcaEncryptionKeyIndex);

                // Call the ZATCA API
                ClearanceReport report;
                if (inv.IsSimplified)
                {
                    report = await _zatcaService.Report(invoice, secrets, useSandbox);
                }
                else
                {
                    report = await _zatcaService.Clear(invoice, secrets, useSandbox);
                }

                // If there are errors or warnings, notify tenant admins
                var warnings = report.ValidationResults?.WarningMessages;
                if (!report.IsSuccess || report.HasWarnings)
                {
                    var level = !report.IsSuccess ? TenantLogLevel.Error : TenantLogLevel.Warning;
                    await _behavior.LogZatcaErrorOrWarning(DefinitionId, def.TitleSingular, inv.Id, report.InvoiceXml, report.ValidationResultsJson(), level);
                }

                // TODO: What if a failure happens here before we commit the transaction.
                // We would lose the invoice XML, and the document will remain open, even
                // though it was already cleared with ZATCA API.
                if (report.IsSuccess)
                {
                    // If calling ZATCA API was successful...
                    // 1 - Update the document info
                    await _behavior.Repository.Zatca__UpdateDocumentInfo(
                        inv.Id,
                        ZatcaState.Reported,
                        report.ValidationResults == null ? null : JsonSerializer.Serialize(report.ValidationResults),
                        invoice.InvoiceCounterValue,
                        report.InvoiceHash,
                        invoice.UniqueInvoiceIdentifier);

                    // 2 - Save the invoice XML in Blob storage
                    var blobName = InvoiceBlobName(inv.UniqueInvoiceIdentifier.ToString(), useSandbox);
                    var blobBytes = Encoding.UTF8.GetBytes(report.InvoiceXml);
                    var blobs = new List<(string name, byte[] content)>() { (blobName, blobBytes) };
                    await _blobService.SaveBlobsAsync(TenantId, blobs);
                }
                else
                {
                    // If calling ZATCA API failed, throw an exception to roll back the transaction
                    throw new ZatcaException(@$"Error while clearing the invoice with ZATCA: 

{report.ValidationResultsJson()}");
                }
            }

            // Commit and return
            trx.Complete();

            return result;
        }

        private async Task<bool> IsClearable(CancellationToken cancellation)
        {
            var def = await Definition(cancellation);

            return def.Code?.EndsWith("388") ?? false;
        }

        #endregion

        public async Task<EmailCommandPreview> EmailCommandPreviewEntities(int templateId, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.EmailCommandPreviewEntities<Document>(templateId, args, cancellation);
        }

        public async Task<EmailPreview> EmailPreviewEntities(int templateId, int emailIndex, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.EmailPreviewEntities<Document>(templateId, emailIndex, args, cancellation);
        }

        public async Task<int> SendByEmail(int templateId, PrintEntitiesArguments<int> args, EmailCommandVersions versions, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.SendByEmail<Document>(templateId, args, versions, cancellation);
        }

        public async Task<EmailCommandPreview> EmailCommandPreviewEntity(int id, int templateId, PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.EmailCommandPreviewEntity<Document>(id, templateId, args, cancellation);
        }

        public async Task<EmailPreview> EmailPreviewEntity(int id, int templateId, int emailIndex, PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.EmailPreviewEntity<Document>(id, templateId, emailIndex, args, cancellation);
        }

        public async Task<int> SendByEmail(int id, int templateId, PrintEntityByIdArguments args, EmailCommandVersions versions, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.SendByEmail<Document>(id, templateId, args, versions, cancellation);
        }

        public async Task<MessageCommandPreview> MessageCommandPreviewEntities(int templateId, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.MessageCommandPreviewEntities<Document>(templateId, args, cancellation);
        }

        public async Task<int> SendByMessage(int templateId, PrintEntitiesArguments<int> args, string version, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.SendByMessage<Document>(templateId, args, version, cancellation);
        }

        public async Task<MessageCommandPreview> MessageCommandPreviewEntity(int id, int templateId, PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.MessageCommandPreviewEntity<Document>(id, templateId, args, cancellation);
        }

        public async Task<int> SendByMessage(int id, int templateId, PrintEntityByIdArguments args, string version, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.SendByMessage<Document>(id, templateId, args, version, cancellation);
        }

        public async Task<FileResult> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // This enforces read permissions
            string att = nameof(Document.Attachments);
            string attFileId = nameof(Attachment.FileId);
            string attFileName = nameof(Attachment.FileName);
            string attFileExt = nameof(Attachment.FileExtension);
            var result = await GetById(docId, new GetByIdArguments
            {
                Select = $"{att}.{attFileId},{att}.{attFileName},{att}.{attFileExt}"
            },
            cancellation);

            // Get the blob name
            var attachment = result.Entity?.Attachments?.FirstOrDefault(att => att.Id == attachmentId);
            if (attachment != null && !string.IsNullOrWhiteSpace(attachment.FileId))
            {
                try
                {
                    // Get the bytes
                    string blobName = AttachmentBlobName(attachment.FileId);
                    var fileBytes = await _blobService.LoadBlobAsync(TenantId, blobName, cancellation);

                    // Get the content type
                    var fileName = $"{attachment.FileName ?? "Attachment"}.{attachment.FileExtension}";
                    return new FileResult(fileBytes, fileName);
                }
                catch (BlobNotFoundException)
                {
                    throw new NotFoundException<int>(attachmentId);
                }
            }
            else
            {
                throw new NotFoundException<int>(attachmentId);
            }
        }

        public override async Task<DocumentResult> GetById(int id, GetByIdArguments args, CancellationToken cancellation)
        {
            var result = await base.GetById(id, args, cancellation);
            var entity = result.Entity;

            // TODO it's more accurate to do this from the client side (e.g. if the user views a cached document)
            if (entity.OpenedAt == null)
            {
                if (entity.AssigneeId == UserId)
                {
                    // Mark the entity's OpenedAt both in the DB and in the returned entity
                    var assignedAt = entity.AssignedAt.Value;
                    var openedAt = DateTimeOffset.Now;
                    var statuses = await _behavior.Repository.Documents__Preview(entity.Id, assignedAt, openedAt, UserId, cancellation);
                    entity.OpenedAt = openedAt;

                    // Notify the user
                    _clientProxy.UpdateInboxStatuses(TenantId, statuses);
                }
            }

            return result;
        }

        public async Task<LinesResult> AutoGenerateLinesForMultipleDefinitions(List<int> lineDefIds, List<DocumentForSave> docs, Dictionary<string, string> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            ConcurrentBag<LinesResult> results = new();
            await Task.WhenAll(lineDefIds.Select(async lineDefId =>
            {
                var linesResult = await AutoGenerateLines(lineDefId, docs, args, cancellation);

                results.Add(linesResult);
            }));

            // Merge the line results together
            List<LineForSave> lines = new();
            Dictionary<int, Account> accounts = new();
            Dictionary<int, Resource> resources = new();
            Dictionary<int, Agent> agents = new();
            Dictionary<int, EntryType> entryTypes = new();
            Dictionary<int, Center> centers = new();
            Dictionary<string, Currency> currencies = new();
            Dictionary<int, Unit> units = new();

            foreach (var r in results)
            {
                lines.AddRange(r.Data);
                foreach (var entity in r.Accounts)
                {
                    accounts.TryAdd(entity.Id, entity);
                }
                foreach (var entity in r.Resources)
                {
                    resources.TryAdd(entity.Id, entity);
                }
                foreach (var entity in r.Agents)
                {
                    agents.TryAdd(entity.Id, entity);
                }
                foreach (var entity in r.EntryTypes)
                {
                    entryTypes.TryAdd(entity.Id, entity);
                }
                foreach (var entity in r.Centers)
                {
                    centers.TryAdd(entity.Id, entity);
                }
                foreach (var entity in r.Currencies)
                {
                    currencies.TryAdd(entity.Id, entity);
                }
                foreach (var entity in r.Units)
                {
                    units.TryAdd(entity.Id, entity);
                }
            }

            return new LinesResult(lines,
                accounts.Values.ToList(),
                resources.Values.ToList(),
                agents.Values.ToList(),
                entryTypes.Values.ToList(),
                centers.Values.ToList(),
                currencies.Values.ToList(),
                units.Values.ToList());
        }

        public async Task<LinesResult> AutoGenerateLines(int lineDefId, List<DocumentForSave> docs, Dictionary<string, string> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            docs ??= new List<DocumentForSave>();

            // TODO: Permissions (?)
            await UserPermissionsFilter(PermissionActions.Update, cancellation: default);
            // ids = await CheckActionPermissionsBefore(actionFilter, ids);

            var def = await Definition(cancellation);
            var lineDef = await LineDefinition(lineDefId, cancellation);
            if (!lineDef.GenerateScript)
            {
                throw new ServiceException(@$"Line definition ""{lineDef.TitleSingular}"" does not have an auto-generate script.");
            }

            // Better args will contain only the defined parameter keys and all the defined parameter keys (with possible null values)
            var betterArgs = new Dictionary<string, string>();
            foreach (var param in lineDef.GenerateParameters)
            {
                var value = args.GetValueOrDefault(param.Key);

                // Ensure all required signatures are supplied
                if (param.Visibility == Visibility.Required && string.IsNullOrWhiteSpace(value))
                {
                    var settings = await _behavior.Settings(cancellation);
                    var paramLabel = settings.Localize(param.Label, param.Label2, param.Label3);
                    var msg = _localizer[ErrorMessages.Error_Field0IsRequired, paramLabel];
                    throw new ServiceException(msg);
                }

                betterArgs[param.Key] = value;
            }

            // The SP uses those to create the TVPs
            SetOriginalIndices(docs);

            try
            {
                // Call the SP
                var (lines, accounts, resources, agents, entryTypes, centers, currencies, units) =
                    await _behavior.Repository.Lines__Generate(lineDefId, docs, betterArgs, UserId, cancellation);

                foreach (var line in lines)
                {
                    line.DefinitionId = lineDefId;
                }

                return new LinesResult(lines, accounts, resources, agents, entryTypes, centers, currencies, units);
            }
            catch (CustomScriptException ex) when (ex.IsScriptBug && def.State == DefStates.Visible)
            {
                await _behavior.LogCustomScriptBug(
                    ex,
                    collection: nameof(Line),
                    definitionId: lineDefId,
                    defTitle: lineDef.TitleSingular,
                    scriptName: "Auto-Generate Script",
                    entityIds: Enumerable.Empty<int>()
                    );

                throw; // Bubble up to the client
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            var permissions = (await base.UserPermissions(action, cancellation)).ToList();

            // Add a special permission that lets you see the documents that were assigned to you
            permissions.AddRange(DocumentServiceUtil.HardCodedPermissions(action));

            return permissions;
        }

        protected override async Task<EntityQuery<Document>> Search(EntityQuery<Document> query, GetArguments args, CancellationToken cancellation)
        {
            var def = await Definition(cancellation);
            List<(string prefix, int definitionId)> map;
            bool includeInternalRef;
            bool includeExternalRef;
            if (def != null)
            {
                var prefix = def.Prefix;
                map = new List<(string prefix, int definitionId)>
                {
                    (prefix, DefinitionId)
                };
                includeInternalRef = def.InternalReferenceVisibility;
                includeExternalRef = def.ExternalReferenceVisibility;
            }
            else
            {
                map = new List<(string prefix, int definitionId)>();
                includeInternalRef = false;
                includeExternalRef = false;
            }

            return DocumentServiceUtil.SearchImpl(query, args, map, includeInternalRef, includeExternalRef);
        }

        protected override async Task<DocumentsResult> ToEntitiesResult(List<Document> data, int? count = null, CancellationToken cancellation = default)
        {
            var requiredSignatures = await GetRequiredSignatures(data, cancellation);
            return new DocumentsResult(data, requiredSignatures, count);
        }

        protected override async Task<DocumentResult> ToEntityResult(Document entity, CancellationToken cancellation = default)
        {
            var singleton = new List<Document> { entity };
            var requiredSignatures = await GetRequiredSignatures(singleton, cancellation);
            return new DocumentResult(entity, requiredSignatures);
        }

        protected async Task<IReadOnlyList<RequiredSignature>> GetRequiredSignatures(IEnumerable<Document> data, CancellationToken cancellation)
        {
            if (IncludeRequiredSignatures)
            {
                // DocumentIds parameter
                var docIds = data.Select(doc => new IdListItem { Id = doc.Id });
                if (!docIds.Any())
                {
                    return new List<RequiredSignature>();
                }

                var docIdsTable = RepositoryUtilities.DataTable(docIds);
                var docIdsTvp = new SqlParameter("@DocumentIds", docIdsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = System.Data.SqlDbType.Structured
                };

                var userId = new SqlParameter("@UserId", UserId);

                var query = _behavior.Repository.EntityQuery<RequiredSignature>()
                    .AdditionalParameters(docIdsTvp, userId)
                    .Expand($"{nameof(RequiredSignature.Role)},{nameof(RequiredSignature.User)},{nameof(RequiredSignature.SignedBy)},{nameof(RequiredSignature.OnBehalfOfUser)},{nameof(RequiredSignature.ProxyRole)}")
                    .OrderBy(nameof(RequiredSignature.LineId));

                return await query.ToListAsync(QueryContext(), cancellation);
            }
            else
            {
                return null;
            }
        }

        private static void SetOriginalIndices(List<DocumentForSave> docs)
        {
            foreach (var (doc, docIndex) in docs.Indexed())
            {
                // Remember the indices, comes in handy in the validation later
                doc.EntityMetadata.OriginalIndex = docIndex;

                if (doc.LineDefinitionEntries != null)
                {
                    foreach (var (lineDefEntry, lineDefEntryIndex) in doc.LineDefinitionEntries.Indexed())
                    {
                        if (lineDefEntry != null)
                        {
                            lineDefEntry.EntityMetadata.OriginalIndex = lineDefEntryIndex;
                        }
                    }
                }

                if (doc.Lines != null)
                {
                    foreach (var (line, lineIndex) in doc.Lines.Indexed())
                    {
                        if (line != null)
                        {
                            line.EntityMetadata.OriginalIndex = lineIndex;

                            if (line.Entries != null)
                            {
                                foreach (var (entry, entryIndex) in line.Entries.Indexed())
                                {
                                    if (entry != null)
                                    {
                                        entry.EntityMetadata.OriginalIndex = entryIndex;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override async Task<List<DocumentForSave>> SavePreprocessAsync(List<DocumentForSave> docs)
        {
            var docDef = await Definition();

            // Creating new entities forbidden if the definition is archived
            if (docs.Any(e => e?.Id == 0) && docDef.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new ServiceException(msg);
            }

            // Get the functional currency Id
            var settings = await _behavior.Settings();
            var functionalId = settings.FunctionalCurrencyId;

            // Get the built in definition Ids
            var defs = await _behavior.Definitions();
            var jvDefId = defs.ManualJournalVouchersDefinitionId;
            var manualLineDefId = defs.ManualLinesDefinitionId;

            bool isJV = DefinitionId == jvDefId;

            // Set default values
            foreach (var (doc, docIndex) in docs.Indexed())
            {
                // Set all IsCommon values that are invisible to FALSE
                if (isJV)
                {
                    // Those are always true in JV
                    doc.PostingDateIsCommon = true;
                    doc.MemoIsCommon = true;
                    doc.CenterIsCommon = false;
                }
                else
                {
                    doc.MemoIsCommon ??= docDef.MemoIsCommonVisibility && (doc.MemoIsCommon ?? false);
                    doc.PostingDateIsCommon ??= docDef.PostingDateIsCommonVisibility && (doc.PostingDateIsCommon ?? false);
                    doc.CenterIsCommon ??= docDef.CenterIsCommonVisibility && (doc.CenterIsCommon ?? false);
                }

                doc.CurrencyIsCommon = docDef.CurrencyVisibility && (doc.CurrencyIsCommon ?? false);
                doc.AgentIsCommon = docDef.AgentVisibility && (doc.AgentIsCommon ?? false);
                doc.ResourceIsCommon = docDef.ResourceVisibility && (doc.ResourceIsCommon ?? false);
                doc.NotedAgentIsCommon = docDef.NotedAgentVisibility && (doc.NotedAgentIsCommon ?? false);
                doc.NotedResourceIsCommon = docDef.NotedResourceVisibility && (doc.NotedResourceIsCommon ?? false);
                doc.QuantityIsCommon = docDef.QuantityVisibility && (doc.QuantityIsCommon ?? false);
                doc.UnitIsCommon = docDef.UnitVisibility && (doc.UnitIsCommon ?? false);
                doc.Time1IsCommon = docDef.Time1Visibility && (doc.Time1IsCommon ?? false);
                doc.DurationIsCommon = docDef.DurationVisibility && (doc.DurationIsCommon ?? false);
                doc.DurationUnitIsCommon = docDef.DurationUnitVisibility && (doc.DurationUnitIsCommon ?? false);
                doc.Time2IsCommon = docDef.Time2Visibility && (doc.Time2IsCommon ?? false);
                doc.NotedDateIsCommon = docDef.NotedDateVisibility && (doc.NotedDateIsCommon ?? false);
                doc.ExternalReferenceIsCommon = docDef.ExternalReferenceVisibility && (doc.ExternalReferenceIsCommon ?? false);
                doc.ReferenceSourceIsCommon = docDef.ReferenceSourceVisibility && (doc.ReferenceSourceIsCommon ?? false);
                doc.InternalReferenceIsCommon = docDef.InternalReferenceVisibility && (doc.InternalReferenceIsCommon ?? false);

                // In case of a single business unit
                if (settings.SingleBusinessUnitId != null)
                {
                    doc.CenterId = settings.SingleBusinessUnitId;
                }

                // Defaults that make the code simpler later
                doc.Clearance ??= 0; // Public
                doc.Lines ??= new List<LineForSave>();
                doc.LineDefinitionEntries ??= new List<DocumentLineDefinitionEntryForSave>();

                doc.Lines.ForEach(line =>
                {
                    // Line defaults
                    line.Entries ??= new List<EntryForSave>();
                    line.Boolean1 ??= false;
                });
            }

            SetOriginalIndices(docs);

            // Set common header values on the lines
            docs.ForEach(doc =>
            {
                // All fields that aren't visible and marked as common, set them to null, the UI hides them anyways
                // Those 3 are different than the rest, they can remain visible even when is common = false
                doc.Memo = docDef.MemoVisibility != null ? doc.Memo : null;
                doc.CenterId = docDef.CenterVisibility != null ? doc.CenterId : null;
                doc.PostingDate = docDef.PostingDateVisibility != null ? doc.PostingDate : null;

                doc.CurrencyId = docDef.CurrencyVisibility && doc.CurrencyIsCommon.Value ? doc.CurrencyId : null;

                doc.AgentId = docDef.AgentVisibility && doc.AgentIsCommon.Value ? doc.AgentId : null;
                doc.ResourceId = docDef.ResourceVisibility && doc.ResourceIsCommon.Value ? doc.ResourceId : null;
                doc.NotedAgentId = docDef.NotedAgentVisibility && doc.NotedAgentIsCommon.Value ? doc.NotedAgentId : null;
                doc.NotedResourceId = docDef.NotedResourceVisibility && doc.NotedResourceIsCommon.Value ? doc.NotedResourceId : null;

                doc.Quantity = docDef.QuantityVisibility && doc.QuantityIsCommon.Value ? doc.Quantity : null;
                doc.UnitId = docDef.UnitVisibility && doc.UnitIsCommon.Value ? doc.UnitId : null;
                doc.Time1 = docDef.Time1Visibility && doc.Time1IsCommon.Value ? doc.Time1 : null;
                doc.Duration = docDef.DurationVisibility && doc.DurationIsCommon.Value ? doc.Duration : null;
                doc.DurationUnitId = docDef.DurationUnitVisibility && doc.DurationUnitIsCommon.Value ? doc.DurationUnitId : null;
                doc.Time2 = docDef.Time2Visibility && doc.Time2IsCommon.Value ? doc.Time2 : null;
                doc.NotedDate = docDef.NotedDateVisibility && doc.NotedDateIsCommon.Value ? doc.NotedDate : null;

                doc.ExternalReference = docDef.ExternalReferenceVisibility && doc.ExternalReferenceIsCommon.Value ? doc.ExternalReference : null;
                doc.ReferenceSourceId = docDef.ReferenceSourceVisibility && doc.ReferenceSourceIsCommon.Value ? doc.ReferenceSourceId : null;
                doc.InternalReference = docDef.InternalReferenceVisibility && doc.InternalReferenceIsCommon.Value ? doc.InternalReference : null;

                // Manual lines always inherit memo and posting date from document header (if visible)
                if (docDef.MemoVisibility != null)
                {
                    doc.Lines.ForEach(line =>
                    {
                        if (line.DefinitionId == manualLineDefId)
                        {
                            line.Memo = doc.Memo;
                        }
                    });
                }

                if (docDef.PostingDateVisibility != null)
                {
                    doc.Lines.ForEach(line =>
                    {
                        if (line.DefinitionId == manualLineDefId)
                        {
                            line.PostingDate = doc.PostingDate;
                        }
                    });
                }

                // All fields that are marked as common, copy the common value across to the 
                // lines and entries, we deal with the lines one definitionId at a time
                foreach (var linesGroup in doc.Lines.GroupBy(e => e.DefinitionId.Value))
                {
                    if (!defs.Lines.TryGetValue(linesGroup.Key, out LineDefinitionForClient lineDef))
                    {
                        // Validation takes care of this later on
                        continue;
                    }

                    // Silently remove entries that are out of bounds (they could be a relic from a time when the definition specified more entries)
                    doc.LineDefinitionEntries.RemoveAll(e => e.LineDefinitionId == linesGroup.Key && e.EntryIndex >= lineDef.Entries.Count);

                    var defaultsToForm = lineDef.ViewDefaultsToForm;
                    var tabEntries = new DocumentLineDefinitionEntryForSave[lineDef.Entries.Count];
                    foreach (var tabEntry in doc.LineDefinitionEntries.Where(e => e.LineDefinitionId == linesGroup.Key))
                    {
                        if (tabEntry.EntryIndex < 0)
                        {
                            continue; // Validation takes care of this later
                        }

                        if (tabEntries[tabEntry.EntryIndex.Value] != null)
                        {
                            continue; // Validation takes care of this later
                        }

                        tabEntries[tabEntry.EntryIndex.Value] = tabEntry;
                    }

                    foreach (var line in linesGroup)
                    {
                        // If the number of entries is not the same as the definition specifies, fix that
                        while (line.Entries.Count < lineDef.Entries.Count)
                        {
                            // If less, add the missing entries
                            var entryDef = lineDef.Entries[line.Entries.Count];
                            line.Entries.Add(new EntryForSave { });
                        }

                        // Copy the direction from the definition
                        for (var i = 0; i < line.Entries.Count; i++)
                        {
                            if (line.DefinitionId != manualLineDefId)
                            {
                                line.Entries[i].Direction = lineDef.Entries[i].Direction;
                            }
                        }

                        #region IsCommon Behavior

                        // Copy common values from the headers if they are marked inherits from header
                        // IMPORTANT: Keep in sync with after the SQL preprocess
                        foreach (var colDef in lineDef.Columns)
                        {
                            if (colDef.ColumnName == nameof(Line.Memo))
                            {
                                if (CopyFromDocument(colDef, doc.MemoIsCommon))
                                {
                                    line.Memo = doc.Memo;
                                }
                                else
                                {
                                    var tabEntry = tabEntries.FirstOrDefault() ?? DefaultTabEntryForSave;
                                    if (CopyFromTab(colDef, tabEntry.MemoIsCommon, defaultsToForm))
                                    {
                                        line.Memo = tabEntry.Memo;
                                    }
                                }
                            }
                            else if (colDef.ColumnName == nameof(Line.PostingDate))
                            {
                                if (CopyFromDocument(colDef, doc.PostingDateIsCommon))
                                {
                                    line.PostingDate = doc.PostingDate;
                                }
                                else
                                {
                                    var tabEntry = tabEntries.FirstOrDefault() ?? DefaultTabEntryForSave;
                                    if (CopyFromTab(colDef, tabEntry.PostingDateIsCommon, defaultsToForm))
                                    {
                                        line.PostingDate = tabEntry.PostingDate;
                                    }
                                }
                            }
                            else
                            {
                                if (colDef.EntryIndex >= line.Entries.Count ||
                                    colDef.EntryIndex >= lineDef.Entries.Count ||
                                    colDef.EntryIndex < 0)
                                {
                                    // To avoid index out of bounds exception
                                    continue;
                                }

                                // Copy the common values
                                var entry = line.Entries[colDef.EntryIndex];
                                var tabEntry = tabEntries[colDef.EntryIndex] ?? DefaultTabEntryForSave;

                                switch (colDef.ColumnName)
                                {
                                    case nameof(Entry.CurrencyId):
                                        if (CopyFromDocument(colDef, doc.CurrencyIsCommon))
                                        {
                                            entry.CurrencyId = doc.CurrencyId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.CurrencyIsCommon, defaultsToForm))
                                        {
                                            entry.CurrencyId = tabEntry.CurrencyId;
                                        }
                                        break;

                                    case nameof(Entry.CenterId):
                                        if (CopyFromDocument(colDef, doc.CenterIsCommon))
                                        {
                                            entry.CenterId = doc.CenterId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.CenterIsCommon, defaultsToForm))
                                        {
                                            entry.CenterId = tabEntry.CenterId ?? settings.SingleBusinessUnitId;
                                        }
                                        break;

                                    case nameof(Entry.AgentId):
                                        if (CopyFromDocument(colDef, doc.AgentIsCommon))
                                        {
                                            entry.AgentId = doc.AgentId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.AgentIsCommon, defaultsToForm))
                                        {
                                            entry.AgentId = tabEntry.AgentId;
                                        }
                                        break;

                                    case nameof(Entry.ResourceId):
                                        if (CopyFromDocument(colDef, doc.ResourceIsCommon))
                                        {
                                            entry.ResourceId = doc.ResourceId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ResourceIsCommon, defaultsToForm))
                                        {
                                            entry.ResourceId = tabEntry.ResourceId;
                                        }
                                        break;

                                    case nameof(Entry.NotedAgentId):
                                        if (CopyFromDocument(colDef, doc.NotedAgentIsCommon))
                                        {
                                            entry.NotedAgentId = doc.NotedAgentId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.NotedAgentIsCommon, defaultsToForm))
                                        {
                                            entry.NotedAgentId = tabEntry.NotedAgentId;
                                        }
                                        break;

                                    case nameof(Entry.NotedResourceId):
                                        if (CopyFromDocument(colDef, doc.NotedResourceIsCommon))
                                        {
                                            entry.NotedResourceId = doc.NotedResourceId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.NotedResourceIsCommon, defaultsToForm))
                                        {
                                            entry.NotedResourceId = tabEntry.NotedResourceId;
                                        }
                                        break;

                                    case nameof(Entry.Quantity):
                                        if (CopyFromDocument(colDef, doc.QuantityIsCommon))
                                        {
                                            entry.Quantity = doc.Quantity;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.QuantityIsCommon, defaultsToForm))
                                        {
                                            entry.Quantity = tabEntry.Quantity;
                                        }
                                        break;

                                    case nameof(Entry.UnitId):
                                        if (CopyFromDocument(colDef, doc.UnitIsCommon))
                                        {
                                            entry.UnitId = doc.UnitId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.UnitIsCommon, defaultsToForm))
                                        {
                                            entry.UnitId = tabEntry.UnitId;
                                        }
                                        break;

                                    case nameof(Entry.Time1):
                                        if (CopyFromDocument(colDef, doc.Time1IsCommon))
                                        {
                                            entry.Time1 = doc.Time1;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.Time1IsCommon, defaultsToForm))
                                        {
                                            entry.Time1 = tabEntry.Time1;
                                        }
                                        break;

                                    case nameof(Entry.Duration):
                                        if (CopyFromDocument(colDef, doc.DurationIsCommon))
                                        {
                                            entry.Duration = doc.Duration;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.DurationIsCommon, defaultsToForm))
                                        {
                                            entry.Duration = tabEntry.Duration;
                                        }
                                        break;

                                    case nameof(Entry.DurationUnitId):
                                        if (CopyFromDocument(colDef, doc.DurationUnitIsCommon))
                                        {
                                            entry.DurationUnitId = doc.DurationUnitId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.DurationUnitIsCommon, defaultsToForm))
                                        {
                                            entry.DurationUnitId = tabEntry.DurationUnitId;
                                        }
                                        break;

                                    case nameof(Entry.Time2):
                                        if (CopyFromDocument(colDef, doc.Time2IsCommon))
                                        {
                                            entry.Time2 = doc.Time2;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.Time2IsCommon, defaultsToForm))
                                        {
                                            entry.Time2 = tabEntry.Time2;
                                        }
                                        break;

                                    case nameof(Entry.NotedDate):
                                        if (CopyFromDocument(colDef, doc.NotedDateIsCommon))
                                        {
                                            entry.NotedDate = doc.NotedDate;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.NotedDateIsCommon, defaultsToForm))
                                        {
                                            entry.NotedDate = tabEntry.NotedDate;
                                        }
                                        break;

                                    case nameof(Entry.ExternalReference):
                                        if (CopyFromDocument(colDef, doc.ExternalReferenceIsCommon))
                                        {
                                            entry.ExternalReference = doc.ExternalReference;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ExternalReferenceIsCommon, defaultsToForm))
                                        {
                                            entry.ExternalReference = tabEntry.ExternalReference;
                                        }
                                        break;

                                    case nameof(Entry.ReferenceSourceId):
                                        if (CopyFromDocument(colDef, doc.ReferenceSourceIsCommon))
                                        {
                                            entry.ReferenceSourceId = doc.ReferenceSourceId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ReferenceSourceIsCommon, defaultsToForm))
                                        {
                                            entry.ReferenceSourceId = tabEntry.ReferenceSourceId;
                                        }
                                        break;

                                    case nameof(Entry.InternalReference):
                                        if (CopyFromDocument(colDef, doc.InternalReferenceIsCommon))
                                        {
                                            entry.InternalReference = doc.InternalReference;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.InternalReferenceIsCommon, defaultsToForm))
                                        {
                                            entry.InternalReference = tabEntry.InternalReference;
                                        }
                                        break;
                                }
                            }
                        }

                        #endregion
                    }
                }
            });

            // SQL server preprocessing
            try
            {
                await _behavior.Repository.Documents__Preprocess(DefinitionId, docs, UserId);
            }
            catch (CustomScriptException ex) when (ex.IsScriptBug && docDef.State == DefStates.Visible)
            {
                string lineDefName = "(Unkown)";
                var entities = Enumerable.Empty<LineForSave>();
                if (ex.LineDefinitionId != null)
                {
                    var lineDef = await LineDefinition(ex.LineDefinitionId.Value);
                    lineDefName = lineDef.TitleSingular;
                    entities = docs.SelectMany(doc => doc.Lines).Where(line => line.DefinitionId == ex.LineDefinitionId.Value);
                }

                await _behavior.LogCustomScriptBug(
                                ex,
                                collection: nameof(Line),
                                definitionId: ex.LineDefinitionId,
                                defTitle: lineDefName,
                                scriptName: "Preprocess Script",
                                entities: entities
                            );

                throw; // Bubble up to the client
            }

            var tabEntryDesc = TypeDescriptor.Get<DocumentLineDefinitionEntryForSave>();

            // C# Processing after SQL
            docs.ForEach(doc =>
            {
                // Lines preprocessing
                doc.Lines.ForEach(line =>
                {
                    line.Entries.ForEach(entry =>
                    {
                        // If currency is functional, make sure that Value = MonetaryValue
                        if (entry.CurrencyId == settings.FunctionalCurrencyId)
                        {
                            if (line.DefinitionId == manualLineDefId)
                            {
                                // Manual lines, the value is always entered by the user
                                entry.MonetaryValue = entry.Value;
                            }
                            else
                            {
                                // Smart lines, the monetary value is always entered by the user
                                entry.Value = entry.MonetaryValue;
                            }
                        }

                        // Always round the value according to the functional currency decimals
                        if (entry.Value != null)
                        {
                            entry.Value = Math.Round(entry.Value.Value, settings.FunctionalCurrencyDecimals);
                        }
                    });
                });

                ////// handle subtle exchange rate rounding bugs. IF...
                // (1) All non-manual entries have the same non-functional currency
                // (2) Monetary Value of non-manual entries is balanced
                // (3) Value of non-manual is not balanced
                // => Take the difference and add it to the entry with the biggest value
                foreach (var line in doc.Lines.Where(e => e.DefinitionId != manualLineDefId && e.Entries.Count > 0))
                {
                    var firstCurrencyId = line.Entries[0].CurrencyId;
                    if (firstCurrencyId != functionalId && line.Entries.All(e => e.CurrencyId == firstCurrencyId) &&
                        line.Entries.Sum(e => e.MonetaryValue * e.Direction) == 0)
                    {
                        var valueDiff = line.Entries.Sum(e => (e.Value ?? 0) * e.Direction.Value);
                        var maxDiff = line.Entries.Count * (1.0m / settings.FunctionalCurrencyDecimals); // maxDiff = 0.01 for USD
                        if (valueDiff != 0 && Math.Abs(valueDiff) <= maxDiff)
                        {
                            var entry = line.Entries.MaxBy(e => e.Value ?? 0); // Adjust the entry with the max value
                            entry.Value -= Math.Sign(entry.Direction.Value) * valueDiff;
                        }
                    }
                }
            });

            #region IsCommon Double Check

            // Ensure IsCommon is still honored
            // IMPORTANT: Keep in sync with part before the SQL preprocess
            docs.ForEach(doc =>
            {
                // All fields that are marked as common, copy the common value across to the 
                // lines and entries, we deal with the lines one definitionId at a time
                foreach (var linesGroup in doc.Lines.GroupBy(e => e.DefinitionId.Value))
                {
                    if (!defs.Lines.TryGetValue(linesGroup.Key, out LineDefinitionForClient lineDef))
                    {
                        // Validation takes care of this later on
                        continue;
                    }

                    var defaultsToForm = lineDef.ViewDefaultsToForm;
                    var tabEntries = new DocumentLineDefinitionEntryForSave[lineDef.Entries.Count];
                    foreach (var tabEntry in doc.LineDefinitionEntries.Where(e => e.LineDefinitionId == linesGroup.Key))
                    {
                        if (tabEntry.EntryIndex < 0)
                        {
                            continue; // Validation takes care of this later
                        }

                        if (tabEntries[tabEntry.EntryIndex.Value] != null)
                        {
                            continue; // Validation takes care of this later
                        }

                        tabEntries[tabEntry.EntryIndex.Value] = tabEntry;
                    }

                    foreach (var line in linesGroup)
                    {
                        // Copy common values from the headers if they are marked inherits from header
                        foreach (var colDef in lineDef.Columns)
                        {
                            if (colDef.ColumnName == nameof(Line.Memo))
                            {
                                if (CopyFromDocument(colDef, doc.MemoIsCommon))
                                {
                                    if (line.Memo != doc.Memo)
                                    {
                                        throw new InvalidOperationException($"[Bug] {nameof(doc.MemoIsCommon)}=true, but {nameof(line.Memo)} of line of type {lineDef.TitleSingular} was changed in preprocess from '{doc.Memo}' to '{line.Memo}'");
                                    }
                                }
                                else
                                {
                                    var tabEntry = tabEntries.FirstOrDefault() ?? DefaultTabEntryForSave;
                                    if (CopyFromTab(colDef, tabEntry.MemoIsCommon, defaultsToForm))
                                    {
                                        if (line.Memo != tabEntry.Memo)
                                        {
                                            throw new InvalidOperationException($"[Bug] {nameof(tabEntry.MemoIsCommon)}=true, but {nameof(line.Memo)} of line of type {lineDef.TitleSingular} was changed in preprocess from '{tabEntry.Memo}' to '{line.Memo}'");
                                        }
                                    }
                                }
                            }
                            else if (colDef.ColumnName == nameof(Line.PostingDate))
                            {
                                if (CopyFromDocument(colDef, doc.PostingDateIsCommon))
                                {
                                    line.PostingDate = doc.PostingDate;
                                    if (line.PostingDate != doc.PostingDate)
                                    {
                                        throw new InvalidOperationException($"[Bug] {nameof(doc.PostingDateIsCommon)}=true, but {nameof(line.PostingDate)} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.PostingDate:yyyy-MM-dd} to '{line.PostingDate:yyyy-MM-dd}'");
                                    }
                                }
                                else
                                {
                                    var tabEntry = tabEntries.FirstOrDefault() ?? DefaultTabEntryForSave;
                                    if (CopyFromTab(colDef, tabEntry.PostingDateIsCommon, defaultsToForm))
                                    {
                                        if (line.PostingDate != tabEntry.PostingDate)
                                        {
                                            throw new InvalidOperationException($"[Bug] {nameof(tabEntry.PostingDateIsCommon)}=true, but {nameof(line.PostingDate)} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.PostingDate:yyyy-MM-dd} to '{line.PostingDate:yyyy-MM-dd}'");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (colDef.EntryIndex >= line.Entries.Count ||
                                    colDef.EntryIndex >= lineDef.Entries.Count ||
                                    colDef.EntryIndex < 0)
                                {
                                    // To avoid index out of bounds exception
                                    continue;
                                }

                                // Check that an invisible inheriting value has not been overridden without the visible parent value
                                var entry = line.Entries[colDef.EntryIndex];
                                var tabEntry = tabEntries[colDef.EntryIndex] ?? DefaultTabEntryForSave;

                                switch (colDef.ColumnName)
                                {
                                    case nameof(Entry.CurrencyId):
                                        if (CopyFromDocument(colDef, doc.CurrencyIsCommon))
                                        {
                                            if (entry.CurrencyId != doc.CurrencyId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.CurrencyId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.CurrencyId} to {entry.CurrencyId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.CurrencyIsCommon, defaultsToForm))
                                        {
                                            if (entry.CurrencyId != tabEntry.CurrencyId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.CurrencyId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.CurrencyId} to {entry.CurrencyId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.AgentId):
                                        if (CopyFromDocument(colDef, doc.AgentIsCommon))
                                        {
                                            if (entry.AgentId != doc.AgentId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.AgentId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.AgentId} to {entry.AgentId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.AgentIsCommon, defaultsToForm))
                                        {
                                            if (entry.AgentId != tabEntry.AgentId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.AgentId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.AgentId} to {entry.AgentId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.ResourceId):
                                        if (CopyFromDocument(colDef, doc.ResourceIsCommon))
                                        {
                                            if (entry.ResourceId != doc.ResourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ResourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.ResourceId} to {entry.ResourceId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ResourceIsCommon, defaultsToForm))
                                        {
                                            if (entry.ResourceId != tabEntry.ResourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ResourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.ResourceId} to {entry.ResourceId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.NotedAgentId):
                                        if (CopyFromDocument(colDef, doc.NotedAgentIsCommon))
                                        {
                                            if (entry.NotedAgentId != doc.NotedAgentId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.NotedAgentId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.NotedAgentId} to {entry.NotedAgentId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.NotedAgentIsCommon, defaultsToForm))
                                        {
                                            if (entry.NotedAgentId != tabEntry.NotedAgentId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.NotedAgentId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.NotedAgentId} to {entry.NotedAgentId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.NotedResourceId):
                                        if (CopyFromDocument(colDef, doc.NotedResourceIsCommon))
                                        {
                                            if (entry.NotedResourceId != doc.NotedResourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.NotedResourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.NotedResourceId} to {entry.NotedResourceId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.NotedResourceIsCommon, defaultsToForm))
                                        {
                                            if (entry.NotedResourceId != tabEntry.NotedResourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.NotedResourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.NotedResourceId} to {entry.NotedResourceId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.Quantity):
                                        if (CopyFromDocument(colDef, doc.QuantityIsCommon))
                                        {
                                            if (entry.Quantity != doc.Quantity)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Quantity)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.Quantity} to {entry.Quantity}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.QuantityIsCommon, defaultsToForm))
                                        {
                                            if (entry.Quantity != tabEntry.Quantity)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Quantity)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.Quantity} to {entry.Quantity}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.UnitId):
                                        if (CopyFromDocument(colDef, doc.UnitIsCommon))
                                        {
                                            if (entry.UnitId != doc.UnitId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.UnitId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.UnitId} to {entry.UnitId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.UnitIsCommon, defaultsToForm))
                                        {
                                            if (entry.UnitId != tabEntry.UnitId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.UnitId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.UnitId} to {entry.UnitId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.CenterId):
                                        if (CopyFromDocument(colDef, doc.CenterIsCommon))
                                        {
                                            if (entry.CenterId != doc.CenterId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.CenterId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.CenterId} to {entry.CenterId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.CenterIsCommon, defaultsToForm))
                                        {
                                            if (entry.CenterId != (tabEntry.CenterId ?? settings.SingleBusinessUnitId))
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.CenterId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.CenterId} to {entry.CenterId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.Time1):
                                        if (CopyFromDocument(colDef, doc.Time1IsCommon))
                                        {
                                            if (entry.Time1 != doc.Time1)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Time1)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.Time1} to {entry.Time1}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.Time1IsCommon, defaultsToForm))
                                        {
                                            if (entry.Time1 != tabEntry.Time1)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Time1)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.Time1} to {entry.Time1}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.Duration):
                                        if (CopyFromDocument(colDef, doc.DurationIsCommon))
                                        {
                                            if (entry.Duration != doc.Duration)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Duration)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.Duration} to {entry.Duration}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.DurationIsCommon, defaultsToForm))
                                        {
                                            if (entry.Duration != tabEntry.Duration)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Duration)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.Duration} to {entry.Duration}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.DurationUnitId):
                                        if (CopyFromDocument(colDef, doc.DurationUnitIsCommon))
                                        {
                                            if (entry.DurationUnitId != doc.DurationUnitId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.DurationUnitId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.DurationUnitId} to {entry.DurationUnitId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.DurationUnitIsCommon, defaultsToForm))
                                        {
                                            if (entry.DurationUnitId != tabEntry.DurationUnitId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.DurationUnitId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.DurationUnitId} to {entry.DurationUnitId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.Time2):
                                        if (CopyFromDocument(colDef, doc.Time2IsCommon))
                                        {
                                            if (entry.Time2 != doc.Time2)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Time2)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.Time2} to {entry.Time2}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.Time2IsCommon, defaultsToForm))
                                        {
                                            if (entry.Time2 != tabEntry.Time2)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Time2)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.Time2} to {entry.Time2}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.NotedDate):
                                        if (CopyFromDocument(colDef, doc.NotedDateIsCommon))
                                        {
                                            if (entry.NotedDate != doc.NotedDate)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.NotedDate)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.NotedDate} to {entry.NotedDate}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.NotedDateIsCommon, defaultsToForm))
                                        {
                                            if (entry.NotedDate != tabEntry.NotedDate)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.NotedDate)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.NotedDate} to {entry.NotedDate}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.ExternalReference):
                                        if (CopyFromDocument(colDef, doc.ExternalReferenceIsCommon))
                                        {
                                            if (entry.ExternalReference != doc.ExternalReference)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ExternalReference)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.ExternalReference} to {entry.ExternalReference}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ExternalReferenceIsCommon, defaultsToForm))
                                        {
                                            if (entry.ExternalReference != tabEntry.ExternalReference)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ExternalReference)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.ExternalReference} to {entry.ExternalReference}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.ReferenceSourceId):
                                        if (CopyFromDocument(colDef, doc.ReferenceSourceIsCommon))
                                        {
                                            if (entry.ReferenceSourceId != doc.ReferenceSourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ReferenceSourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.ReferenceSourceId} to {entry.ReferenceSourceId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ReferenceSourceIsCommon, defaultsToForm))
                                        {
                                            if (entry.ReferenceSourceId != tabEntry.ReferenceSourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ReferenceSourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.ReferenceSourceId} to {entry.ReferenceSourceId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.InternalReference):
                                        if (CopyFromDocument(colDef, doc.InternalReferenceIsCommon))
                                        {
                                            if (entry.InternalReference != doc.InternalReference)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.InternalReference)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.InternalReference} to {entry.InternalReference}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.InternalReferenceIsCommon, defaultsToForm))
                                        {
                                            if (entry.InternalReference != tabEntry.InternalReference)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.InternalReference)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.InternalReference} to {entry.InternalReference}");
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            });

            #endregion

            return docs;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<DocumentForSave> docs, bool returnIds)
        {
            #region Validation

            // Find lines with duplicate Ids
            var duplicateLineIds = docs.SelectMany(doc => doc.Lines) // All lines
                .Where(line => line.Id != 0).GroupBy(line => line.Id).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(line => line, line => line.Id); // to dictionary

            // Find entries with duplicate Ids
            var duplicateEntryIds = docs.SelectMany(doc => doc.Lines).SelectMany(line => line.Entries)
                .Where(entry => entry.Id != 0).GroupBy(entry => entry.Id).Where(g => g.Count() > 1)
                .SelectMany(g => g).ToDictionary(entry => entry, entry => entry.Id);

            // Find documents with duplicate Serial numbers
            var duplicateSerialNumbers = docs.Where(doc => doc.SerialNumber > 0)
                .GroupBy(doc => doc.SerialNumber).Where(g => g.Count() > 1).SelectMany(g => g)
                .ToDictionary(doc => doc, doc => doc.SerialNumber.Value);

            var settings = await _behavior.Settings();
            var defs = await _behavior.Definitions();
            var docDef = await Definition();
            var manualLineDefId = defs.ManualLinesDefinitionId;
            var meta = await GetMetadataForSave(cancellation: default);

            var lineDefs = new Dictionary<LineDefinitionForClient, Dictionary<string, LineDefinitionColumnForClient[]>>();
            LineDefinitionColumnForClient GetColumnDef(LineDefinitionForClient lineDef, string colName, int entryIndex)
            {
                if (lineDef == null)
                {
                    return null;
                }

                if (!lineDefs.TryGetValue(lineDef, out Dictionary<string, LineDefinitionColumnForClient[]> colDefs))
                {
                    colDefs = new Dictionary<string, LineDefinitionColumnForClient[]>();

                    foreach (var colDefGroup in lineDef.Columns.GroupBy(e => e.ColumnName))
                    {
                        var columns = new LineDefinitionColumnForClient[lineDef.Entries.Count];
                        foreach (var colDef in colDefGroup)
                        {
                            if (colDef.EntryIndex >= 0 && colDef.EntryIndex < lineDef.Entries.Count)
                            {
                                columns[colDef.EntryIndex] = colDef;
                            }
                        }
                        colDefs.Add(colDefGroup.Key, columns);
                    }

                    lineDefs.Add(lineDef, colDefs);
                }

                if (colDefs.TryGetValue(colName, out LineDefinitionColumnForClient[] array) &&
                    (entryIndex >= 0 || entryIndex < array.Length))
                {
                    return array[entryIndex];
                }

                return null;
            }

            ///////// Document Validation
            for (int docIndex = 0; docIndex < docs.Count; docIndex++)
            {
                var doc = docs[docIndex];

                if (!docDef.IsOriginalDocument)
                {
                    // If not an original document, the serial number is required
                    if (doc.SerialNumber == null || doc.SerialNumber == 0)
                    {
                        ModelState.AddError($"[{docIndex}].{nameof(doc.SerialNumber)}",
                            _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Document_SerialNumber"]]);
                    }
                    else if (duplicateSerialNumbers.ContainsKey(doc))
                    {
                        var serial = duplicateSerialNumbers[doc];
                        ModelState.AddError($"[{docIndex}].{nameof(doc.SerialNumber)}",
                            _localizer["Error_TheSerialNumber0IsDuplicated", FormatSerial(serial, docDef.Prefix, docDef.CodeWidth)]);
                    }
                }

                if (docDef.PostingDateVisibility != null && doc.PostingDate != null)
                {
                    //// Date cannot be in the future unless document is of type Plan
                    //if (docDef.DocumentType != DocumentTypes.Plan && doc.PostingDate > DateTime.Today.AddDays(1))
                    //{
                    //    ModelState.AddError($"[{docIndex}].{nameof(doc.PostingDate)}",
                    //        _localizer["Error_DateCannotBeInTheFuture"]);
                    //}

                    //// Date cannot be before archive date
                    //if (doc.PostingDate <= settings.ArchiveDate && docDef.DocumentType >= 2)
                    //{
                    //    var calendar = Calendar ?? settings.PrimaryCalendar;
                    //    var archiveDate = CalendarUtilities.FormatDate(settings.ArchiveDate, _localizer, settings.DateFormat, calendar);
                    //    ModelState.AddError($"[{docIndex}].{nameof(doc.PostingDate)}",
                    //        _localizer["Error_DateCannotBeBeforeArchiveDate1", archiveDate]);
                    //}
                }

                ////////// LineDefinitionEntries Validation
                if (doc.LineDefinitionEntries != null)
                {
                    // Remove duplicates
                    var duplicateTabEntries = doc.LineDefinitionEntries
                        .GroupBy(e => (e.LineDefinitionId, e.EntryIndex))
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g)
                        .ToHashSet();

                    // Make sure EntryIndex is not below 0
                    for (int tabEntryIndex = 0; tabEntryIndex < doc.LineDefinitionEntries.Count; tabEntryIndex++)
                    {
                        var tabEntry = doc.LineDefinitionEntries[tabEntryIndex];
                        if (tabEntry.EntryIndex < 0)
                        {
                            var path = $"[{docIndex}].{nameof(doc.LineDefinitionEntries)}[{tabEntryIndex}].{nameof(tabEntry.EntryIndex)}";
                            var msg = "Entry index cannot be negative";
                            ModelState.AddError(path, msg);
                        }

                        if (duplicateTabEntries.Contains(tabEntry))
                        {
                            var path = $"[{docIndex}].{nameof(doc.LineDefinitionEntries)}[{tabEntryIndex}].{nameof(tabEntry.EntryIndex)}";
                            var msg = $"Entry index {tabEntry.EntryIndex} is duplicated for the same LineDefinitionId '{tabEntry.LineDefinitionId}'";
                            ModelState.AddError(path, msg);
                        }
                    }

                    // TODO: PostingDate validation
                }

                // lines and entries validation, we deal with the lines one definitionId at a time
                foreach (var linesGroup in doc.Lines.GroupBy(e => e.DefinitionId.Value))
                {
                    // Retrieve the line definition
                    if (!defs.Lines.TryGetValue(linesGroup.Key, out LineDefinitionForClient lineDef)) // We checked earlier if this is null
                    {
                        foreach (var line in linesGroup)
                        {
                            ModelState.AddError(LinePath(docIndex, line.EntityMetadata.OriginalIndex, nameof(Line.DefinitionId)),
                                _localizer["Error_UnknownLineDefinitionId0", line.DefinitionId]);
                        }

                        continue; // No point to keep going
                    }

                    // Collect the line definition entries that belong to this line definition in a neat array
                    var defaultsToForm = lineDef.ViewDefaultsToForm;
                    var tabEntries = new DocumentLineDefinitionEntryForSave[lineDef.Entries.Count];
                    foreach (var tabEntry in doc.LineDefinitionEntries.Where(e => e.LineDefinitionId == linesGroup.Key))
                    {
                        if (tabEntry.EntryIndex >= 0 && tabEntry.EntryIndex < lineDef.Entries.Count)
                        {
                            tabEntries[tabEntry.EntryIndex.Value] = tabEntry;
                        }
                    }

                    foreach (var line in linesGroup)
                    {
                        int lineIndex = line.EntityMetadata.OriginalIndex;

                        // Prevent duplicate line Ids
                        if (duplicateLineIds.ContainsKey(line))
                        {
                            // This error indicates a bug
                            var id = duplicateLineIds[line];
                            ModelState.AddError(LinePath(docIndex, lineIndex, nameof(Line.Id)),
                                _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                        }

                        // PostingDate validation
                        if (line.PostingDate != null)
                        {
                            var tabEntry = (tabEntries.Length > 0 ? tabEntries[0] : null) ?? DefaultTabEntryForSave;
                            var columnDef = GetColumnDef(lineDef, nameof(Line.PostingDate), 0);
                            if (
                                columnDef != null &&
                                !CopyFromTab(columnDef, tabEntry.PostingDateIsCommon.Value, lineDef.ViewDefaultsToForm) &&
                                !CopyFromDocument(columnDef, doc.PostingDateIsCommon.Value)
                               )
                            {
                                // Line date cannot be in the future unless the document is of type Plan
                                if (lineDef.LineType >= LineTypes.Event && line.PostingDate > DateTime.Today.AddDays(1))
                                {
                                    ModelState.AddError(LinePath(docIndex, lineIndex, nameof(Line.PostingDate)),
                                        _localizer["Error_DateCannotBeInTheFuture"]);
                                }

                                // Line date cannot be before archive when the document is plan or regulatory
                                if (line.PostingDate <= settings.ArchiveDate && lineDef.LineType >= LineTypes.Event)
                                {
                                    var calendar = Calendar ?? settings.PrimaryCalendar;
                                    var archiveDate = CalendarUtilities.FormatDate(settings.ArchiveDate, _localizer, settings.DateFormat, calendar);
                                    ModelState.AddError(LinePath(docIndex, lineIndex, nameof(Line.PostingDate)),
                                        _localizer["Error_DateCannotBeBeforeArchiveDate1", archiveDate]);
                                }
                            }
                        }

                        foreach (var entry in line.Entries)
                        {
                            var entryIndex = entry.EntityMetadata.OriginalIndex;

                            // Prevent duplicate entry Ids
                            if (duplicateEntryIds.ContainsKey(entry))
                            {
                                var id = duplicateEntryIds[entry];
                                ModelState.AddError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Id)),
                                    _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                            }

                            // Value must be positive
                            if (entry.Value < 0)
                            {
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    string fieldLabel = entry.Direction == -1 ? _localizer["Credit"] : _localizer["Debit"];

                                    ModelState.AddError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Value)),
                                        _localizer["Error_TheField0CannotBeNegative", fieldLabel]);
                                }
                            }

                            // MonetaryValue must be positive
                            if (entry.MonetaryValue < 0)
                            {
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    ModelState.AddError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.MonetaryValue)),
                                        _localizer["Error_TheField0CannotBeNegative", _localizer["Entry_MonetaryValue"]]);
                                }
                            }

                            // Quantity must be positive
                            if (entry.Quantity < 0)
                            {
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    var fieldLabel = _localizer["Entry_Quantity"];
                                    var msg = _localizer["Error_TheField0CannotBeNegative", fieldLabel];
                                    var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Quantity));

                                    ModelState.AddError(path, msg);
                                }
                            }

                            // CenterId is required in Entries table
                            if (entry.CenterId == null)
                            {
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    var fieldLabel = _localizer["Entry_Center"];
                                    var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];
                                    var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CenterId));

                                    ModelState.AddError(path, msg);
                                }
                                else
                                {
                                    var colDef = lineDef.Columns.FirstOrDefault(e => e.EntryIndex == entryIndex && e.ColumnName == nameof(Entry.CenterId));
                                    if (colDef != null)
                                    {
                                        if (CopyFromDocument(colDef, doc.CenterIsCommon))
                                        {
                                            var fieldLabel = settings.Localize(docDef.CenterLabel, docDef.CenterLabel2, docDef.CenterLabel3) ?? _localizer["Document_Center"];
                                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];
                                            var path = DocumentPath(docIndex, nameof(Document.CenterId));

                                            ModelState.AddError(path, msg);
                                        }
                                        else
                                        {
                                            var tabEntry = (entryIndex < tabEntries.Length ? tabEntries[entryIndex] : null) ?? DefaultTabEntryForSave;

                                            var fieldLabel = settings.Localize(colDef.Label, colDef.Label2, colDef.Label3) ?? _localizer["Entry_Center"];
                                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];

                                            if (CopyFromTab(colDef, tabEntry.CenterIsCommon, defaultsToForm))
                                            {
                                                if (tabEntry != DefaultTabEntryForSave) // The default one has no index
                                                {
                                                    var index = tabEntry.EntityMetadata.OriginalIndex;
                                                    var path = LineDefinitionEntryPath(docIndex, index, nameof(Document.CenterId));

                                                    ModelState.AddError(path, msg);
                                                }
                                            }
                                            else
                                            {
                                                var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CenterId));

                                                ModelState.AddError(path, msg);
                                            }
                                        }
                                    }
                                }
                            }

                            // CurrencyId is required in Entries table
                            if (entry.CurrencyId == null)
                            {
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    var fieldLabel = _localizer["Entry_Currency"];
                                    var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];
                                    var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CurrencyId));

                                    ModelState.AddError(path, msg);
                                }
                                else
                                {
                                    var colDef = lineDef.Columns.FirstOrDefault(e => e.EntryIndex == entryIndex && e.ColumnName == nameof(Entry.CurrencyId));
                                    if (colDef != null)
                                    {
                                        if (CopyFromDocument(colDef, doc.CurrencyIsCommon))
                                        {
                                            var fieldLabel = settings.Localize(docDef.CurrencyLabel, docDef.CurrencyLabel2, docDef.CurrencyLabel3) ?? _localizer["Entry_Currency"];
                                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];
                                            var path = DocumentPath(docIndex, nameof(Document.CurrencyId));

                                            ModelState.AddError(path, msg);
                                        }
                                        else
                                        {
                                            var tabEntry = (entryIndex < tabEntries.Length ? tabEntries[entryIndex] : null) ?? DefaultTabEntryForSave;

                                            var fieldLabel = settings.Localize(colDef.Label, colDef.Label2, colDef.Label3) ?? _localizer["Entry_Currency"];
                                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];

                                            if (CopyFromTab(colDef, tabEntry.CurrencyIsCommon, defaultsToForm))
                                            {
                                                if (tabEntry != DefaultTabEntryForSave) // The default one has no index
                                                {
                                                    var index = tabEntry.EntityMetadata.OriginalIndex;
                                                    var path = LineDefinitionEntryPath(docIndex, index, nameof(Document.CurrencyId));

                                                    ModelState.AddError(path, msg);
                                                }
                                            }
                                            else
                                            {
                                                var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CurrencyId));

                                                ModelState.AddError(path, msg);
                                            }
                                        }
                                    }
                                }
                            }

                            // If the currency is functional, value must equal monetary value
                            if (entry.CurrencyId == settings.FunctionalCurrencyId && entry.Value != entry.MonetaryValue)
                            {
                                var currencyName = settings
                                    .Localize(settings.FunctionalCurrencyName,
                                                settings.FunctionalCurrencyName2,
                                                settings.FunctionalCurrencyName3);

                                // TODO: Use the proper field name from definition, instead of "Amount"
                                ModelState.AddError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.MonetaryValue)),
                                    _localizer["TheAmount0DoesNotMatchTheValue1EvenThoughBothIn2", entry.MonetaryValue ?? 0, entry.Value ?? 0, currencyName]);
                            }
                        }
                    }
                }

                ///////// Attachment Validation
                if (doc.Attachments != null)
                {
                    for (var attIndex = 0; attIndex < doc.Attachments.Count; attIndex++)
                    {
                        var att = doc.Attachments[attIndex];

                        if (att.Id != 0 && att.File != null)
                        {
                            ModelState.AddError($"[{docIndex}].{nameof(doc.Attachments)}[{attIndex}].File",
                                _localizer["Error_OnlyNewAttachmentsCanIncludeFileBytes"]);
                        }

                        if (att.Id == 0 && att.File == null)
                        {
                            ModelState.AddError($"[{docIndex}].{nameof(doc.Attachments)}[{attIndex}].File",
                                _localizer["Error_NewAttachmentsMustIncludeFileBytes"]);
                        }

                        if (att.File != null && att.File.Length == 0)
                        {
                            ModelState.AddError($"[{docIndex}].{nameof(doc.Attachments)}[{attIndex}].File",
                                _localizer["Error_AttachmentCannotBeAnEmptyFile"]);
                        }
                    }
                }
            }

            // Just in case an entry's CurrencyId or CenterId is still null and it was not 
            // captured by model validation because perhaps the field is not visible, we 
            // throw a 400 (otherwise SQL will throw an error upon save since they are NOT NULL)
            if (ModelState.IsValid)
            {
                docs.ForEach(doc =>
                {
                    doc?.Lines?.ForEach(line =>
                    {
                        line?.Entries?.ForEach(entry =>
                        {
                            if (entry?.CurrencyId == null)
                            {
                                var docIndex = doc.EntityMetadata.OriginalIndex;
                                var lineIndex = line.EntityMetadata.OriginalIndex;
                                var entryIndex = entry.EntityMetadata.OriginalIndex;

                                var entryPath = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CurrencyId));
                                throw new ServiceException($"[Bug] the CurrencyId for {entryPath} was never set.");
                            }

                            if (entry?.CenterId == null)
                            {
                                var docIndex = doc.EntityMetadata.OriginalIndex;
                                var lineIndex = line.EntityMetadata.OriginalIndex;
                                var entryIndex = entry.EntityMetadata.OriginalIndex;

                                var entryPath = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CenterId));
                                throw new ServiceException($"[Bug] the CenterId for {entryPath} was never set.");
                            }
                        });
                    });
                });
            }

            // Finally: Remove empty tab entries
            // Should technically be in preprocess, but it would mess up the validation then
            if (ModelState.IsValid)
            {
                docs.ForEach(doc => doc.LineDefinitionEntries?.RemoveAll(EqualsDefaultTabEntry));
            }

            #endregion

            _blobsToSave = BaseUtil.ExtractAttachments(docs, e => e.Attachments, AttachmentBlobName).ToList();

            try
            {
                // Save the documents
                var (result, notificationInfos, fileIdsToDelete) = await _behavior.Repository.Documents__Save(
                        DefinitionId,
                        documents: docs,
                        returnIds: returnIds,
                        validateOnly: ModelState.IsError,
                        top: ModelState.RemainingErrors,
                        userId: UserId);

                // Validation
                AddErrorsAndThrowIfInvalid(result.Errors);

                _notificationInfos = notificationInfos;
                _blobsToDelete = fileIdsToDelete.Select(AttachmentBlobName).ToList();

                // Return the new Ids
                return result.Ids;
            }
            catch (CustomScriptException ex) when (ex.IsScriptBug && docDef.State == DefStates.Visible)
            {
                string lineDefName = "(Unkown)";
                if (ex.LineDefinitionId != null)
                {
                    var lineDef = await LineDefinition(ex.LineDefinitionId.Value);
                    lineDefName = lineDef.TitleSingular;
                }

                await _behavior.LogCustomScriptBug(
                                ex,
                                collection: nameof(Line),
                                definitionId: ex.LineDefinitionId,
                                defTitle: lineDefName,
                                scriptName: "Validate Script",
                                entities: docs
                            );

                throw; // Bubble up to the client
            }
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<DocumentForSave> entities, IReadOnlyList<Document> data)
        {
            using var trx = TransactionFactory.Suppress();

            // Notify affected users
            _clientProxy.UpdateInboxStatuses(TenantId, _notificationInfos);

            // Delete the file Ids retrieved earlier if any
            if (_blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(TenantId, _blobsToDelete);
            }

            // Save new blobs if any
            if (_blobsToSave.Any())
            {
                await _blobService.SaveBlobsAsync(TenantId, _blobsToSave);
            }
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            var (result, fileIdsToDelete) = await _behavior.Repository.Documents__Delete(
                definitionId: DefinitionId,
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Non-transactional side effects:
            // (1) Inbox notifications
            _clientProxy.UpdateInboxStatuses(TenantId, result.InboxStatuses);

            // (2) Delete the file Ids retrieved earlier if any
            if (fileIdsToDelete.Any())
            {
                var blobsToDelete = fileIdsToDelete.Select(AttachmentBlobName);
                await _blobService.DeleteBlobsAsync(TenantId, blobsToDelete);
            }
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse($"{nameof(Document.SerialNumber)} desc");
            return Task.FromResult(result);
        }

        private static string AttachmentBlobName(string guid)
        {
            return $"Attachments/{guid}";
        }

        protected override ExpressionSelect ParseSelect(string select)
        {
            if (select == null)
            {
                return null;
            }

            // We provide a shorthand notation for common and huge select
            // strings, this one is usually requested from the document details
            // screen and it contains hundreds of atoms
            string shorthand = "$Details";
            ExpressionSelect result;
            if (select.Contains(shorthand))
            {
                // Use the built in expansion for shorthand
                result = _detailsSelectExpression.Clone();
                select = select.Replace(shorthand, "");

                // Add any additional atoms
                ExpressionSelect remainderExpression = base.ParseSelect(select);
                if (remainderExpression != null)
                {
                    result.AddAll(remainderExpression);
                }
            }
            else
            {
                result = base.ParseSelect(select);
            }

            return result;
        }

        protected override IEnumerable<string> AdditionalSelectForExport()
        {
            // Those are needed to get and set the entity values, even though they are not exported or imported themselves
            yield return $"{nameof(Document.Lines)}.{nameof(Line.DefinitionId)}";
            yield return $"{nameof(Document.LineDefinitionEntries)}.{nameof(DocumentLineDefinitionEntry.EntryIndex)}";
            yield return $"{nameof(Document.LineDefinitionEntries)}.{nameof(DocumentLineDefinitionEntry.LineDefinitionId)}";
        }

        protected override async Task<MappingInfo> GetDefaultMapping(TypeMetadata docMetaForSave, TypeMetadata docMeta, CancellationToken cancellation)
        {
            var defs = await _behavior.Definitions(cancellation);

            // Step #1 - Add the header properties of the document
            var def = await Definition(cancellation);
            var docProps = GetDefaultSimplePropertyMappings(docMetaForSave, docMeta, 0);
            int nextAvailableIndex = docProps.Count;

            // Step #2 - Add the tabs
            var lineDefs = defs.Lines;
            var settings = await _behavior.Settings(cancellation);

            // Some metadata objects to help us later
            IMetadataOverridesProvider overrides = await FactBehavior.GetMetadataOverridesProvider(cancellation);

            var lineMeta = _metadata.GetMetadata(TenantId, typeof(Line), null, overrides);
            var lineMetaForSave = _metadata.GetMetadata(TenantId, typeof(LineForSave), null, overrides);
            var entryMeta = _metadata.GetMetadata(TenantId, typeof(Entry), null, overrides);
            var entryMetaForSave = _metadata.GetMetadata(TenantId, typeof(EntryForSave), null, overrides);
            var tabMeta = _metadata.GetMetadata(TenantId, typeof(DocumentLineDefinitionEntry), null, overrides);
            var tabMetaForSave = _metadata.GetMetadata(TenantId, typeof(DocumentLineDefinitionEntryForSave), null, overrides);

            var linesCollectionPropertyMeta = docMeta.CollectionProperty(nameof(Document.Lines));
            var linesCollectionPropertyMetaForSave = docMetaForSave.CollectionProperty(nameof(Document.Lines));
            var entriesCollectionPropertyMeta = lineMeta.CollectionProperty(nameof(Line.Entries));
            var entriesCollectionPropertyMetaForSave = lineMetaForSave.CollectionProperty(nameof(Line.Entries));

            string selectPrefixForTabHeaderProperties = nameof(Document.LineDefinitionEntries);
            string selectPrefixForSmartEntries = nameof(Line.Entries);
            string selectForManualLines = $"{nameof(Document.Lines)}.{nameof(Line.Entries)}";

            var tabFkNames = tabMeta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);
            var entryFkNames = entryMeta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);
            var lineFkNames = lineMeta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);

            var tabMappings = new List<MappingInfo>();
            foreach (var lineDefId in def.LineDefinitions.Select(e => e.LineDefinitionId).Where(id => lineDefs.ContainsKey(id)))
            {
                var lineDef = lineDefs[lineDefId];
                string TabDisplay() => settings.Localize(lineDef.TitlePlural, lineDef.TitlePlural2, lineDef.TitlePlural3);

                // Special handling for manual lines
                if (lineDefId == defs.ManualLinesDefinitionId)
                {
                    var adjustmentSimpleProps = GetDefaultSimplePropertyMappings(entryMetaForSave, entryMeta, nextAvailableIndex);
                    nextAvailableIndex += adjustmentSimpleProps.Count;
                    tabMappings.Add(new MappingInfo(entryMetaForSave, entryMeta, adjustmentSimpleProps, new List<MappingInfo>(), entriesCollectionPropertyMetaForSave, entriesCollectionPropertyMeta)
                    {
                        // Like onNewManualEntry
                        CreateEntity = () => new Entry { Id = 0, Direction = 1 },
                        GetEntitiesForRead = (Entity entity) =>
                        {
                            var doc = entity as Document;
                            return doc.Lines.Where(e => e.DefinitionId == lineDefId).SelectMany(e => e.Entries);
                        },
                        GetOrCreateListForSave = (Entity entity) =>
                        {
                            var doc = entity as DocumentForSave;
                            if (doc.EntityMetadata.ManualLine is not LineForSave manualLine)
                            {
                                manualLine = doc.Lines.FirstOrDefault(e => e.DefinitionId == lineDefId);
                                if (manualLine == null)
                                {
                                    manualLine = new LineForSave
                                    {
                                        DefinitionId = lineDefId,
                                        Entries = new List<EntryForSave>()
                                    };

                                    doc.Lines.Add(manualLine);
                                }

                                doc.EntityMetadata.ManualLine = manualLine;
                            }

                            return manualLine.Entries;
                        },
                        Display = DefinitionId == defs.ManualJournalVouchersDefinitionId ? () => _localizer["Entries"] : TabDisplay,
                        Select = selectForManualLines,
                    });

                    continue;
                }

                // First: add the tab headers
                if (!lineDef.ViewDefaultsToForm)
                {
                    foreach (var column in lineDef.Columns.Where(e => e.InheritsFromHeader > InheritsFrom.None))
                    {
                        if (column.ReadOnlyState <= 0)
                        {
                            continue; // Those are readonly and most likely auto-computed
                        }

                        var colName = column.ColumnName;
                        var entryIndex = IsLineColumn(colName) ? 0 : column.EntryIndex;

                        var propMeta = tabMeta.Property(colName);
                        var propMetaForSave = tabMetaForSave.Property(colName);

                        Entity GetOrCreateEntityForSave(Entity entity)
                        {
                            var doc = entity as DocumentForSave;
                            var tabEntry = doc.LineDefinitionEntries.FirstOrDefault(e => e.LineDefinitionId == lineDefId && e.EntryIndex == entryIndex);
                            if (tabEntry == null)
                            {
                                tabEntry = MakeDefaultTabEntryForSave();
                                tabEntry.EntryIndex = entryIndex;
                                tabEntry.LineDefinitionId = lineDefId;
                                tabEntry.EntityMetadata.BaseEntity = doc;
                                doc.LineDefinitionEntries.Add(tabEntry);
                            }

                            return tabEntry;
                        }

                        Entity GetEntityForRead(Entity entity)
                        {
                            var doc = entity as Document;
                            var tabEntry = doc.LineDefinitionEntries.FirstOrDefault(e => e.LineDefinitionId == lineDefId && e.EntryIndex == entryIndex) ?? DefaultTabEntry;
                            return tabEntry;
                        }

                        string Display() => $"{TabDisplay()}: {settings.Localize(column.Label, column.Label2, column.Label3)}";
                        int index = nextAvailableIndex++;

                        if (tabFkNames.TryGetValue(colName, out NavigationPropertyMetadata navPropMetadata))
                        {
                            // Foreign Key
                            var keyPropMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty;
                            docProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, keyPropMetadata)
                            {
                                Display = Display,
                                Index = index,
                                SelectPrefix = selectPrefixForTabHeaderProperties,
                                GetTerminalEntityForSave = GetOrCreateEntityForSave,
                                GetTerminalEntityForRead = GetEntityForRead,
                            });
                        }
                        else
                        {
                            // Scalar Property
                            docProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                            {
                                Display = Display,
                                Index = index,
                                SelectPrefix = selectPrefixForTabHeaderProperties,
                                GetTerminalEntityForSave = GetOrCreateEntityForSave,
                                GetTerminalEntityForRead = GetEntityForRead,
                            });
                        }

                        // Property Is Common
                        var isCommonPropName = (colName.EndsWith("Id") ? colName[0..^2] : colName) + "IsCommon";
                        var propIsCommonMeta = tabMeta.Property(isCommonPropName); // What about UnitId
                        var propIsCommonMetaForSave = tabMetaForSave.Property(isCommonPropName);
                        docProps.Add(new PropertyMappingInfo(propIsCommonMeta, propIsCommonMetaForSave)
                        {
                            Index = nextAvailableIndex++,
                            Display = () => $"{TabDisplay()}: {_localizer["Field0IsCommon", settings.Localize(column.Label, column.Label2, column.Label3)]}",
                            SelectPrefix = selectPrefixForTabHeaderProperties,
                            GetTerminalEntityForSave = GetOrCreateEntityForSave,
                            GetTerminalEntityForRead = GetEntityForRead,
                        });
                    }
                }

                // Second: add the line properties
                var pivotedLineProps = new List<PropertyMappingInfo>(); // Flattens out the entry properties with the line properties, just like the UI screen
                foreach (var column in lineDef.Columns)
                {
                    if (column.ReadOnlyState <= 0)
                    {
                        continue; // Those are readonly and most likely auto-computed
                    }

                    var colName = column.ColumnName;
                    string Display() => settings.Localize(column.Label, column.Label2, column.Label3);
                    int index = nextAvailableIndex++;
                    if (IsLineColumn(colName))
                    {
                        // Line Properties
                        var propMeta = lineMeta.Property(colName);
                        var propMetaForSave = lineMetaForSave.Property(colName);
                        if (lineFkNames.TryGetValue(colName, out NavigationPropertyMetadata navPropMetadata))
                        {
                            var keyPropMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty;
                            pivotedLineProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, keyPropMetadata)
                            {
                                Index = index,
                                Display = Display,
                            });
                        }
                        else
                        {
                            pivotedLineProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                            {
                                Index = index,
                                Display = Display
                            });
                        }
                    }
                    else
                    {
                        // Entry Properties
                        var propMeta = entryMeta.Property(colName);
                        var propMetaForSave = entryMetaForSave.Property(colName);

                        Entity GetEntityForRead(Entity entity)
                        {
                            var line = entity as Line;
                            if (line.Entries.Count > column.EntryIndex)
                            {
                                return line.Entries[column.EntryIndex];
                            }
                            else
                            {
                                return null; // Should return a default entry
                            }
                        }

                        Entity GetOrCreateEntityForSave(Entity entity)
                        {
                            var line = entity as LineForSave;
                            if (line.Entries.Count > column.EntryIndex)
                            {
                                return line.Entries[column.EntryIndex];
                            }
                            else
                            {
                                return null; // Should return a default entry
                            }
                        }

                        if (entryFkNames.TryGetValue(colName, out NavigationPropertyMetadata navPropMetadata))
                        {
                            var keyPropMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty;
                            pivotedLineProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, keyPropMetadata)
                            {
                                Index = index,
                                Display = Display,
                                SelectPrefix = selectPrefixForSmartEntries,
                                GetTerminalEntityForRead = GetEntityForRead,
                                GetTerminalEntityForSave = GetOrCreateEntityForSave
                            });
                        }
                        else
                        {
                            pivotedLineProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                            {
                                Index = index,
                                Display = Display,
                                SelectPrefix = selectPrefixForSmartEntries,
                                GetTerminalEntityForRead = GetEntityForRead,
                                GetTerminalEntityForSave = GetOrCreateEntityForSave
                            });
                        }
                    }
                }

                tabMappings.Add(new MappingInfo(lineMetaForSave, lineMeta, pivotedLineProps, new List<MappingInfo>(), linesCollectionPropertyMetaForSave, linesCollectionPropertyMeta)
                {
                    CreateEntity = () =>
                    {
                        var line = new LineForSave
                        {
                            DefinitionId = lineDefId,
                            Boolean1 = false,
                            Entries = new List<EntryForSave>(),
                        };

                        foreach (var entry in lineDef.Entries)
                        {
                            line.Entries.Add(new Entry
                            {
                                Id = 0,
                                Direction = entry.Direction,
                                Value = 0,

                                EntityMetadata = new EntityMetadata { BaseEntity = line }
                            });
                        }

                        return line;
                    },
                    GetEntitiesForRead = (Entity entity) =>
                    {
                        var doc = entity as Document;
                        return doc.Lines.Where(e => e.DefinitionId == lineDefId);
                    },
                    Display = TabDisplay
                });
            }

            return new MappingInfo(docMetaForSave, docMeta, docProps, tabMappings, null, null)
            {
                CreateEntity = () => new DocumentForSave
                {
                    PostingDateIsCommon = true,
                    MemoIsCommon = true,
                    CurrencyIsCommon = true,
                    CenterIsCommon = true,

                    AgentIsCommon = true,
                    ResourceIsCommon = true,
                    NotedAgentIsCommon = true,
                    NotedResourceIsCommon = true,

                    QuantityIsCommon = true,
                    UnitIsCommon = true,
                    Time1IsCommon = true,
                    DurationIsCommon = true,
                    DurationUnitIsCommon = true,
                    Time2IsCommon = true,
                    NotedDateIsCommon = true,

                    ExternalReferenceIsCommon = true,
                    ReferenceSourceIsCommon = true,
                    InternalReferenceIsCommon = true,

                    LineDefinitionEntries = new List<DocumentLineDefinitionEntryForSave>(),
                    Lines = new List<LineForSave>(),
                },
            };
        }

        #region Helper Functions

        private static bool CopyFromDocument(LineDefinitionColumnForClient colDef, bool? docIsCommon)
        {
            return colDef.InheritsFromHeader >= InheritsFrom.DocumentHeader && (docIsCommon ?? false);
        }

        private static bool CopyFromTab(LineDefinitionColumnForClient colDef, bool? tabIsCommon, bool isForm)
        {
            return !isForm && colDef.InheritsFromHeader >= InheritsFrom.TabHeader && (tabIsCommon ?? false);
        }

        private static string FormatSerial(int serial, string prefix, int codeWidth)
        {
            var result = serial.ToString();
            if (result.Length < codeWidth)
            {
                result = "00000000000000000".Truncate(codeWidth - result.Length) + result;
            }

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                result = prefix + result;
            }

            return result;
        }

        private static string DocumentPath(int docIndex, string propName)
        {
            return $"[{docIndex}].{propName}";
        }

        private static string LineDefinitionEntryPath(int docIndex, int index, string propName)
        {
            return $"[{docIndex}].{nameof(Document.LineDefinitionEntries)}[{index}].{propName}";
        }

        private static string EntryPath(int docIndex, int lineIndex, int entryIndex, string propName)
        {
            return $"[{docIndex}].{nameof(Document.Lines)}[{lineIndex}].{nameof(Line.Entries)}[{entryIndex}].{propName}";
        }

        private static string LinePath(int docIndex, int lineIndex, string propName)
        {
            return $"[{docIndex}].{nameof(Document.Lines)}[{lineIndex}].{propName}";
        }

        private static bool IsLineColumn(string colName)
        {
            return colName switch
            {
                nameof(Line.Memo) or
                nameof(Line.PostingDate) or
                nameof(Line.Boolean1) or
                nameof(Line.Decimal1) or
                nameof(Line.Decimal2) or
                nameof(Line.Text1) or
                nameof(Line.Text2) => true,
                _ => false,
            };
        }

        #endregion

        #region Import/Export Lines

        public async Task<Stream> CsvTemplateForLines(int lineDefId, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Get the metadatas
            IMetadataOverridesProvider overrides = await FactBehavior.GetMetadataOverridesProvider(cancellation);
            var lineMetaForSave = _metadata.GetMetadata(TenantId, typeof(LineForSave), null, overrides);
            var lineMeta = _metadata.GetMetadata(TenantId, typeof(Line), null, overrides);

            // Get the default mapping, auto calculated from the entity for save metadata
            var mapping = await GetDefaultMappingForLines(lineMetaForSave, lineMeta, lineDefId, cancellation);

            // Get the headers from the mapping
            string[] headers = HeadersFromMapping(mapping);

            // Create a CSV file containing only those headers
            var data = new List<string[]> { headers };
            var packager = new CsvPackager();
            return packager.Package(data);
        }

        public async Task<LinesResult> ParseLines(Stream fileStream, int lineDefId, string fileName, string contentType, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Validation
            if (fileStream == null)
            {
                throw new ServiceException(_localizer["Error_NoFileWasUploaded"]);
            }

            // Extract the raw data from the file stream
            IEnumerable<string[]> data = BaseUtil.ExtractStringsFromFile(fileStream, fileName, contentType, _localizer);
            if (!data.Any())
            {
                throw new ServiceException(_localizer["Error_UploadedFileWasEmpty"]);
            }

            // Map the columns
            var importErrors = new ImportErrors();
            var headers = data.First();
            MappingInfo mapping = await MappingFromHeadersForLines(headers, lineDefId, importErrors, cancellation);
            importErrors.ThrowIfInvalid(_localizer);

            // Parse the data to entities
            var lines = await _parser.ParseAsync<LineForSave>(data.Skip(1), mapping, importErrors);

            var (accounts, resources, agents, entryTypes, centers, currencies, units) =
                await _behavior.Repository.EntryList_IncludeRelatedEntities(lines.ToList(), cancellation);

            return new LinesResult(lines.ToList(), accounts, resources, agents, entryTypes, centers, currencies, units);
        }

        private async Task<MappingInfo> GetDefaultMappingForLines(TypeMetadata lineMetaForSave, TypeMetadata lineMeta, int lineDefId, CancellationToken cancellation)
        {
            var defs = await _behavior.Definitions(cancellation);
            int nextAvailableIndex = 0;

            var lineDefs = defs.Lines;
            var settings = await _behavior.Settings(cancellation);

            // Some metadata objects to help us later
            IMetadataOverridesProvider overrides = await FactBehavior.GetMetadataOverridesProvider(cancellation);

            var entryMeta = _metadata.GetMetadata(TenantId, typeof(Entry), null, overrides);
            var entryMetaForSave = _metadata.GetMetadata(TenantId, typeof(EntryForSave), null, overrides);

            var entriesCollectionPropertyMeta = lineMeta.CollectionProperty(nameof(Line.Entries));
            var entriesCollectionPropertyMetaForSave = lineMetaForSave.CollectionProperty(nameof(Line.Entries));

            string selectPrefixForSmartEntries = nameof(Line.Entries);

            var entryFkNames = entryMeta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);
            var lineFkNames = lineMeta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);

            var lineDef = lineDefs[lineDefId];

            // Second: add the line properties
            var pivotedLineProps = new List<PropertyMappingInfo>(); // Flattens out the entry properties with the line properties, just like the UI screen
            foreach (var column in lineDef.Columns)
            {
                if (column.ReadOnlyState <= 0)
                {
                    continue; // Those are readonly and most likely auto-computed
                }

                var colName = column.ColumnName;
                string Display() => settings.Localize(column.Label, column.Label2, column.Label3);
                int index = nextAvailableIndex++;
                if (IsLineColumn(colName))
                {
                    // Line Properties
                    var propMeta = lineMeta.Property(colName);
                    var propMetaForSave = lineMetaForSave.Property(colName);
                    if (lineFkNames.TryGetValue(colName, out NavigationPropertyMetadata navPropMetadata))
                    {
                        var keyPropMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty;
                        pivotedLineProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, keyPropMetadata)
                        {
                            Index = index,
                            Display = Display,
                        });
                    }
                    else
                    {
                        pivotedLineProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                        {
                            Index = index,
                            Display = Display
                        });
                    }
                }
                else
                {
                    // Entry Properties
                    var propMeta = entryMeta.Property(colName);
                    var propMetaForSave = entryMetaForSave.Property(colName);

                    Entity GetEntityForRead(Entity entity)
                    {
                        var line = entity as Line;
                        if (line.Entries.Count > column.EntryIndex)
                        {
                            return line.Entries[column.EntryIndex];
                        }
                        else
                        {
                            return null; // Should return a default entry
                        }
                    }

                    Entity GetOrCreateEntityForSave(Entity entity)
                    {
                        var line = entity as LineForSave;
                        if (line.Entries.Count > column.EntryIndex)
                        {
                            return line.Entries[column.EntryIndex];
                        }
                        else
                        {
                            return null; // Should return a default entry
                        }
                    }

                    if (entryFkNames.TryGetValue(colName, out NavigationPropertyMetadata navPropMetadata))
                    {
                        var keyPropMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty;
                        pivotedLineProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, keyPropMetadata)
                        {
                            Index = index,
                            Display = Display,
                            SelectPrefix = selectPrefixForSmartEntries,
                            GetTerminalEntityForRead = GetEntityForRead,
                            GetTerminalEntityForSave = GetOrCreateEntityForSave
                        });
                    }
                    else
                    {
                        pivotedLineProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                        {
                            Index = index,
                            Display = Display,
                            SelectPrefix = selectPrefixForSmartEntries,
                            GetTerminalEntityForRead = GetEntityForRead,
                            GetTerminalEntityForSave = GetOrCreateEntityForSave
                        });
                    }
                }
            }

            return new MappingInfo(lineMetaForSave, lineMeta, pivotedLineProps, new List<MappingInfo>(), null, null)
            {
                CreateEntity = () =>
                {
                    var line = new LineForSave
                    {
                        DefinitionId = lineDefId,
                        Boolean1 = false,
                        Entries = new List<EntryForSave>(),
                    };

                    foreach (var entry in lineDef.Entries)
                    {
                        line.Entries.Add(new Entry
                        {
                            Id = 0,
                            Direction = entry.Direction,
                            Value = 0,

                            EntityMetadata = new EntityMetadata { BaseEntity = line }
                        });
                    }

                    return line;
                },
                GetEntitiesForRead = (Entity entity) =>
                {
                    var doc = entity as Document;
                    return doc.Lines.Where(e => e.DefinitionId == lineDefId);
                },
                // Display = TabDisplay
            };
        }

        private async Task<MappingInfo> MappingFromHeadersForLines(string[] headers, int lineDefId, ImportErrors errors, CancellationToken cancellation)
        {
            // Create the trie of labels
            var trie = new LabelPathTrie();
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                if (string.IsNullOrWhiteSpace(header))
                {
                    if (!errors.AddImportError(1, i + 1, _localizer["Error_EmptyHeadersNotAllowed"]))
                    {
                        return null;
                    }
                }

                var (steps, key) = SplitHeader(header);
                trie.AddPath(steps, key, index: i);
            }

            // Get the metadatas
            IMetadataOverridesProvider overrides = await FactBehavior.GetMetadataOverridesProvider(cancellation);
            var lineMetaForSave = _metadata.GetMetadata(TenantId, typeof(LineForSave), null, overrides);
            var lineMeta = _metadata.GetMetadata(TenantId, typeof(Line), null, overrides);

            // Create the mapping recurisvely using the trie
            var defaultMapping = await GetDefaultMappingForLines(lineMetaForSave, lineMeta, lineDefId, cancellation);
            var result = trie.CreateMapping(defaultMapping, errors, _localizer);
            return result;
        }

        #endregion
    }

    public class DocumentsGenericService : FactWithIdServiceBase<Document, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IPermissionsCache _permissionsCache;

        public DocumentsGenericService(
            ApplicationFactServiceBehavior behavior,
            FactServiceDependencies deps,
            IPermissionsCache permissionsCache) : base(deps)
        {
            _behavior = behavior;
            _permissionsCache = permissionsCache;
        }

        protected override string View => throw new NotImplementedException(); // We override user permissions

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to documents
            string prefix = "documents/";
            var permissions = (await _permissionsCache
                .GenericPermissionsFromCache(
                    tenantId: _behavior.TenantId,
                    userId: UserId,
                    version: _behavior.PermissionsVersion,
                    viewPrefix: prefix,
                    action: action,
                    cancellation: cancellation)).ToList();

            // Massage the permissions by adding definitionId = {definitionId} as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionIdString = permission.View.Remove(0, prefix.Length);
                if (!int.TryParse(definitionIdString, out int definitionId))
                {
                    throw new ServiceException($"Could not parse definition Id '{definitionIdString}' to a valid integer.");
                }

                string definitionPredicate = $"{nameof(Document.DefinitionId)} eq {definitionId}";
                if (!string.IsNullOrWhiteSpace(permission.Criteria))
                {
                    permission.Criteria = $"{definitionPredicate} and ({permission.Criteria})";
                }
                else
                {
                    permission.Criteria = definitionPredicate;
                }
            }

            // Add a special permission that lets you see the documents that were assigned to you
            permissions.AddRange(DocumentServiceUtil.HardCodedPermissions(action));

            // Return the massaged permissions
            return permissions;
        }

        protected override async Task<EntityQuery<Document>> Search(EntityQuery<Document> query, GetArguments args, CancellationToken cancellation)
        {
            // Get a map from all serial prefixes to definitionIds
            var defs = await _behavior.Definitions(cancellation);
            var prefixMap = defs.Documents.Select(e => (e.Value.Prefix, e.Key)); // Select all (Serial Prefix, DefinitionId)

            return DocumentServiceUtil.SearchImpl(query, args, prefixMap, false, false);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse($"{nameof(Document.PostingDate)} desc");
            return Task.FromResult(result);
        }
    }

    internal class DocumentServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific services, so we move it out here.
        /// </summary>
        internal static EntityQuery<Document> SearchImpl(EntityQuery<Document> query, GetArguments args, IEnumerable<(string Prefix, int DefinitionId)> prefixMap, bool includeInternalRef, bool includeExternalRef)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                // IF: the search starts with the serial prefix (case insensitive) then search the serial numbers exclusively 
                var searchLower = search.Trim().ToLower();
                var (prefix, definitionId) = prefixMap.FirstOrDefault(e =>
                    !string.IsNullOrWhiteSpace(e.Prefix) &&
                    searchLower.StartsWith(e.Prefix.ToLower()) &&
                    searchLower.Length > e.Prefix.Length);

                if (definitionId != 0 && int.TryParse(searchLower.Remove(0, prefix.Length), out int serial))
                {
                    var serialNumberProp = nameof(Document.SerialNumber);
                    var definitionIdProp = nameof(Document.DefinitionId);

                    // Prepare the filter string
                    var filterString = $"{serialNumberProp} eq {serial} and {definitionIdProp} eq {definitionId}";

                    // Apply the filter
                    query = query.Filter(filterString);
                }

                // ELSE: search the memo, posting date, etc normally
                else
                {
                    search = search.Replace("'", "''"); // escape quotes by repeating them


                    // Prepare the filter string
                    var filterString = $"{nameof(Document.Memo)} contains '{search}'";

                    // If the search is a number, include documents with that serial number
                    if (int.TryParse(search.Trim(), out int searchNumber))
                    {
                        filterString = $"{filterString} or {nameof(Document.SerialNumber)} eq {searchNumber}";
                    }

                    // If the search is a date, include documents with that date
                    if (DateTime.TryParse(search.Trim(), out DateTime searchDate))
                    {
                        filterString = $"{filterString} or {nameof(Document.PostingDate)} eq '{searchDate:yyyy-MM-dd}'";
                    }

                    if (includeInternalRef)
                    {
                        filterString = $"{filterString} or {nameof(Document.InternalReference)} contains '{search}'";
                    }

                    if (includeExternalRef)
                    {
                        filterString = $"{filterString} or {nameof(Document.ExternalReference)} contains '{search}'";
                    }

                    // Apply the filter
                    query = query.Filter(ExpressionFilter.Parse(filterString));
                }
            }

            return query;
        }

        /// <summary>
        /// The permission to read a document that is in your inbox is universal. 
        /// Otherwise you would think your inbox is empty when it is not.
        /// </summary>
        internal static IEnumerable<AbstractPermission> HardCodedPermissions(string action)
        {
            if (action == PermissionActions.Read)
            {
                // If someone assigns the document to you, you can read it
                // and forward it to someone else, until it either gets
                // forwarded again (the second condition so that you can 
                // refresh the document immediately after forwarding)
                yield return new AbstractPermission
                {
                    View = "documents", // Not important
                    Action = PermissionActions.Read,
                    Criteria = "AssigneeId eq me OR AssignedById eq me"
                };
            }
        }
    }
}
