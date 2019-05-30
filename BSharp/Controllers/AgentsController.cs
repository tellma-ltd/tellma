//using AutoMapper;
//using BSharp.Controllers.DTO;
//using BSharp.Controllers.Misc;
//using BSharp.Data;
//using BSharp.Services.FilterParser;
//using BSharp.Services.ImportExport;
//using BSharp.Services.MultiTenancy;
//using BSharp.Services.Utilities;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ModelBinding;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Localization;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace BSharp.Controllers
//{
//    [Route("api/agents/{agentType}")]
//    [LoadTenantInfo]
//    public class AgentsController : CrudControllerBaseOld<AgentForSave, Agent, AgentForQuery, int?>
//    {
//        // Hard-coded agent types
//        private const string ORGANIZATION = "organizations";
//        private const string INDIVIDUAL = "individuals";

//        // Prepare agent types
//        private readonly string[] _agentTypes = { INDIVIDUAL, ORGANIZATION };

//        private readonly ApplicationContext _db;
//        private readonly IModelMetadataProvider _metadataProvider;
//        private readonly ILogger<AgentsController> _logger;
//        private readonly IStringLocalizer<AgentsController> _localizer;
//        private readonly ITenantUserInfoAccessor _tenantInfo;
//        private readonly IFilterParser _filterParser;

//        public AgentsController(ILogger<AgentsController> logger, IStringLocalizer<AgentsController> localizer,
//            IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
//        {
//            _db = serviceProvider.GetRequiredService<ApplicationContext>();
//            _metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();
//            _tenantInfo = serviceProvider.GetRequiredService<ITenantUserInfoAccessor>();
//            _filterParser = serviceProvider.GetRequiredService<IFilterParser>();

//            _logger = logger;
//            _localizer = localizer;
//        }

//        [HttpPut("activate")]
//        public async Task<ActionResult<EntitiesResponse<Agent>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments<int> args)
//        {
//            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(() =>
//                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: true)
//            , _logger);
//        }

//        [HttpPut("deactivate")]
//        public async Task<ActionResult<EntitiesResponse<Agent>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments<int> args)
//        {
//            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(() =>
//                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: false)
//            , _logger);
//        }

//        private async Task<ActionResult<EntitiesResponse<Agent>>> ActivateDeactivate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
//        {
//            await CheckActionPermissions(ids.Cast<int?>());

//            var isActiveParam = new SqlParameter("@IsActive", isActive);

//            DataTable idsTable = ControllerUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: false);
//            var idsTvp = new SqlParameter("@Ids", idsTable)
//            {
//                TypeName = $"dbo.IdList",
//                SqlDbType = SqlDbType.Structured
//            };

//            string sql = @"
//DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
//DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

//MERGE INTO [dbo].[Custodies] AS t
//	USING (
//		SELECT [Id]
//		FROM @Ids
//	) AS s ON (t.Id = s.Id)
//	WHEN MATCHED AND (t.IsActive <> @IsActive)
//	THEN
//		UPDATE SET 
//			t.[IsActive]	= @IsActive,
//			t.[ModifiedAt]	= @Now,
//			t.[ModifiedById]	= @UserId;
//";

//            using (var trx = await _db.Database.BeginTransactionAsync())
//            {
//                try
//                {
//                    // Update the entities
//                    await _db.Database.ExecuteSqlCommandAsync(sql, idsTvp, isActiveParam);
//                    trx.Commit();
//                }
//                catch (Exception ex)
//                {
//                    trx.Rollback();
//                    throw ex;
//                }
//            }

//            // Determine whether entities should be returned
//            if (!returnEntities)
//            {
//                // IF no returned items are expected, simply return 200 OK
//                return Ok();
//            }
//            else
//            {
//                // Load the entities using their Ids
//                var affectedDbEntitiesQ = _db.VW_Agents.Where(e => ids.Contains(e.Id.Value)); //.FromSql("SELECT * FROM [dbo].[Custodies] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
//                var affectedDbEntitiesExpandedQ = Expand(affectedDbEntitiesQ, expand);
//                var affectedDbEntities = await affectedDbEntitiesExpandedQ.ToListAsync();

//                // Add the metadata
//                ApplySelectAndAddMetadata(affectedDbEntities, expand, null);

//                // sort the entities the way their Ids came, as a good practice
//                var affectedEntities = Mapper.Map<List<Agent>>(affectedDbEntities);

//                Agent[] sortedAffectedEntities = new Agent[ids.Count];
//                Dictionary<int, Agent> affectedEntitiesDic = affectedEntities.ToDictionary(e => e.Id.Value);
//                for (int i = 0; i < ids.Count; i++)
//                {
//                    var id = ids[i];
//                    Agent entity = null;
//                    if (affectedEntitiesDic.ContainsKey(id))
//                    {
//                        entity = affectedEntitiesDic[id];
//                    }

