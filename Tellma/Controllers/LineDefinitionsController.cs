using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Tellma.Services.Utilities;
using System.Linq;
using Tellma.Entities.Descriptors;
using System.Reflection;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class LineDefinitionsController : CrudControllerBase<LineDefinitionForSave, LineDefinition, int>
    {
        public const string BASE_ADDRESS = "line-definitions";

        private readonly LineDefinitionsService _service;

        public LineDefinitionsController(LineDefinitionsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override CrudServiceBase<LineDefinitionForSave, LineDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<LineDefinition> data, Extras extras)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulSave(data, extras);
        }

        protected override Task OnSuccessfulDelete(List<int> ids)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulDelete(ids);
        }
    }

    public class LineDefinitionsService : CrudServiceBase<LineDefinitionForSave, LineDefinition, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _defCache;
        private readonly ISettingsCache _settingsCache;

        private string View => LineDefinitionsController.BASE_ADDRESS;

        public LineDefinitionsService(ApplicationRepository repo, IDefinitionsCache defCache, ISettingsCache settingsCache, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
            _defCache = defCache;
            _settingsCache = settingsCache;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<LineDefinition> Search(Query<LineDefinition> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(LookupDefinition.TitlePlural);
                var titleP2 = nameof(LookupDefinition.TitlePlural2);
                var titleP3 = nameof(LookupDefinition.TitlePlural3);
                var titleS = nameof(LookupDefinition.TitleSingular);
                var titleS2 = nameof(LookupDefinition.TitleSingular2);
                var titleS3 = nameof(LookupDefinition.TitleSingular3);
                var code = nameof(LineDefinition.Code);
                var desc = nameof(LineDefinition.Description);
                var desc2 = nameof(LineDefinition.Description2);
                var desc3 = nameof(LineDefinition.Description3);

                var filterString = $"{titleS} contains '{search}' or {titleS2} contains '{search}' or {titleS3} contains '{search}' or {titleP} contains '{search}' or {titleP2} contains '{search}' or {titleP3} contains '{search}' or {code} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return query;
        }

        protected override Task<List<LineDefinitionForSave>> SavePreprocessAsync(List<LineDefinitionForSave> entities)
        {
            var settings = _settingsCache.GetCurrentSettingsIfCached().Data;

            entities.ForEach(lineDefinition =>
            {
                lineDefinition.AllowSelectiveSigning ??= false;
                lineDefinition.ViewDefaultsToForm ??= false;
                lineDefinition.BarcodeBeepsEnabled ??= false;

                lineDefinition.Columns ??= new List<LineDefinitionColumnForSave>();
                lineDefinition.Entries ??= new List<LineDefinitionEntryForSave>();
                lineDefinition.GenerateParameters ??= new List<LineDefinitionGenerateParameterForSave>();
                lineDefinition.StateReasons ??= new List<LineDefinitionStateReasonForSave>();
                lineDefinition.Workflows ??= new List<WorkflowForSave>();

                lineDefinition?.Columns.ForEach(column =>
                {
                    // Those two are required in the sql table, so they cannot be null
                    if (column.ColumnName == nameof(Entry.CenterId))
                    {
                        column.VisibleState = LineState.Draft;
                        column.RequiredState = LineState.Draft;
                    }

                    if (column.ColumnName == nameof(Entry.CurrencyId))
                    {
                        column.VisibleState = LineState.Draft;
                        column.RequiredState = LineState.Draft;
                    }

                    // IMPORTANT: Keep in sync with line-definitions-details.component.ts
                    switch (column.ColumnName)
                    {
                        case "PostingDate":
                        case "Memo":
                        case "CurrencyId":
                        case "CenterId":
                        case "CustodianId":
                        case "CustodyId":
                        case "ParticipantId":
                        case "ResourceId":
                        case "Quantity":
                        case "UnitId":
                        case "Time1":
                        case "Time2":
                        case "ExternalReference":
                        case "InternalReference":
                            break;
                        default:
                            column.InheritsFromHeader = 0; // Only listed columns can inherit
                            break;
                    }

                    if (column.ColumnName == null || !column.ColumnName.EndsWith("Id"))
                    {
                        column.Filter = null; // Only listed columns can inherit
                    }
                });

                // Generate Parameters
                lineDefinition.GenerateParameters.ForEach(parameter =>
                {
                    parameter.ControlOptions = ControllerUtilities.PreprocessControlOptions(parameter.Control, parameter.ControlOptions, settings);
                });

                // Workflows
                lineDefinition?.Workflows.ForEach(workflow =>
                {
                    workflow?.Signatures?.ForEach(signature =>
                    {
                        if (signature != null)
                        {
                            if (signature.RuleType != RuleTypes.ByUser)
                            {
                                signature.UserId = null;
                            }

                            if (signature.RuleType != RuleTypes.ByRole)
                            {
                                signature.RoleId = null;
                            }

                            if (signature.RuleType != RuleTypes.ByCustodian)
                            {
                                signature.RuleTypeEntryIndex = null;
                            }

                            if (signature.RuleType == RuleTypes.Public)
                            {
                                signature.ProxyRoleId = null;
                            }

                            if (signature.PredicateType == null)
                            {
                                signature.PredicateTypeEntryIndex = null;
                                signature.Value = null;
                            }
                        }
                    });
                });
            });

            return Task.FromResult(entities);
        }

        protected override async Task SaveValidateAsync(List<LineDefinitionForSave> entities)
        {
            var defs = _defCache.GetCurrentDefinitionsIfCached().Data;
            var settings = _settingsCache.GetCurrentSettingsIfCached().Data;

            // C# validation
            int lineDefinitionIndex = 0;
            entities.ForEach(lineDefinition =>
            {
                // Columns
                int columnIndex = 0;
                lineDefinition.Columns.ForEach(column =>
                {
                    int index = column.EntryIndex.Value;
                    if (index < 0)
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Columns)}[{columnIndex}].{nameof(LineDefinitionColumn.EntryIndex)}";
                        string msg = _localizer["Error_IndexMustBeGreaterOrEqualZero"];

                        ModelState.AddModelError(path, msg);
                    }
                    else if (index > (lineDefinition.Entries?.Count ?? 0))
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Columns)}[{columnIndex}].{nameof(LineDefinitionColumn.EntryIndex)}";
                        string msg = _localizer["Error_NoEntryCorrespondsToIndex0", index];

                        ModelState.AddModelError(path, msg);
                    }

                    // Required state should always be <= ReadOnlyState
                    if (column.RequiredState > column.ReadOnlyState)
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Columns)}[{columnIndex}].{nameof(LineDefinitionColumn.ReadOnlyState)}";
                        string msg = _localizer["Error_ReadOnlyStateCannotBeBeforeRequiredState"]; ;

                        ModelState.AddModelError(path, msg);
                    }

                    columnIndex++;
                });

                // GenerateScript
                if (!string.IsNullOrWhiteSpace(lineDefinition.GenerateScript))
                {
                    // If auto-generate script is specified, DefaultsToForm must be false
                    if (lineDefinition.ViewDefaultsToForm.Value)
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.GenerateScript)}";
                        string msg = _localizer["Error_CannotHaveGenerateScriptWithDefaultsToForm"];

                        ModelState.AddModelError(path, msg);
                    }
                }

                // Generate parameters
                int paramIndex = 0;
                lineDefinition.GenerateParameters.ForEach(parameter =>
                {
                    var errors = ControllerUtilities.ValidateControlOptions(parameter.Control, parameter.ControlOptions, _localizer, settings, defs);
                    foreach (var msg in errors)
                    {
                        ModelState.AddModelError($"[{lineDefinitionIndex}].{nameof(lineDefinition.GenerateParameters)}[{paramIndex}].{nameof(parameter.ControlOptions)}", msg);
                    }

                    paramIndex++;
                });

                // Workflows
                int workflowIndex = 0;
                lineDefinition.Workflows.ForEach(workflow =>
                {
                    int signatureIndex = 0;
                    workflow.Signatures?.ForEach(signature =>
                    {
                        // Role is required
                        if (signature.RuleType == RuleTypes.ByRole && signature.RoleId == null)
                        {
                            string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.RoleId)}";
                            string msg = _localizer[Constants.Error_Field0IsRequired, _localizer["WorkflowSignature_Role"]];

                            ModelState.AddModelError(path, msg);
                        }

                        // User is required
                        if (signature.RuleType == RuleTypes.ByUser && signature.UserId == null)
                        {
                            string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.UserId)}";
                            string msg = _localizer[Constants.Error_Field0IsRequired, _localizer["WorkflowSignature_User"]];

                            ModelState.AddModelError(path, msg);
                        }

                        if (signature.RuleType == RuleTypes.ByCustodian && signature.RuleTypeEntryIndex == null)
                        {
                            // Entry index is required
                            if (signature.RuleTypeEntryIndex == null)
                            {
                                string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.RuleTypeEntryIndex)}";
                                string msg = _localizer[Constants.Error_Field0IsRequired, _localizer["WorkflowSignature_RuleTypeEntryIndex"]];

                                ModelState.AddModelError(path, msg);
                            }
                            else
                            {
                                // Make sure Entry index is not out of bounds
                                int index = signature.RuleTypeEntryIndex.Value;
                                if (index < 0)
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.RuleTypeEntryIndex)}";
                                    string msg = _localizer["Error_IndexMustBeGreaterOrEqualZero"];

                                    ModelState.AddModelError(path, msg);
                                }
                                else if (index > (lineDefinition.Entries?.Count ?? 0))
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.RuleTypeEntryIndex)}";
                                    string msg = _localizer["Error_NoEntryCorrespondsToIndex0", index];

                                    ModelState.AddModelError(path, msg);
                                }
                            }
                        }

                        if (signature.PredicateType == PredicateTypes.ValueGreaterOrEqual)
                        {
                            // Value is required
                            if (signature.Value == null)
                            {
                                string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.Value)}";
                                string msg = _localizer[Constants.Error_Field0IsRequired, _localizer["WorkflowSignature_Value"]];

                                ModelState.AddModelError(path, msg);
                            }

                            // Entry Index is required
                            if (signature.PredicateTypeEntryIndex == null)
                            {
                                string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.PredicateTypeEntryIndex)}";
                                string msg = _localizer[Constants.Error_Field0IsRequired, _localizer["WorkflowSignature_PredicateTypeEntryIndex"]];

                                ModelState.AddModelError(path, msg);
                            }
                            else
                            {
                                // Make sure Entry index is not out of bounds
                                int index = signature.PredicateTypeEntryIndex.Value;
                                if (index < 0)
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.PredicateTypeEntryIndex)}";
                                    string msg = _localizer["Error_IndexMustBeGreaterOrEqualZero"];

                                    ModelState.AddModelError(path, msg);
                                }
                                else if (index > (lineDefinition.Entries?.Count ?? 0))
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.PredicateTypeEntryIndex)}";
                                    string msg = _localizer["Error_NoEntryCorrespondsToIndex0", index];

                                    ModelState.AddModelError(path, msg);
                                }
                            }
                        }

                        signatureIndex++;
                    });

                    workflowIndex++;
                });

                // Barcode
                if (lineDefinition.BarcodeColumnIndex != null)
                {
                    // If barcode is enabled, BarcodeProperty must be specified
                    if (string.IsNullOrWhiteSpace(lineDefinition.BarcodeProperty))
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeProperty)}";
                        string msg = _localizer[Constants.Error_Field0IsRequired, _localizer["LineDefinition_BarcodeProperty"]];

                        ModelState.AddModelError(path, msg);
                    }

                    // If barcode is enabled, BarcodeExistingItemHandling must be specified
                    if (string.IsNullOrWhiteSpace(lineDefinition.BarcodeExistingItemHandling))
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeExistingItemHandling)}";
                        string msg = _localizer[Constants.Error_Field0IsRequired, _localizer["LineDefinition_BarcodeExistingItemHandling"]];

                        ModelState.AddModelError(path, msg);
                    }

                    // If barcode is enabled, DefaultsToForm must be false
                    if (lineDefinition.ViewDefaultsToForm.Value)
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeColumnIndex)}";
                        string msg = _localizer["Error_CannotHaveBarcodeWithDefaultsToForm"];

                        ModelState.AddModelError(path, msg);
                    }

                    // BarcodeColumnIndex must be within Columns range
                    var colIndex = lineDefinition.BarcodeColumnIndex.Value;
                    if (colIndex >= lineDefinition.Columns.Count)
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeColumnIndex)}";
                        string msg = _localizer["Error_BarcodeColumnIndexOutOfRange"];

                        ModelState.AddModelError(path, msg);
                    }
                    else
                    {
                        // Barcode Column cannot inherit from headers
                        var colDef = lineDefinition.Columns[colIndex];
                        if (colDef.InheritsFromHeader > 0)
                        {
                            string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeColumnIndex)}";
                            string msg = _localizer["Error_BarcodeColumnCannotInheritFromHeaders"];

                            ModelState.AddModelError(path, msg);
                        }

                        // Barcode Column must be visible from DRAFT
                        if (colDef.VisibleState > 0)
                        {
                            string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeColumnIndex)}";
                            string msg = _localizer["Error_BarcodeColumnMustBeVisibleFromDraft"];

                            ModelState.AddModelError(path, msg);
                        }

                        // Barcode Column must be editable from DRAFT
                        if (colDef.ReadOnlyState == 0)
                        {
                            string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeColumnIndex)}";
                            string msg = _localizer["Error_BarcodeColumnCannotBeReadOnlyFromDraft"];

                            ModelState.AddModelError(path, msg);
                        }

                        Dictionary<string, Type> acceptableColumnNames = new Dictionary<string, Type> {
                            { "CustodianId", typeof(Relation) },
                            { "CustodyId", typeof(Custody) },
                            { "ParticipantId", typeof(Relation) },
                            { "ResourceId", typeof(Resource) }
                        };

                        if (string.IsNullOrWhiteSpace(colDef.ColumnName))
                        {
                            // Error handled earlier
                        }
                        else if (!acceptableColumnNames.TryGetValue(colDef.ColumnName, out Type colType))
                        {
                            // Barcode Column must have on of the supported column names
                            string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeColumnIndex)}";
                            string names = string.Join(", ", acceptableColumnNames.Keys.Select(e => _localizer["Entry_" + e[0..^2]]));
                            string msg = _localizer["Error_BarcodeColumnWrongColumnNameAcceptableAre0", names];

                            ModelState.AddModelError(path, msg);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(lineDefinition.BarcodeProperty))
                            {
                                // Barcode Property must be a valid property on the column type
                                var propDesc = TypeDescriptor.Get(colType).Property(lineDefinition.BarcodeProperty);
                                if (propDesc == null)
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeProperty)}";
                                    string msg = _localizer["Error_BarcodeProperty0IsNotAValidFieldOnType1", lineDefinition.BarcodeProperty, colType];

                                    ModelState.AddModelError(path, msg);
                                }
                                // Barcode Property must be string or int
                                else if ((propDesc.Type != typeof(string) && propDesc.Type != typeof(int) && propDesc.Type != typeof(int?))
                                    || propDesc.PropertyInfo.GetCustomAttribute<ChoiceListAttribute>(inherit: true) != null)
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeProperty)}";
                                    string msg = _localizer["Error_BarcodePropertyShouldBeOfTypeStringOrInt"];

                                    ModelState.AddModelError(path, msg);
                                }
                            }
                        }

                        if (lineDefinition.BarcodeExistingItemHandling == "IncrementQuantity")
                        {
                            // If handling is Increment Quantity, then a quantity column with the same entry index must be visible, and editable
                            var quantityColumn = lineDefinition.Columns.FirstOrDefault(col => col.ColumnName == "Quantity" && col.EntryIndex == colDef.EntryIndex);
                            if (quantityColumn == null)
                            {
                                string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeExistingItemHandling)}";
                                string msg = _localizer["Error_QuantityColumnWithSameEntryIndexMustBeAdded"];

                                ModelState.AddModelError(path, msg);
                            }
                            else
                            {
                                if (quantityColumn.InheritsFromHeader > 0)
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeExistingItemHandling)}";
                                    string msg = _localizer["Error_QuantityColumnCannotInheritFromHeaders"];

                                    ModelState.AddModelError(path, msg);
                                }

                                if (quantityColumn.VisibleState > 0)
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeExistingItemHandling)}";
                                    string msg = _localizer["Error_QuantityColumnMustBeVisibleFromDraft"];

                                    ModelState.AddModelError(path, msg);
                                }

                                if (quantityColumn.ReadOnlyState == 0)
                                {
                                    string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeExistingItemHandling)}";
                                    string msg = _localizer["Error_QuantityColumnCannotBeReadOnlyFromDraft"];

                                    ModelState.AddModelError(path, msg);
                                }
                            }
                        }
                    }
                }

                lineDefinitionIndex++;
            });

            // No point keeping on
            ModelState.ThrowIfInvalid();

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.LineDefinitions_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<LineDefinitionForSave> entities, bool returnIds)
        {
            return await _repo.LineDefinitions__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // Make sure 
            int jvDefId = _defCache.GetCurrentDefinitionsIfCached()?.Data?.ManualLinesDefinitionId ??
                throw new BadRequestException("The Manual Line Id is not defined");

            int index = 0;
            ids.ForEach(id =>
            {
                if (id == jvDefId)
                {
                    string path = $"[{index}]";
                    string msg = _localizer["Error_CannotModifySystemItem"];

                    ModelState.AddModelError(path, msg);
                }

                index++;
            });

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.LineDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.LineDefinitions__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["LineDefinition"]]);
            }
        }
    }
}
