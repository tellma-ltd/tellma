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

        private string View => LineDefinitionsController.BASE_ADDRESS;

        public LineDefinitionsService(ApplicationRepository repo, IDefinitionsCache defCache, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
            _defCache = defCache;
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

                var filterString = $"{titleS} {Ops.contains} '{search}' or {titleS2} {Ops.contains} '{search}' or {titleS3} {Ops.contains} '{search}' or {titleP} {Ops.contains} '{search}' or {titleP2} {Ops.contains} '{search}' or {titleP3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}' or {desc} {Ops.contains} '{search}' or {desc2} {Ops.contains} '{search}' or {desc3} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override Task<List<LineDefinitionForSave>> SavePreprocessAsync(List<LineDefinitionForSave> entities)
        {
            entities.ForEach(lineDefinition =>
            {
                lineDefinition?.Columns?.ForEach(column =>
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
                        case "AdditionalReference":
                            break;
                        default:
                            column.InheritsFromHeader = 0; // Other columns cannot inherit from header
                            break;
                    }
                });

                lineDefinition?.Workflows?.ForEach(workflow =>
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
            // C# validation
            int lineDefinitionIndex = 0;
            entities.ForEach(lineDefinition =>
            {
                int columnIndex = 0;
                lineDefinition.Columns?.ForEach(column =>
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

                int workflowIndex = 0;
                lineDefinition.Workflows?.ForEach(workflow =>
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