//                    sortedAffectedEntities[i] = entity;
//                }

//                // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
//                await ApplyReadPermissionsMask(affectedDbEntities, affectedDbEntitiesExpandedQ, await UserPermissions(PermissionLevel.Read), GetDefaultMask());

//                // Flatten related entities and map each to its respective DTO 
//                var relatedEntities = FlattenRelatedEntitiesAndTrim(affectedDbEntities, expand);

//                // Prepare a proper response
//                var response = new EntitiesResponse<Agent>
//                {
//                    Data = sortedAffectedEntities,
//                    CollectionName = GetCollectionName(typeof(Agent)),
//                    RelatedEntities = relatedEntities
//                };

//                // Commit and return
//                return Ok(response);
//            }
//        }

//        protected string ViewId()
//        {
//            string agentType = RouteData.Values["agentType"]?.ToString();
//            var allowedViewIds = _agentTypes.Union(Enumerable.Repeat(ALL, 1));

//            // Make sure the agentType is supported
//            if (agentType == ALL && (HttpContext.Request.Method != HttpMethods.Get || HttpContext.Request.Path.Value.EndsWith("/template")))
//            {
//                // Programmer mistake
//                throw new BadRequestException("The type 'all' is only supported for HTTP GET requests other than /template");
//            }
//            else if (!allowedViewIds.Contains(agentType))
//            {
//                // Programmer mistake
//                throw new BadRequestException("Only the following agent types are supported: " + string.Join(", ", allowedViewIds));
//            }

//            return agentType;
//        }

//        protected override async Task<IQueryable<AgentForQuery>> ApplyReadPermissionsCriteria(IQueryable<AgentForQuery> query, IEnumerable<AbstractPermission> permissions)
//        {
//            if(ViewId() != ALL)
//            {
//                return await base.ApplyReadPermissionsCriteria(query, permissions);
//            }
//            else
//            {
//                // Get all permissions related to agents
//                var allPermissions = await ControllerUtilities.GetPermissions(_db.AbstractPermissions, PermissionLevel.Read, _agentTypes);
//                if (!allPermissions.Any())
//                {
//                    // User doesn't have access to any type of agent
//                    throw new ForbiddenException();
//                }
//                else if (allPermissions.Any(e => e.ViewId == ALL))
//                {
//                    // Optimization
//                    return query;
//                }
//                else if (_agentTypes.All(t => allPermissions.Any(e => e.ViewId == t && string.IsNullOrWhiteSpace(e.Criteria))))
//                {
//                    // this might be risky if the developer forgets to add an agent type in 'agentTypes' array
//                    return query;
//                }
//                else
//                {
//                    /* IF we reach here it means the user can only see a filtered list of agents
//                     * The purpose of the code below is to construct a dynamic linq query that looks like this:
//                     *
//                     * e => 
//                     * (e.AgentType == "individuals" && <dynamic linq for individuals>) ||
//                     * (e.AgentType == "organizations" && <dynamic linq for organizations>) ||
//                     * 
//                     */

//                    // The parameter on which the dynamic LINQ expression is based
//                    var eParam = Expression.Parameter(typeof(AgentForQuery));

//                    Expression fullExpression = null;
//                    foreach (var g in allPermissions.GroupBy(e => e.ViewId))
//                    {
//                        string viewId = g.Key;

//                        Expression typePropAccess = Expression.Property(eParam, nameof(AgentForQuery.AgentType));
//                        Expression viewIdConstant = Expression.Constant(viewId);
//                        Expression typePropEquality = Expression.Equal(typePropAccess, viewIdConstant);
//                        Expression viewIdExpression;

//                        if (g.Any(e => string.IsNullOrWhiteSpace(e.Criteria)))
//                        {
//                            // The user can read all records of this type
//                            viewIdExpression = typePropEquality;
//                        }
//                        else
//                        {
//                            // The user has access to part of the data set based on a list of filters that will 
//                            // be ORed together in a dynamic linq query
//                            IEnumerable<string> criteriaList = g.Select(e => e.Criteria);

//                            // First criteria
//                            viewIdExpression = _filterParser.ParseFilterExpression<AgentForQuery>(criteriaList.First(), eParam);

//                            // The remaining criteria
//                            foreach (var criteria in criteriaList.Skip(1))
//                            {
//                                var criteriaExpression = _filterParser.ParseFilterExpression<AgentForQuery>(criteria, eParam);
//                                viewIdExpression = Expression.OrElse(viewIdExpression, criteriaExpression);
//                            }

