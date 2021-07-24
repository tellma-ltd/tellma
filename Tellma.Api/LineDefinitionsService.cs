using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class LineDefinitionsService : CrudServiceBase<LineDefinitionForSave, LineDefinition, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;

        public LineDefinitionsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        protected override string View => "line-definitions";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<LineDefinition>> Search(EntityQuery<LineDefinition> query, GetArguments args, CancellationToken _)
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

            return Task.FromResult(query);
        }

        protected override async Task<List<LineDefinitionForSave>> SavePreprocessAsync(List<LineDefinitionForSave> entities)
        {
            var settings = await _behavior.Settings();

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
                        case nameof(Line.PostingDate):
                        case nameof(Line.Memo):
                        case nameof(Entry.CurrencyId):
                        case nameof(Entry.CenterId):
                        case nameof(Entry.CustodianId):
                        case nameof(Entry.RelationId):
                        case nameof(Entry.ResourceId):
                        case nameof(Entry.NotedRelationId):
                        case nameof(Entry.Quantity):
                        case nameof(Entry.UnitId):
                        case nameof(Entry.Time1):
                        case nameof(Entry.Duration):
                        case nameof(Entry.DurationUnitId):
                        case nameof(Entry.Time2):
                        case nameof(Entry.ExternalReference):
                        case nameof(Entry.ReferenceSourceId):
                        case nameof(Entry.InternalReference):
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
                    parameter.ControlOptions = ApplicationUtil.PreprocessControlOptions(parameter.Control, parameter.ControlOptions, settings);
                });

                // Workflows
                lineDefinition?.Workflows.ForEach(workflow =>
                {
                    workflow?.Signatures?.ForEach(signature =>
                    {
                        if (signature != null)
                        {
                            signature.RuleType ??= RuleTypes.ByRole; // Default

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

            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<LineDefinitionForSave> entities, bool returnIds)
        {
            var defs = await _behavior.Definitions();
            var settings = await _behavior.Settings();

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
                    else if (index >= (lineDefinition.Entries?.Count ?? 0))
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Columns)}[{columnIndex}].{nameof(LineDefinitionColumn.EntryIndex)}";
                        string msg = _localizer["Error_NoEntryCorrespondsToIndex0", index];

                        ModelState.AddModelError(path, msg);
                    }

                    // Required state should always be <= ReadOnlyState
                    if (column.VisibleState > column.RequiredState)
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Columns)}[{columnIndex}].{nameof(LineDefinitionColumn.RequiredState)}";
                        string msg = _localizer["Error_RequiredStateCannotBeBeforeVisibleState"]; ;

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
                    var errors = ApplicationUtil.ValidateControlOptions(parameter.Control, parameter.ControlOptions, _localizer, settings, defs);
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
                            string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["WorkflowSignature_Role"]];

                            ModelState.AddModelError(path, msg);
                        }

                        // User is required
                        if (signature.RuleType == RuleTypes.ByUser && signature.UserId == null)
                        {
                            string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.UserId)}";
                            string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["WorkflowSignature_User"]];

                            ModelState.AddModelError(path, msg);
                        }

                        if (signature.RuleType == RuleTypes.ByCustodian && signature.RuleTypeEntryIndex == null)
                        {
                            // Entry index is required
                            if (signature.RuleTypeEntryIndex == null)
                            {
                                string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.RuleTypeEntryIndex)}";
                                string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["WorkflowSignature_RuleTypeEntryIndex"]];

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
                                else if (index >= (lineDefinition.Entries?.Count ?? 0))
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
                                string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["WorkflowSignature_Value"]];

                                ModelState.AddModelError(path, msg);
                            }

                            // Entry Index is required
                            if (signature.PredicateTypeEntryIndex == null)
                            {
                                string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.Workflows)}[{workflowIndex}].{nameof(Workflow.Signatures)}[{signatureIndex}].{nameof(WorkflowSignature.PredicateTypeEntryIndex)}";
                                string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["WorkflowSignature_PredicateTypeEntryIndex"]];

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
                                else if (index >= (lineDefinition.Entries?.Count ?? 0))
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
                        string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["LineDefinition_BarcodeProperty"]];

                        ModelState.AddModelError(path, msg);
                    }

                    // If barcode is enabled, BarcodeExistingItemHandling must be specified
                    if (string.IsNullOrWhiteSpace(lineDefinition.BarcodeExistingItemHandling))
                    {
                        string path = $"[{lineDefinitionIndex}].{nameof(LineDefinition.BarcodeExistingItemHandling)}";
                        string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["LineDefinition_BarcodeExistingItemHandling"]];

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

                        var acceptableColumnNames = new Dictionary<string, Type> {
                            { nameof(Entry.CustodianId), typeof(Relation) },
                            { nameof(Entry.RelationId), typeof(Relation) },
                            { nameof(Entry.NotedRelationId), typeof(Relation) },
                            { nameof(Entry.ResourceId), typeof(Resource) },
                        };

                        if (string.IsNullOrWhiteSpace(colDef.ColumnName))
                        {
                            // Error handled earlier
                        }
                        else if (!acceptableColumnNames.TryGetValue(colDef.ColumnName, out Type colType))
                        {
                            // Barcode Column must have one of the supported column names
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
            if (!ModelState.IsValid)
            {
                return null;
            }

            SaveResult result = await _behavior.Repository.LineDefinitions__Save(entities, returnIds: returnIds, UserId);
            AddLocalizedErrors(result.Errors);

            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            // Make sure 
            var defs = await _behavior.Definitions();
            int manualLineDefId = defs.ManualLinesDefinitionId;

            int index = 0;
            ids.ForEach(id =>
            {
                if (id == manualLineDefId)
                {
                    string path = $"[{index}]";
                    string msg = _localizer["Error_CannotModifySystemItem"];

                    ModelState.AddModelError(path, msg);
                }

                index++;
            });

            if (!ModelState.IsValid)
            {
                return;
            }

            try
            {
                DeleteResult result = await _behavior.Repository.LineDefinitions__Delete(ids, userId: UserId);
                AddLocalizedErrors(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }
    }
}
