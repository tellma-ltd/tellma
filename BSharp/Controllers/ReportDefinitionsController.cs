using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationApi]
    public class ReportDefinitionsController : CrudControllerBase<ReportDefinitionForSave, ReportDefinition, string>
    {
        public const string BASE_ADDRESS = "report-definitions";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string View => BASE_ADDRESS;

        public ReportDefinitionsController(
            ILogger<ReportDefinitionsController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        [HttpPut("update-state")]
        public async Task<ActionResult<EntitiesResponse<ReportDefinition>>> UpdateState([FromBody] List<string> ids, [FromQuery] UpdateStateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                UpdateState(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    state: args.State)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<ReportDefinition>>> UpdateState([FromBody] List<string> ids, bool returnEntities, string expand, string state)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("UpdateState", idsArray);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            // TODO: UPDATE state in DB
            if (returnEntities)
            {
                var response = await GetByIdListAsync(idsArray, expandExp);

                trx.Complete();
                return Ok(response);
            }
            else
            {
                trx.Complete();
                return Ok();
            }
        }

        protected override async Task<GetResponse<ReportDefinition>> GetImplAsync(GetArguments args, Query<ReportDefinition> queryOverride)
        {
            // Prepare the query
            IEnumerable<ReportDefinition> query = _db.Values;

            // Before ordering or paging, retrieve the total count
            int totalCount = query.Count();

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Take(top);

            // Load the data in memory
            var result = query.ToList();

            // Prepare the result in a response object
            return new GetResponse<ReportDefinition>
            {
                Skip = skip,
                Top = result.Count(),
                OrderBy = args.OrderBy,
                TotalCount = totalCount,

                Result = result,
                RelatedEntities = new Dictionary<string, IEnumerable<Entity>>(),
                CollectionName = GetCollectionName(typeof(ReportDefinition))
            };
        }

        protected override async Task<GetByIdResponse<ReportDefinition>> GetByIdImplAsync(string id, [FromQuery] GetByIdArguments args)
        {
            _db.TryGetValue(id, out ReportDefinition result);
            if (result == null)
            {
                throw new NotFoundException<string>(id);
            }

            // Return
            return new GetByIdResponse<ReportDefinition>
            {
                Result = result,
                CollectionName = GetCollectionName(typeof(ReportDefinition)),
                RelatedEntities = new Dictionary<string, IEnumerable<Entity>>()
            };
        }

        protected override async Task<EntitiesResponse<ReportDefinition>> SaveImplAsync(List<ReportDefinitionForSave> entities, SaveArguments args)
        {
            var result = new List<ReportDefinition>(entities.Count);
            foreach (var entityForSave in entities)
            {
                if (string.IsNullOrWhiteSpace(entityForSave.Id))
                {
                    entityForSave.Id = Guid.NewGuid().ToString("D");
                }
                var entity = new ReportDefinition();

                foreach (var prop in typeof(ReportDefinition).GetProperties().Where(e => !e.PropertyType.IsList()))
                {
                    var propForSave = typeof(ReportDefinitionForSave).GetProperty(prop.Name);
                    if (propForSave != null)
                    {
                        prop.SetValue(entity, propForSave.GetValue(entityForSave));
                        entity.EntityMetadata[prop.Name] = FieldMetadata.Loaded;
                    }
                }

                if (entityForSave.Parameters != null)
                {
                    entity.Parameters = new List<ReportParameterDefinition>();
                    foreach (var e in entityForSave.Parameters)
                    {
                        entity.Parameters.Add(new ReportParameterDefinition
                        {
                            Id = e.Id == 0 ? _id++ : e.Id,
                            Key = e.Key,
                            Label = e.Label,
                            Label2 = e.Label2,
                            Label3 = e.Label3,
                            //Control = e.Control,
                            //Collection = e.Collection,
                            //DefinitionId = e.DefinitionId,
                            //Filter = e.Filter,
                            //MinDecimalPlaces = e.MinDecimalPlaces,
                            //MaxDecimalPlaces = e.MaxDecimalPlaces,
                            Visibility = e.Visibility,
                            ReportDefinitionId = e.ReportDefinitionId,
                            Value = e.Value
                        });
                    }
                }

                if (entityForSave.Rows != null)
                {
                    entity.Rows = new List<ReportRowDefinition>();
                    foreach (var e in entityForSave.Rows)
                    {
                        entity.Rows.Add(new ReportRowDefinition
                        {
                            Id = e.Id == 0 ? _id++ : e.Id,
                            Label = e.Label,
                            Label2 = e.Label2,
                            Label3 = e.Label3,
                            ReportDefinitionId = e.ReportDefinitionId,
                            AutoExpand = e.AutoExpand,
                            Path = e.Path,
                            Modifier = e.Modifier,
                            OrderDirection = e.OrderDirection
                        });
                    }
                }

                if (entityForSave.Columns != null)
                {
                    entity.Columns = new List<ReportColumnDefinition>();
                    foreach (var e in entityForSave.Columns)
                    {
                        entity.Columns.Add(new ReportColumnDefinition
                        {
                            Id = e.Id == 0 ? _id++ : e.Id,
                            Label = e.Label,
                            Label2 = e.Label2,
                            Label3 = e.Label3,
                            ReportDefinitionId = e.ReportDefinitionId,
                            AutoExpand = e.AutoExpand,
                            Path = e.Path,
                            Modifier = e.Modifier,
                            OrderDirection = e.OrderDirection
                        });
                    }
                }

                if (entityForSave.Measures != null)
                {
                    entity.Measures = new List<ReportMeasureDefinition>();
                    foreach (var e in entityForSave.Measures)
                    {
                        entity.Measures.Add(new ReportMeasureDefinition
                        {
                            Id = e.Id == 0 ? _id++ : e.Id,
                            Label = e.Label,
                            Label2 = e.Label2,
                            Label3 = e.Label3,
                            ReportDefinitionId = e.ReportDefinitionId,
                            Path = e.Path,
                            OrderDirection = e.OrderDirection,
                            Aggregation = e.Aggregation
                        });
                    }
                }

                if (entityForSave.Select != null)
                {
                    entity.Select = new List<ReportSelectDefinition>();
                    foreach (var e in entityForSave.Select)
                    {
                        entity.Select.Add(new ReportSelectDefinition
                        {
                            Id = e.Id == 0 ? _id++ : e.Id,
                            Label = e.Label,
                            Label2 = e.Label2,
                            Label3 = e.Label3,
                            ReportDefinitionId = e.ReportDefinitionId,
                            Path = e.Path,
                        });
                    }
                }

                _db[entity.Id] = entity;
                result.Add(entity);
            }

            return new EntitiesResponse<ReportDefinition>
            {
                Result = result,
                CollectionName = GetCollectionName(typeof(ReportDefinition)),
                RelatedEntities = new Dictionary<string, IEnumerable<Entity>>(),
                IsPartial = false
            };
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, View);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<ReportDefinition> Search(Query<ReportDefinition> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var title = nameof(ReportDefinition.Title);
                var title2 = nameof(ReportDefinition.Title2);
                var title3 = nameof(ReportDefinition.Title3);
                var desc = nameof(ReportDefinition.Description);
                var desc2 = nameof(ReportDefinition.Description2);
                var desc3 = nameof(ReportDefinition.Description3);

                var filterString = $"{title} {Ops.contains} '{search}' or {title2} {Ops.contains} '{search}' or {title3} {Ops.contains} '{search}' or {desc} {Ops.contains} '{search}' or {desc2} {Ops.contains} '{search}' or {desc3} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<ReportDefinitionForSave> entities)
        {
            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                // Ensure that Id is supplied
                if (string.IsNullOrEmpty(entity.Id))
                {
                    string path = $"[{index}].{nameof(entity.Id)}";
                    string msg = _localizer[nameof(RequiredAttribute), _localizer["Code"]];

                    ModelState.AddModelError(path, msg);
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                // null Ids will cause an error when calling the SQL validation
                return;
            }

            // TODO: Validation

            //// SQL validation
            //int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            //var sqlErrors = await _repo.ReportDefinitions_Validate__Save(entities, top: remainingErrorCount);

            //// Add errors to model state
            //ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<string>> SaveExecuteAsync(List<ReportDefinitionForSave> entities, ExpandExpression expand, bool returnIds)
        {
            // TODO: Save
            // await _repo.ReportDefinitions__Save(entities);
            return entities.Select(e => e.Id).ToList();
        }

        protected override async Task DeleteValidateAsync(List<string> ids)
        {
            // TODO: Validate Delete
            //// SQL validation
            //int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            //var sqlErrors = await _repo.ReportDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            //// Add errors to model state
            //ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<string> ids)
        {
            // TODO: Delete
            //try
            //{
            //    await _repo.ReportDefinitions__Delete(ids);
            //}
            //catch (ForeignKeyViolationException)
            //{
            //    throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ReportDefinition"]]);
            //}
        }

        protected override Query<ReportDefinition> GetAsQuery(List<ReportDefinitionForSave> entities)
        {
            // TODO: GetAsQuery
            throw new NotImplementedException(nameof(GetAsQuery));
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            // By default: Order report definitions by title
            var tenantInfo = _repo.GetTenantInfo();
            string nameProperty = nameof(ReportDefinition.Title);
            if (tenantInfo.SecondaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                nameProperty = $"{nameof(ReportDefinition.Title2)},{nameof(ReportDefinition.Title)}";
            }
            else if (tenantInfo.TernaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                nameProperty = $"{nameof(ReportDefinition.Title3)},{nameof(ReportDefinition.Title)}";
            }

            return OrderByExpression.Parse(nameProperty);
        }

        private static int _id = 1000;
        private static readonly Dictionary<string, ReportDefinition> _db = new Dictionary<string, ReportDefinition>
        {
            ["my-amazing-report"] = new ReportDefinition
            {
                Id = "my-amazing-report",
                Title = "My Amazing Report",
                Title2 = "تقريري المذهل",
                Title3 = "我的惊人报告",
                MainMenuIcon = "chart-pie",
                MainMenuSection = "Financials",
                MainMenuSortKey = 202m,
                State = "Draft",
                Type = ReportType.Summary,
                Chart = "Line",
                DefaultsToChart = false,
                Collection = "MeasurementUnit",
                Filter = "UnitType eq @UnitType and (Name contains @Name or Name2 contains @Name)",
                Parameters = new List<ReportParameterDefinition>
                {
                    new ReportParameterDefinition
                    {
                        Key = "Name", // "FromDate"
                        Label = "Name Contains",
                        Label2 = "الإسم يحتوي",
                        Label3 = "我的密",
                        Visibility = Visibility.Optional
                    },
                },
                Columns = new List<ReportColumnDefinition>
                {
                    //new ReportColumnDefinition
                    //{
                    //    Path = "ModifiedBy",
                    //    Label = "Modified By",
                    //    Label2 = "آخر تعديل",
                    //    Label3 = "我的密",
                    //    AutoExpand = true,
                    //},
                    //new ReportColumnDefinition
                    //{
                    //    Path = "UnitType",
                    //    Label = "Unit Type",
                    //    Label2 = "نوع الوحدة",
                    //    Label3 = "我的密",
                    //    OrderDirection = "desc",
                    //    AutoExpand =true
                    //},
                },
                Rows = new List<ReportRowDefinition>
                {
                    new ReportRowDefinition
                    {
                        Path = "CreatedBy",
                        Label = "Created By",
                        Label2 = "إنشاء من قبل",
                        Label3 = "我的密",
                        AutoExpand = true,
                    },
                    new ReportRowDefinition
                    {
                        Path = "UnitType",
                        Label = "Unit Type",
                        Label2 = "نوع الوحدة",
                        Label3 = "我的密",
                        OrderDirection = "desc",
                        AutoExpand = true
                    },
                },
                Measures = new List<ReportMeasureDefinition>
                {
                    new ReportMeasureDefinition
                    {
                        Path = "Id",
                        Aggregation = "count",
                        Label = "Count",
                        Label2 = "العدد",
                        Label3 = "我的密"
                    },
                    new ReportMeasureDefinition
                    {
                        Path = "Id",
                        Aggregation = "avg",
                        Label = "Average",
                        Label2 = "المعدل",
                        Label3 = "我的密"
                    },
                    //new ReportMeasureDefinition
                    //{
                    //    Path = "Name",
                    //    Aggregation = "max",
                    //    Label = "Max",
                    //    Label2 = "الأقصى",
                    //    Label3 = "我的密"
                    //},
                },
                ShowColumnsTotal = true,
                ShowRowsTotal = true,
                EntityMetadata = new EntityMetadata
                {
                    ["State"] = FieldMetadata.Loaded,
                    ["Type"] = FieldMetadata.Loaded,
                    ["Title"] = FieldMetadata.Loaded,
                    ["Description"] = FieldMetadata.Loaded,
                    ["Description2"] = FieldMetadata.Loaded,
                }
            },
            ["my-incredible-report"] = new ReportDefinition
            {
                Id = "my-incredible-report",
                Title = "My Incredible Report",
                Title2 = "تقريري المدهش",
                Title3 = "我的惊人报告",
                MainMenuIcon = "chart-pie",
                MainMenuSection = "Financials",
                MainMenuSortKey = 202m,
                State = "Deployed",
                Type = ReportType.Summary,
                Chart = "Card",
                DefaultsToChart = true,
                Collection = "Resource",
                DefinitionId = "finished-goods",
                // Filter = "Memo contains @Memo",
                Parameters = new List<ReportParameterDefinition>
                {
                    new ReportParameterDefinition
                    {
                        Key = "Memo", // "FromDate"
                        Label = "Memo Contains",
                        Label2 = "الملاحظات تحتوي",
                        Label3 = "我的密",
                        Visibility = Visibility.Optional
                    }
                },
                Columns = new List<ReportColumnDefinition>
                {

                },
                Rows = new List<ReportRowDefinition>
                {
                    //new ReportRowDefinition
                    //{
                    //    Path = "ModifiedBy",
                    //    Label = "Modified By",
                    //    Label2 = "آخر تعديل",
                    //    Label3 = "我的密",
                    //    AutoExpand =false,
                    //},
                    //new ReportRowDefinition
                    //{
                    //    Path = "ResourceLookup1",
                    //    //Label = "Modified By State",
                    //    //Label2 = "آخر تعديل",
                    //    //Label3 = "我的密",
                    //    OrderDirection = "desc",
                    //    AutoExpand = true,
                    //},
                    //new ReportRowDefinition
                    //{
                    //    Path = "ResourceClassification",
                    //    //Label = "Unit Type",
                    //    //Label2 = "نوع الوحدة",
                    //    //Label3 = "我的密",
                    //   //  OrderDirection = "desc",
                    //    AutoExpand =true
                    //},
                },
                Measures = new List<ReportMeasureDefinition>
                {
                    new ReportMeasureDefinition
                    {
                        Path = "Id",
                        Aggregation = "count",
                        Label = "Count",
                        Label2 = "العدد",
                        Label3 = "我的密"
                    },
                    //new ReportMeasureDefinition
                    //{
                    //    Path = "Id",
                    //    Aggregation = "avg",
                    //    Label = "Average",
                    //    Label2 = "المعدل",
                    //    Label3 = "我的密"
                    //}
                },
                ShowColumnsTotal = false,
                ShowRowsTotal = true,
                EntityMetadata = new EntityMetadata
                {
                    ["State"] = FieldMetadata.Loaded,
                    ["Type"] = FieldMetadata.Loaded,
                    ["Title"] = FieldMetadata.Loaded,
                    ["Description"] = FieldMetadata.Loaded,
                    ["Description2"] = FieldMetadata.Loaded,
                }
            },
            ["my-awesome-report"] = new ReportDefinition
            {
                Id = "my-awesome-report",
                Title = "My Awesome Report",
                Title2 = "تقريري العجيب",
                Title3 = "我的惊人报告",
                MainMenuIcon = "chart-pie",
                MainMenuSection = "Financials",
                MainMenuSortKey = 203m,
                State = "Archived",
                // Top = 10,
                Type = ReportType.Details,
                Collection = "MeasurementUnit",
                Filter = "UnitType eq @UnitType",
                OrderBy = "BaseAmount desc",
                Parameters = new List<ReportParameterDefinition>
                {
                },
                Select = new List<ReportSelectDefinition>
                {
                    new ReportSelectDefinition
                    {
                        Path = ""
                    },
                    new ReportSelectDefinition
                    {
                        Path = "Description"
                    },
                    new ReportSelectDefinition
                    {
                        Path = "UnitType"
                    },
                    new ReportSelectDefinition
                    {
                        Path = "CreatedBy"
                    },
                    new ReportSelectDefinition
                    {
                        Path = "CreatedBy/State"
                    },
                    new ReportSelectDefinition
                    {
                        Path = "BaseAmount",
                        Label = "My Base Amount",
                        Label2 = "مقداري الأساسي",
                        Label3 = "我的惊人报告"
                    }
                },
                EntityMetadata = new EntityMetadata
                {
                    ["State"] = FieldMetadata.Loaded,
                    ["Type"] = FieldMetadata.Loaded,
                    ["Title"] = FieldMetadata.Loaded,
                    ["Description"] = FieldMetadata.Loaded,
                    ["Description2"] = FieldMetadata.Loaded,
                }
            }
        };
    }
}