//                            viewIdExpression = Expression.AndAlso(typePropEquality, viewIdExpression);
//                        }

//                        // OR this viewId expression with the remaining viewId expressions
//                        fullExpression = fullExpression == null ? viewIdExpression : Expression.OrElse(fullExpression, viewIdExpression);
//                    }

//                    var lambda = Expression.Lambda<Func<AgentForQuery, bool>>(fullExpression, eParam);
//                    return query.Where(lambda);
//                }
//            }
//        }

//        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level)
//        {
//            var viewId = ViewId();
//            if(viewId == ALL)
//            {
//                return ControllerUtilities.GetPermissions(_db.AbstractPermissions, level, _agentTypes);
//            }
//            else
//            {
//                return ControllerUtilities.GetPermissions(_db.AbstractPermissions, level, viewId);
//            }
//        }

//        private string SingularName()
//        {
//            return _localizer[ViewId()];
//        }

//        private string PluralName()
//        {
//            return _localizer[ViewId() + "s"]; // Works for both Individual and Organization
//        }

//        protected override async Task<IDbContextTransaction> BeginSaveTransaction()
//        {
//            return await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
//        }

//        protected override IQueryable<AgentForQuery> GetBaseQuery()
//        {
//            string agentType = ViewId();
//            return agentType == ALL ? _db.VW_Agents : _db.VW_Agents.Where(e => e.AgentType == agentType);
//        }

//        protected override IQueryable<AgentForQuery> Search(IQueryable<AgentForQuery> query, string search, IEnumerable<AbstractPermission> permissions)
//        {
//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                query = query.Where(e => e.Name.Contains(search) || e.Name2.Contains(search) || e.Code.Contains(search));
//            }

//            return query;
//        }

//        protected override IQueryable<AgentForQuery> IncludeInactive(IQueryable<AgentForQuery> query, bool inactive)
//        {
//            if (!inactive)
//            {
//                query = query.Where(e => e.IsActive == true);
//            }

//            return query;
//        }

//        protected override async Task ValidateAsync(List<AgentForSave> entities)
//        {
//            // Get the agent type from the context
//            string agentType = ViewId();

//            // Hash the indices for performance
//            var indices = entities.ToIndexDictionary();

//            // Check that Ids make sense in relation to EntityState, and that no entity is DELETED
//            // All these errors indicate a bug
//            foreach (var entity in entities)
//            {
//                if (entity.EntityState == EntityStates.Deleted)
//                {
//                    // Won't be supported for this API
//                    var index = indices[entity];
//                    ModelState.AddModelError($"[{index}].{nameof(entity.EntityState)}", _localizer["Error_Deleting0IsNotSupportedFromThisAPI", PluralName()]);
//                }
//            }

//            // Check that Ids are unique
//            var duplicateIds = entities.Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1);
//            foreach (var groupWithDuplicateIds in duplicateIds)
//            {
//                foreach (var entity in groupWithDuplicateIds)
//                {
//                    // This error indicates a bug
//                    var index = indices[entity];
//                    ModelState.AddModelError($"[{index}].{nameof(entity.Id)}", _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", entity.Id]);
//                }
//            }

//            // No need to invoke SQL if the model state is full of errors
//            if (ModelState.HasReachedMaxErrors)
//            {
//                return;
//            }

//            // Perform SQL-side validation
//            DataTable entitiesTable = ControllerUtilities.DataTable(entities, addIndex: true);
//            var entitiesTvp = new SqlParameter("Entities", entitiesTable) { TypeName = $"dbo.{nameof(AgentForSave)}List", SqlDbType = SqlDbType.Structured };
//            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

//            // (1) Code must be unique
//            var sqlErrors = await _db.Validation.FromSql($@"
//SET NOCOUNT ON;
//	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

//    INSERT INTO @ValidationErrors([Key], [ErrorName])
//    SELECT '[' + CAST([Id] AS NVARCHAR(255)) + '].Id' As [Key], N'Error_CannotModifyInactiveItem' As [ErrorName]
//    FROM @Entities
//    WHERE Id IN (SELECT Id from [dbo].[Custodies] WHERE IsActive = 0)
//	OPTION(HASH JOIN);

//    -- Non Null Ids must exist
//    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
//    SELECT '[' + CAST([Id] AS NVARCHAR(255)) + '].Id' As [Key], N'Error_TheId0WasNotFound' As [ErrorName], CAST([Id] As NVARCHAR(255)) As [Argument1]
//    FROM @Entities
//    WHERE Id Is NOT NULL AND Id NOT IN (SELECT Id from [dbo].[Custodies] WHERE CustodyType = 'Agent' AND AgentType = '{agentType}')

