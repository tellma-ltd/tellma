using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
using BSharp.Services.OData;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/ifrs-notes")]
    [LoadTenantInfo]
    public class IfrsNotesController : ReadControllerBase<IfrsNote, IfrsNoteForQuery, string>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<IfrsNotesController> _logger;
        private readonly IStringLocalizer<IfrsNotesController> _localizer;
        private readonly ITenantUserInfoAccessor _tenantInfo;

        public IfrsNotesController(ILogger<IfrsNotesController> logger, IStringLocalizer<IfrsNotesController> localizer,
            IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _logger = logger;
            _localizer = localizer;

            _db = serviceProvider.GetRequiredService<ApplicationContext>();
            _metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();
            _tenantInfo = serviceProvider.GetRequiredService<ITenantUserInfoAccessor>();
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<IfrsNote>>> Activate([FromBody] List<string> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(() =>
                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: true)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<IfrsNote>>> Deactivate([FromBody] List<string> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(() =>
                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<IfrsNote>>> ActivateDeactivate([FromBody] List<string> ids, bool returnEntities, string expand, bool isActive)
        {
            await CheckActionPermissions(ids);

            using (var trx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var isActiveParam = new SqlParameter("@IsActive", isActive);
                    var idsConcatenation = new SqlParameter("@Ids", string.Join('|', ids));

                    string sql = @"
DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

MERGE INTO [dbo].[IfrsConcepts] AS t
	USING (
		SELECT [Id]
		FROM (SELECT VALUE AS Id FROM STRING_SPLIT(@Ids, '|')) As X
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[ModifiedAt]	= @Now,
			t.[ModifiedById]= @UserId;
";

                    // Update the entities
                    await _db.Database.ExecuteSqlCommandAsync(sql, idsConcatenation, isActiveParam);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw ex;
                }
            }

            // Determine whether entities should be returned
            if (!returnEntities)
            {
                // IF no returned items are expected, simply return 200 OK
                return Ok();
            }
            else
            {
                // Load the entities using their Ids
                var affectedDbEntitiesQ = CreateODataQuery().FilterByIds(ids.ToArray());
                var affectedDbEntitiesExpandedQ = affectedDbEntitiesQ.Clone().Expand(expand);
                var affectedDbEntities = await affectedDbEntitiesExpandedQ.ToListAsync();

                // sort the entities the way their Ids came, as a good practice
                var affectedEntities = Mapper.Map<List<IfrsNote>>(affectedDbEntities);
                IfrsNote[] sortedAffectedEntities = new IfrsNote[ids.Count];
                Dictionary<string, IfrsNote> affectedEntitiesDic = affectedEntities.ToDictionary(e => e.Id);
                for (int i = 0; i < ids.Count; i++)
                {
                    var id = ids[i];
                    IfrsNote entity = null;
                    if (affectedEntitiesDic.ContainsKey(id))
                    {
                        entity = affectedEntitiesDic[id];
                    }

                    sortedAffectedEntities[i] = entity;
                }

                // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
                await ApplyReadPermissionsMask(affectedDbEntities, affectedDbEntitiesExpandedQ, await UserPermissions(PermissionLevel.Read), GetDefaultMask());

                // Flatten related entities and map each to its respective DTO 
                var relatedEntities = FlattenRelatedEntitiesAndTrim(affectedDbEntities, expand);

                // Prepare a proper response
                var response = new EntitiesResponse<IfrsNote>
                {
                    Data = sortedAffectedEntities,
                    CollectionName = GetCollectionName(typeof(IfrsNote)),
                    RelatedEntities = relatedEntities
                };

                // Commit and return
                return Ok(response);
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            return await ControllerUtilities.GetPermissions(_db.AbstractPermissions, level, "ifrs-notes");
        }
        
        protected override DbContext GetDbContext()
        {
            return _db;
        }

        protected override Func<Type, string> GetSources()
        {
            var info = _tenantInfo.GetCurrentInfo();
            return ControllerUtilities.GetApplicationSources(_localizer, info.PrimaryLanguageId, info.SecondaryLanguageId, info.TernaryLanguageId);
        }

        protected override ODataQuery<IfrsNoteForQuery, string> Search(ODataQuery<IfrsNoteForQuery, string> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var label = nameof(IfrsNoteForQuery.Label);
                var label2 = nameof(IfrsNoteForQuery.Label2);
                var label3 = nameof(IfrsNoteForQuery.Label3); // TODO

                query.Filter($"{label} {Ops.contains} '{search}' or {label2} {Ops.contains} '{search}' or {label3} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<IfrsNote> response, ExportArguments args)
        {
            throw new NotImplementedException();
            //// Get all the properties without Id and EntityState
            //var type = typeof(IfrsNote);
            //var readProps = typeof(IfrsNote).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            //var saveProps = typeof(IfrsNoteForSave).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            //var props = saveProps.Union(readProps).ToArray();

            //// The result that will be returned
            //var result = new AbstractDataGrid(props.Length, response.Data.Count() + 1);

            //// Add the header
            //List<PropertyInfo> addedProps = new List<PropertyInfo>(props.Length);
            //{
            //    var header = result[result.AddRow()];
            //    int i = 0;
            //    foreach (var prop in props)
            //    {
            //        var display = _metadataProvider.GetMetadataForProperty(type, prop.Name)?.DisplayName ?? prop.Name;
            //        if (display != Constants.Hidden)
            //        {
            //            header[i] = AbstractDataCell.Cell(display);

            //            // Add the proper styling for DateTime and DateTimeOffset
            //            if (prop.PropertyType.IsDateOrTime())
            //            {
            //                var att = prop.GetCustomAttribute<DataTypeAttribute>();
            //                var isDateOnly = att != null && att.DataType == DataType.Date;
            //                header[i].NumberFormat = ExportDateTimeFormat(dateOnly: isDateOnly);
            //            }

            //            addedProps.Add(prop);
            //            i++;
            //        }
            //    }
            //}

            //// Add the rows
            //foreach (var entity in response.Data)
            //{
            //    var metadata = entity.EntityMetadata;
            //    var row = result[result.AddRow()];
            //    int i = 0;
            //    foreach (var prop in addedProps)
            //    {
            //        metadata.TryGetValue(prop.Name, out FieldMetadata meta);
            //        if (meta == FieldMetadata.Loaded)
            //        {
            //            var content = prop.GetValue(entity);

            //            // Special handling for choice lists
            //            var choiceListAttr = prop.GetCustomAttribute<ChoiceListAttribute>();
            //            if (choiceListAttr != null)
            //            {
            //                var choiceIndex = Array.FindIndex(choiceListAttr.Choices, e => e.Equals(content));
            //                if (choiceIndex != -1)
            //                {
            //                    string displayName = choiceListAttr.DisplayNames[choiceIndex];
            //                    content = _localizer[displayName];
            //                }
            //            }

            //            // Special handling for DateTimeOffset
            //            if (prop.PropertyType.IsDateTimeOffset() && content != null)
            //            {
            //                content = ToExportDateTime((DateTimeOffset)content);
            //            }

            //            row[i] = AbstractDataCell.Cell(content);
            //        }
            //        else if (meta == FieldMetadata.Restricted)
            //        {
            //            row[i] = AbstractDataCell.Cell(Constants.Restricted);
            //        }
            //        else
            //        {
            //            row[i] = AbstractDataCell.Cell("-");
            //        }

            //        i++;
            //    }
            //}

            //return result;
        }
    }
}