//	-- Code must be unique
//	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
//	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Code' As [Key], N'Error_TheCode0IsUsed' As [ErrorName],
//		FE.Code AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
//	FROM @Entities FE 
//	JOIN [dbo].[Custodies] BE ON FE.Code = BE.Code
//	WHERE (FE.Id IS NULL) OR (FE.Id <> BE.Id);

//SELECT TOP {remainingErrorCount} * FROM @ValidationErrors;
//", entitiesTvp).ToListAsync();

//            // Loop over the errors returned from SQL and add them to ModelState
//            foreach (var sqlError in sqlErrors)
//            {
//                var formatArguments = sqlError.ToFormatArguments();

//                string key = sqlError.Key;
//                string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

//                ModelState.AddModelError(key: key, errorMessage: errorMessage);
//            }
//        }

//        protected override async Task<(List<AgentForQuery>, IQueryable<AgentForQuery>)> PersistAsync(List<AgentForSave> entities, SaveArguments args)
//        {
//            // Some properties are always set to null for organizations
//            string agentType = ViewId();
//            if (agentType == ORGANIZATION)
//            {
//                entities.ForEach(e =>
//                {
//                    e.Title = null;
//                    e.Title2 = null;
//                    e.Gender = null;
//                });
//            }

//            // Add created entities
//            DataTable entitiesTable = ControllerUtilities.DataTable(entities, addIndex: true);
//            var entitiesTvp = new SqlParameter("Entities", entitiesTable)
//            {
//                TypeName = $"dbo.{nameof(AgentForSave)}List",
//                SqlDbType = SqlDbType.Structured
//            };

//            // The agent type
//            var agentTypeParameter = new SqlParameter("AgentType", agentType);

//            string saveSql = $@"
//-- Procedure: AgentsForSave
//SET NOCOUNT ON;
//	DECLARE @IndexedIds [dbo].[IndexedIdList];
//	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
//	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
//    DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

//-- Deletions
//	DELETE FROM [dbo].[Custodies]
//	WHERE [Id] IN (SELECT [Id] FROM @Entities WHERE [EntityState] = N'Deleted');

//	INSERT INTO @IndexedIds([Index], [Id])
//	SELECT x.[Index], x.[Id]
//	FROM
//	(
//		MERGE INTO [dbo].[Custodies] AS t
//		USING (
//			SELECT [Index], [Id], [Name], [Name2], [Code], [Address], [BirthDateTime], [IsRelated], [TaxIdentificationNumber], [Title], [Title2], [Gender]
//			FROM @Entities 
//			WHERE [EntityState] IN (N'Inserted', N'Updated')
//		) AS s ON (t.Id = s.Id)
//		WHEN MATCHED
//		THEN
//			UPDATE SET 
//				t.[Name]			        = s.[Name],
//				t.[Name2]			        = s.[Name2],
//				t.[Code]			        = s.[Code],
//				t.[Address]			        = s.[Address],
//				t.[BirthDateTime]	        = s.[BirthDateTime],
//			    t.[IsRelated]				= s.[IsRelated],
//			    t.[TaxIdentificationNumber] = s.[TaxIdentificationNumber],
//			    t.[Title]					= s.[Title],
//			    t.[Title2]					= s.[Title2],
//			    t.[Gender]					= s.[Gender],
//				t.[ModifiedAt]		        = @Now,
//				t.[ModifiedById]		        = @UserId
//		WHEN NOT MATCHED THEN
//			INSERT ([TenantId], [CustodyType], [Name], [Name2], [Code], [Address], [BirthDateTime], [AgentType], [IsRelated], [TaxIdentificationNumber], [Title], [Title2], [Gender], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
//			VALUES (@TenantId, 'Agent', s.[Name], s.[Name2], s.[Code], s.[Address], s.[BirthDateTime], @AgentType, s.[IsRelated], s.[TaxIdentificationNumber], s.[Title], [Title2], s.[Gender], @Now, @UserId, @Now, @UserId)
//		OUTPUT s.[Index], inserted.[Id] 
//	) AS x;
//";
//            // Optimization
//            if (!(args.ReturnEntities ?? false))
//            {
//                // IF no returned items are expected, simply execute a non-Query and return an empty list;
//                await _db.Database.ExecuteSqlCommandAsync(saveSql, entitiesTvp, agentTypeParameter);
//                return (new List<AgentForQuery>(), null);
//            }
//            else
//            {
//                // If returned items are expected, append a select statement to the SQL command
//                saveSql = saveSql += "SELECT * FROM @IndexedIds;";

//                // Retrieve the map from Indexes to Ids
//                var indexedIds = await _db.Saving.FromSql(saveSql, entitiesTvp, agentTypeParameter).ToListAsync();

//                //// Load the entities using their Ids
//                //DataTable idsTable = DataTable(indexedIds.Select(e => new { e.Id }), addIndex: false);
//                //var idsTvp = new SqlParameter("Ids", idsTable)
//                //{
//                //    TypeName = $"dbo.IdList",
//                //    SqlDbType = SqlDbType.Structured
//                //};

//                // var q = _db.Agents.FromSql("SELECT * FROM dbo.[Custodies] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
//                var ids = indexedIds.Select(e => e.Id);
//                var q = _db.VW_Agents.Where(e => ids.Contains(e.Id.Value));
//                q = Expand(q, args.Expand);
//                var savedEntities = await q.ToListAsync();


//                // SQL Server does not guarantee order, so make sure the result is sorted according to the initial index
//                Dictionary<int, int> indices = indexedIds.ToDictionary(e => e.Id, e => e.Index);
//                var sortedSavedEntities = new AgentForQuery[savedEntities.Count];
//                foreach (var item in savedEntities)
//                {
//                    int index = indices[item.Id.Value];
//                    sortedSavedEntities[index] = item;
//                }

//                // Return the sorted collection
//                return (sortedSavedEntities.ToList(), q);
//            }
//        }

//        protected override async Task DeleteAsync(List<int?> ids)
//        {
//            // Prepare a list of Ids to delete
//            DataTable idsTable = ControllerUtilities.DataTable(ids.Select(e => new { Id = e }), addIndex: false);
//            var idsTvp = new SqlParameter("Ids", idsTable)
//            {
//                TypeName = $"dbo.IdList",
//                SqlDbType = SqlDbType.Structured
//            };

//            using (var trx = await _db.Database.BeginTransactionAsync())
//            {
//                try
//                {
//                    // Delete efficiently with a SQL query
//                    string agentType = ViewId();
//                    await _db.Database.ExecuteSqlCommandAsync(
//                        $"DELETE FROM dbo.[Custodies] WHERE CustodyType = 'Agent' AND AgentType = '{agentType}' AND Id IN (SELECT Id FROM @Ids)"
//                        , idsTvp);

//                    // Commit and return
//                    trx.Commit();
//                    return;
//                }
//                catch (SqlException ex) when (IsForeignKeyViolation(ex))
//                {
//                    throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", SingularName()]);
//                }
//                catch (Exception ex)
//                {
//                    trx.Rollback();
//                    throw ex;
//                }
//            }
//        }

//        protected override AbstractDataGrid GetImportTemplate()
//        {
//            // Get the properties of the DTO for Save, excluding Id or EntityState
//            var custodyType = typeof(CustodyForSave);
//            var agentType = typeof(AgentForSave);
//            var custodyProps = custodyType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
//            var agentProps = agentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
//            var props = custodyProps.Union(agentProps);

//            if (ViewId() == ORGANIZATION)
//            {
//                // For organizations, some properties are left blank
//                var exemptProperties = new string[] { nameof(Agent.Title), nameof(Agent.Title2), nameof(Agent.Gender) };
//                props.Where(p => !exemptProperties.Contains(p.Name));
//            }

//            var propsArray = props.ToArray();

//            // The result that will be returned
//            var result = new AbstractDataGrid(propsArray.Length, 1);

//            // Add the header
//            var header = result[result.AddRow()];
//            int i = 0;
//            foreach (var prop in props)
//            {
//                var display = _metadataProvider.GetMetadataForProperty(agentType, prop.Name)?.DisplayName ?? prop.Name;
//                if (display != Constants.Hidden)
//                {
//                    header[i++] = AbstractDataCell.Cell(display);
//                }
//            }

//            return result;
//        }

//        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Agent> response, ExportArguments args)
//        {
//            // Get all the properties without Id and EntityState
//            var type = typeof(Agent);
//            var custodySaveProps = typeof(CustodyForSave).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
//            var agentSaveProps = typeof(AgentForSave).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

//            var readProps = typeof(Agent).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
//            var saveProps = custodySaveProps.Union(agentSaveProps);

//            var props = saveProps.Union(readProps);

//            if (ViewId() == ORGANIZATION)
//            {
//                // For organizations, some properties are left blank
//                var exemptProperties = new string[] { nameof(Agent.Title), nameof(Agent.Title2), nameof(Agent.Gender) };
//                props.Where(p => !exemptProperties.Contains(p.Name));
//            }

//            var propsArray = props.ToArray();

//            // The result that will be returned
//            var result = new AbstractDataGrid(propsArray.Length, response.Data.Count() + 1);

//            // Add the header
//            List<PropertyInfo> addedProps = new List<PropertyInfo>(propsArray.Length);
//            {
//                var header = result[result.AddRow()];
//                int i = 0;
//                foreach(var prop in propsArray)
//                {
//                    var display = _metadataProvider.GetMetadataForProperty(type, prop.Name)?.DisplayName ?? prop.Name;
//                    if (display != Constants.Hidden)
//                    {
//                        header[i] = AbstractDataCell.Cell(display);

//                        // Add the proper styling
//                        if (prop.PropertyType.IsDateOrTime())
//                        {
//                            var att = prop.GetCustomAttribute<DataTypeAttribute>();
//                            var isDateOnly = att != null && att.DataType == DataType.Date;
//                            header[i].NumberFormat = ExportDateTimeFormat(dateOnly: isDateOnly);
//                        }

//                        addedProps.Add(prop);
//                        i++;
//                    }
//                }
//            }


//            // Add the rows
//            foreach (var entity in response.Data)
//            {
//                var metadata = entity.EntityMetadata;
//                var row = result[result.AddRow()];
//                int i = 0;
//                foreach (var prop in addedProps)
//                {
//                    metadata.TryGetValue(prop.Name, out FieldMetadata meta);
//                    if (meta == FieldMetadata.Loaded)
//                    {
//                        var content = prop.GetValue(entity);

//                        // Special handling for choice lists
//                        var choiceListAttr = prop.GetCustomAttribute<ChoiceListAttribute>();
//                        if (choiceListAttr != null)
//                        {
//                            var choiceIndex = Array.FindIndex(choiceListAttr.Choices, e => e.Equals(content));
//                            if (choiceIndex != -1)
//                            {
//                                string displayName = choiceListAttr.DisplayNames[choiceIndex];
//                                content = _localizer[displayName];
//                            }
//                        }

//                        // Special handling for DateTimeOffset
//                        if (prop.PropertyType.IsDateTimeOffset() && content != null)
//                        {
//                            content = ToExportDateTime((DateTimeOffset)content);
//                        }

//                        row[i] = AbstractDataCell.Cell(content);
//                    }
//                    else if (meta == FieldMetadata.Restricted)
//                    {
//                        row[i] = AbstractDataCell.Cell(Constants.Restricted);
//                    }
//                    else
//                    {
//                        row[i] = AbstractDataCell.Cell("-");
//                    }

//                    i++;
//                }
//            }

//            return result;
//        }

//        protected override async Task<(List<AgentForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args)
//        {
//            // Get the properties of the DTO for Save, excluding Id or EntityState
//            string mode = args.Mode;
//            var readType = typeof(Agent);
//            var custodySaveType = typeof(CustodyForSave);
//            var agentSaveType = typeof(AgentForSave);

//            var readProps = readType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
//                .ToDictionary(prop => _metadataProvider.GetMetadataForProperty(readType, prop.Name)?.DisplayName ?? prop.Name, StringComparer.InvariantCultureIgnoreCase);

//            var orgExemptProperties = new string[] { nameof(Agent.Title), nameof(Agent.Title2), nameof(Agent.Gender) };

//            var saveProps = custodySaveType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
//                .Union(agentSaveType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
//                .Where(e => ViewId() == INDIVIDUAL || orgExemptProperties.Contains(e.Name)) // Take away
//                .ToDictionary(prop => _metadataProvider.GetMetadataForProperty(agentSaveType, prop.Name)?.DisplayName ?? prop.Name, StringComparer.InvariantCultureIgnoreCase);

//            // Maps the index of the grid column to a property on the DtoForSave
//            var saveColumnMap = new List<(int Index, PropertyInfo Property)>(grid.RowSize);

//            // Make sure all column header labels are recognizable
//            // and construct the save column map
//            var firstRow = grid[0];
//            for (int c = 0; c < firstRow.Length; c++)
//            {
//                var column = firstRow[c];
//                string headerLabel = column.Content?.ToString();

//                // So any thing after an empty column is ignored
//                if (string.IsNullOrWhiteSpace(headerLabel))
//                    break;

//                if (saveProps.ContainsKey(headerLabel))
//                {
//                    var prop = saveProps[headerLabel];
//                    saveColumnMap.Add((c, prop));
//                }
//                else if (readProps.ContainsKey(headerLabel))
//                {
//                    // All good, just ignore
//                }
//                else
//                {
//                    AddRowError(1, _localizer["Error_Column0NotRecognizable", headerLabel]);
//                }
//            }

//            // Milestone 1: columns in the abstract grid mapped
//            if (!ModelState.IsValid)
//            {
//                throw new UnprocessableEntityException(ModelState);
//            }

//            // Construct the result using the map generated earlier
//            List<AgentForSave> result = new List<AgentForSave>(grid.Count - 1);
//            for (int i = 1; i < grid.Count; i++) // Skip the header
//            {
//                var row = grid[i];

//                // Anything after an empty row is ignored
//                if (saveColumnMap.All((p) => string.IsNullOrWhiteSpace(row[p.Index].Content?.ToString())))
//                {
//                    break;
//                }

//                var entity = new AgentForSave();
//                foreach (var (index, prop) in saveColumnMap)
//                {
//                    var content = row[index].Content;
//                    var propName = _metadataProvider.GetMetadataForProperty(readType, prop.Name).DisplayName;

//                    // Special handling for choice lists
//                    if (content != null)
//                    {
//                        var choiceListAttr = prop.GetCustomAttribute<ChoiceListAttribute>();
//                        if (choiceListAttr != null)
//                        {
//                            List<string> displayNames = choiceListAttr.DisplayNames.Select(e => _localizer[e].Value).ToList();
//                            string stringContent = content.ToString();
//                            var displayNameIndex = displayNames.IndexOf(stringContent);
//                            if (displayNameIndex == -1)
//                            {
//                                string seperator = _localizer[", "];
//                                AddRowError(i + 1, _localizer["Error_Value0IsNotValidFor1AcceptableValuesAre2", stringContent, propName, string.Join(seperator, displayNames)]);
//                            }
//                            else
//                            {
//                                content = choiceListAttr.Choices[displayNameIndex];
//                            }
//                        }
//                    }

//                    // Special handling for DateTime and DateTimeOffset
//                    if (prop.PropertyType.IsDateOrTime())
//                    {
//                        try
//                        {
//                            var date = ParseImportedDateTime(content);
//                            content = date;

//                            if (prop.PropertyType.IsDateTimeOffset())
//                            {
//                                content = AddUserTimeZone(date);
//                            }
//                        }
//                        catch (Exception)
//                        {
//                            AddRowError(i + 1, _localizer["Error_TheValue0IsNotValidFor1Field", content?.ToString(), propName]);
//                        }
//                    }

//                    // Try setting the value and return an error if it doesn't work
//                    try
//                    {
//                        prop.SetValue(entity, content);
//                    }
//                    catch (ArgumentException)
//                    {
//                        AddRowError(i + 1, _localizer["Error_TheValue0IsNotValidFor1Field", content?.ToString(), propName]);
//                    }
//                }

//                result.Add(entity);
//            }

//            // Milestone 2: DTOs created
//            if (!ModelState.IsValid)
//            {
//                throw new UnprocessableEntityException(ModelState);
//            }

//            // Prepare a dictionary of indices in order to construct any validation errors performantly
//            // "IndexOf" is O(n), this brings it down to O(1)
//            Dictionary<AgentForSave, int> indicesDic = result.ToIndexDictionary();

//            // For each entity, set the Id and EntityState depending on import mode
//            if (mode == "Insert")
//            {
//                // For Insert mode, all are marked inserted and all Ids are null
//                // Any duplicate codes will be handled later in the validation
//                result.ForEach(e => e.Id = null);
//                result.ForEach(e => e.EntityState = EntityStates.Inserted);
//            }
//            else
//            {
//                // For all other modes besides Insert, we need to match the entity codes to Ids by querying the DB
//                // Load the code Ids from the database
//                var nonNullCodes = result.Where(e => !string.IsNullOrWhiteSpace(e.Code));
//                var codesDataTable = ControllerUtilities.DataTable(nonNullCodes.Select(e => new { e.Code }));
//                var entitiesTvp = new SqlParameter("@Codes", codesDataTable)
//                {
//                    TypeName = $"dbo.CodeList",
//                    SqlDbType = SqlDbType.Structured
//                };

//                string agentType = ViewId();

//                var idCodesDic = await _db.CodeIds.FromSql(
//                    $@"SELECT c.Code, e.Id FROM @Codes c JOIN [dbo].[Custodies] e ON c.Code = e.Code WHERE e.CustodyType = 'Agent' && e.AgentType == {agentType};"
//                    , entitiesTvp).ToDictionaryAsync(e => e.Code, e => e.Id);

//                result.ForEach(e =>
//                {
//                    if (!string.IsNullOrWhiteSpace(e.Code) && idCodesDic.ContainsKey(e.Code))
//                    {
//                        e.Id = idCodesDic[e.Code];
//                    }
//                    else
//                    {
//                        e.Id = null;
//                    }
//                });

//                // Make sure no codes are mentioned twice, if we don't do it here, the save validation later will complain
//                // about duplicated Id, but the error will not be clear since user deals with code while importing from Excel              
//                var duplicateIdGroups = result.Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1);
//                foreach (var duplicateIdGroup in duplicateIdGroups)
//                {
//                    foreach (var entity in duplicateIdGroup)
//                    {
//                        int index = indicesDic[entity];
//                        AddRowError(index + 2, _localizer["Error_TheCode0IsDuplicated", entity.Code]);
//                    }
//                }

//                if (mode == "Merge")
//                {
//                    // Merge simply inserts codes that are not found, and updates codes that are found
//                    result.ForEach(e =>
//                    {
//                        if (e.Id != null)
//                        {
//                            e.EntityState = EntityStates.Updated;
//                        }
//                        else
//                        {
//                            e.EntityState = EntityStates.Inserted;
//                        }
//                    });
//                }
//                else
//                {
//                    // In the case of update: codes are required, and MUST match database Ids
//                    if (mode == "Update")
//                    {
//                        for (int index = 0; index < result.Count; index++)
//                        {
//                            var entity = result[index];
//                            if (string.IsNullOrWhiteSpace(entity.Code))
//                            {
//                                AddRowError(index + 2, _localizer["Error_CodeIsRequiredForImportModeUpdate"]);
//                            }
//                            else if (entity.Id == null)
//                            {
//                                AddRowError(index + 2, _localizer["Error_TheCode0DoesNotExist", entity.Code]);
//                            }
//                        }

//                        result.ForEach(e => e.EntityState = EntityStates.Updated);
//                    }
//                    else
//                    {
//                        throw new InvalidOperationException("Unknown save mode"); // Developer bug
//                    }
//                }
//            }

//            // Milestone 3: Id and EntityState are set
//            if (!ModelState.IsValid)
//            {
//                throw new UnprocessableEntityException(ModelState);
//            }

//            // Function that maps any future validation errors back to specific rows
//            int? errorKeyMap(string key)
//            {
//                int? rowNumber = null;
//                if (key != null && key.StartsWith("["))
//                {
//                    var indexStr = key.TrimStart('[').Split(']')[0];
//                    if (int.TryParse(indexStr, out int index))
//                    {
//                        // Add 2:
//                        // 1 for the header in the abstract grid
//                        // 1 for the difference between index and number
//                        rowNumber = index + 2;
//                    }
//                }
//                return rowNumber;
//            }

//            return (result, errorKeyMap);
//        }

//        protected override async Task CheckPermissionsForNew(IEnumerable<AgentForSave> newItems, Expression<Func<AgentForQuery, bool>> lambda)
//        {
//            // Add created entities
//            DataTable entitiesTable = ControllerUtilities.DataTable(newItems.ToList(), addIndex: true);
//            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
//            {
//                TypeName = $"dbo.{nameof(AgentForSave)}List",
//                SqlDbType = SqlDbType.Structured
//            };

//            // Other parameters
//            var custodyType = new SqlParameter("@CustodyType", "Agent");
//            var agentType = new SqlParameter("@AgentType", ViewId());

//            string saveSql = $@"
//	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
//	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
//	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
//	DECLARE @True BIT = 1;

//    SELECT  @TenantId AS TenantId, ISNULL(E.Id, 0) AS Id, E.Name, E.Name2, E.Code, E.Address, E.BirthDateTime, 
//    E.IsRelated, E.TaxIdentificationNumber, E.Title, E.Title2, E.Gender, @CustodyType AS CustodyType, @AgentType AS AgentType,
//    @True AS IsActive, @Now AS CreatedAt, @UserId AS CreatedById, @Now AS ModifiedAt, @UserId AS ModifiedById 
//    FROM @Entities E
//";
//            var countBeforeFilter = newItems.Count();
//            var countAfterFilter = await _db.VW_Agents.FromSql(saveSql, entitiesTvp, custodyType, agentType).Where(lambda).CountAsync();

//            if (countBeforeFilter > countAfterFilter)
//            {
//                throw new ForbiddenException();
//            }
//        }

//        protected override async Task CheckPermissionsForOld(IEnumerable<int?> entityIds, Expression<Func<AgentForQuery, bool>> lambda)
//        {
//            // Load the entities using their Ids
//            DataTable idsTable = ControllerUtilities.DataTable(entityIds.Where(e => e != null).Select(id => new { Id = id.Value }));
//            var idsTvp = new SqlParameter("Ids", idsTable)
//            {
//                TypeName = $"dbo.IdList",
//                SqlDbType = SqlDbType.Structured
//            };

//            // apply the lambda
//            var q = _db.VW_Agents.FromSql("SELECT * FROM [dbo].[Custodies] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
//            int countBeforeFilter = await q.CountAsync();
//            int countAfterFilter = await q.Where(lambda).CountAsync();

//            if (countBeforeFilter > countAfterFilter)
//            {
//                throw new ForbiddenException();
//            }
//        }
//    }
//}