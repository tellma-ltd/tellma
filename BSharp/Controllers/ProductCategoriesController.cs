using BSharp.Controllers.Dto;
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
    [Route("api/product-categories")]
    [ApiController]
    public class ProductCategoriesController : CrudTreeControllerBase<ProductCategoryForSave, ProductCategory, int?>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ITenantUserInfoAccessor _tenantInfo;

        public ProductCategoriesController(ILogger<ProductCategoriesController> logger, IStringLocalizer<Strings> localizer,
            IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _logger = logger;
            _localizer = localizer;

            _db = serviceProvider.GetRequiredService<ApplicationContext>();
            _metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();
            _tenantInfo = serviceProvider.GetRequiredService<ITenantUserInfoAccessor>();
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<ProductCategory>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(() =>
                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: true)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<ProductCategory>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(() =>
                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<ProductCategory>>> ActivateDeactivate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            var nullableIds = ids.Cast<int?>().ToArray();
            await CheckActionPermissions(nullableIds);

            using (var trx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // TODO Validate that all children are inactive


                    var isActiveParam = new SqlParameter("@IsActive", isActive);

                    DataTable idsTable = ControllerUtilities.DataTable(ids.Select(id => new { Id = id }));
                    var idsTvp = new SqlParameter("@Ids", idsTable)
                    {
                        TypeName = $"dbo.IdList",
                        SqlDbType = SqlDbType.Structured
                    };

                    string sql = @"
DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

MERGE INTO [dbo].[ProductCategories] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[ModifiedAt]	= @Now,
			t.[ModifiedById]= @UserId;
";

                    // Update the entities
                    await _db.Database.ExecuteSqlCommandAsync(sql, idsTvp, isActiveParam);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw ex;
                }
            }

            // Determine whether entities should be returned
            if (returnEntities)
            {
                // Return results
                var response = await GetByIdListAsync(nullableIds, expand);
                return Ok(response);
            }
            else
            {
                // IF no returned items are expected, simply return 200 OK
                return Ok();
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await ControllerUtilities.GetPermissions(_db.AbstractPermissions, action, "product-categories");
        }

        protected override DbContext GetRepository()
        {
            return _db;
        }

        protected override Func<Type, string> GetSources()
        {
            var info = _tenantInfo.GetCurrentInfo();
            return ControllerUtilities.GetApplicationSources(_localizer, info.PrimaryLanguageId, info.SecondaryLanguageId, info.TernaryLanguageId);
        }

        protected override ODataQuery<ProductCategory> Search(ODataQuery<ProductCategory> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(ProductCategory.Name);
                var name2 = nameof(ProductCategory.Name2);
                var name3 = nameof(ProductCategory.Name3);
                var code = nameof(ProductCategory.Code);

                query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<ProductCategoryForSave> entities)
        {
            // Hash the indices for performance
            var indices = entities.ToIndexDictionary();

            // Check that Ids make sense in relation to EntityState, and that no entity is DELETED
            // All these errors indicate a bug
            foreach (var entity in entities)
            {
                if (entity.EntityState == EntityStates.Deleted)
                {
                    // Won't be supported for this API
                    var index = indices[entity];
                    ModelState.AddModelError($"[{index}].{nameof(entity.EntityState)}", _localizer["Error_Deleting0IsNotSupportedFromThisAPI", _localizer["ProductCategories"]]);
                }
            }

            // Check that Ids are unique
            var duplicateIds = entities.Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1);
            foreach (var groupWithDuplicateIds in duplicateIds)
            {
                foreach (var entity in groupWithDuplicateIds)
                {
                    // This error indicates a bug
                    var index = indices[entity];
                    ModelState.AddModelError($"[{index}].{nameof(entity.Id)}", _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", entity.Id]);
                }
            }

            // Perform SQL-side validation
            DataTable entitiesTable = ControllerUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("Entities", entitiesTable) { TypeName = $"dbo.{nameof(ProductCategory)}List", SqlDbType = SqlDbType.Structured };
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

            // Code, Name and Name2 must be unique
            var sqlErrors = await _db.Validation.FromSql($@"
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
    SELECT '[' + CAST([Index] AS NVARCHAR (255)) + '].Id' As [Key], N'Error_CannotModifyInactiveItem' As [ErrorName], NULL As [Argument1]
    FROM @Entities
    WHERE Id IN (SELECT Id from [dbo].[ProductCategories] WHERE IsActive = 0)
	OPTION(HASH JOIN);

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
    SELECT '[' + CAST([Index] AS NVARCHAR (255)) + '].Id' As [Key], N'Error_TheId0WasNotFound' As [ErrorName], CAST([Id] As NVARCHAR (255)) As [Argument1]
    FROM @Entities
    WHERE Id Is NOT NULL
	AND Id NOT IN (SELECT Id from [dbo].[ProductCategories])
	OPTION(HASH JOIN);

	-- Code must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code' As [Key], N'Error_TheCode0IsUsed' As [ErrorName],
		FE.Code AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities FE 
	JOIN [dbo].[ProductCategories] BE ON FE.Code = BE.Code
	WHERE FE.[Code] IS NOT NULL
	AND ((FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id))
	OPTION(HASH JOIN);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR (255)) + '].Code' As [Key], N'Error_TheCode0IsDuplicated' As [ErrorName],
		[Code] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities
	WHERE [Code] IN (
		SELECT [Code]
		FROM @Entities
		WHERE [Code] IS NOT NULL
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name' As [Key], N'Error_TheName0IsUsed' As [ErrorName],
		FE.[Name] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities FE 
	JOIN [dbo].[ProductCategories] BE ON FE.[Name] = BE.[Name]
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name2 must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name2' As [Key], N'Error_TheName0IsUsed' As [ErrorName],
		FE.[Name2] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities FE 
	JOIN [dbo].[ProductCategories] BE ON FE.[Name2] = BE.[Name2]
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name3 must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name3' As [Key], N'Error_TheName0IsUsed' As [ErrorName],
		FE.[Name3] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities FE 
	JOIN [dbo].[ProductCategories] BE ON FE.[Name3] = BE.[Name3]
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR (255)) + '].Name' As [Key], N'Error_TheName0IsDuplicated' As [ErrorName],
		[Name] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities
	WHERE [Name] IN (
		SELECT [Name]
		FROM @Entities
		GROUP BY [Name]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name2 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR (255)) + '].Name2' As [Key], N'Error_TheName0IsDuplicated' As [ErrorName],
		[Name2] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities
	WHERE [Name2] IN (
		SELECT [Name2]
		FROM @Entities
		WHERE [Name2] IS NOT NULL
		GROUP BY [Name2]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name3 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR (255)) + '].Name3' As [Key], N'Error_TheName0IsDuplicated' As [ErrorName],
		[Name3] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities
	WHERE [Name3] IN (
		SELECT [Name3]
		FROM @Entities
		WHERE [Name3] IS NOT NULL
		GROUP BY [Name3]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

SELECT TOP {remainingErrorCount} * FROM @ValidationErrors;
", entitiesTvp).ToListAsync();

            // Loop over the errors returned from SQL and add them to ModelState
            foreach (var sqlError in sqlErrors)
            {
                var formatArguments = sqlError.ToFormatArguments();

                string key = sqlError.Key;
                string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

                ModelState.AddModelError(key: key, errorMessage: errorMessage);
            }

        }

        protected override async Task<List<int?>> SaveExecuteAsync(List<ProductCategoryForSave> entities, SaveArguments args)
        {
            // Add created entities
            DataTable entitiesTable = ControllerUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("Entities", entitiesTable)
            {
                TypeName = $"dbo.{nameof(ProductCategory)}List",
                SqlDbType = SqlDbType.Structured
            };

            string saveSql = $@"
-- Procedure: ProductCategoriesSave
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[ProductCategories] AS t
		USING (
			SELECT
				E.[Index], E.[Id], E.[ParentId],
				HIERARCHYID::Parse('/' + CAST(-ABS(CHECKSUM(NewId()) % 2147483648) AS VARCHAR(30)) + '/') AS [Node],
				E.[Name], E.[Name2], E.[Name3], E.[Code]
			FROM @Entities E
			WHERE [EntityState] IN (N'Inserted', N'Updated')
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[ParentId]		= s.[ParentId],
				t.[Name]			= s.[Name],
				t.[Name2]			= s.[Name2],
				t.[Name3]			= s.[Name3],
				t.[Code]			= s.[Code],
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ( [ParentId], [Node], [Name],	[Name2], [Name3], [Code])
			VALUES (s.[Parentid], s.[Node], s.[Name], s.[Name2], s.[Name3], s.[Code])
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

	-- The following code is needed for bulk import, when the reliance is on Parent Index
	MERGE [dbo].[ProductCategories] As t
	USING (
		SELECT II.[Id], IIParent.[Id] As ParentId
		FROM @Entities O
		JOIN @IndexedIds IIParent ON IIParent.[Index] = O.ParentIndex
		JOIN @IndexedIds II ON II.[Index] = O.[Index]
	) As s
	ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[ParentId] = s.[ParentId];

	-- reorganize the nodes
	WITH Children ([Id], [ParentId], [Num]) AS (
		SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E2.[Id])
		FROM [dbo].[ProductCategories] E
		LEFT JOIN [dbo].[ProductCategories] E2 ON E.[ParentId] = E2.[Id]
	),
	Paths ([Node], [Id]) AS (  
		-- This section provides the value for the roots of the hierarchy  
		SELECT CAST(('/'  + CAST(C.Num AS VARCHAR(30)) + '/') AS HIERARCHYID) AS [Node], [Id]
		FROM Children AS C   
		WHERE [ParentId] IS NULL
		UNION ALL   
		-- This section provides values for all nodes except the root  
		SELECT CAST(P.[Node].ToString() + CAST(C.Num AS VARCHAR(30)) + '/' AS HIERARCHYID), C.[Id]
		FROM Children C
		JOIN Paths P ON C.[ParentId] = P.[Id]
	)
	MERGE INTO [dbo].[ProductCategories] As t
	USING Paths As s ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];
";
            // Optimization
            bool returnEntities = args.ReturnEntities ?? false;
            if (!returnEntities)
            {
                // IF no returned items are expected, simply execute a non-Query and return an empty list;
                await _db.Database.ExecuteSqlCommandAsync(saveSql, entitiesTvp);
                return null;
            }
            else
            {
                // If returned items are expected, append a select statement to the SQL command
                saveSql = saveSql += "SELECT * FROM @IndexedIds;";

                // Retrieve the map from Indexes to Ids
                var indexedIds = await _db.Saving.FromSql(saveSql, entitiesTvp).ToListAsync();

                // return the Ids in the same order they came
                return indexedIds.OrderBy(e => e.Index).Select(e => (int?)e.Id).ToList();
            }
        }

        protected override Task DeleteValidateAsync(List<int?> ids)
        {
            return Task.CompletedTask;
            //// Perform SQL-side validation
            //DataTable idsTable = ControllerUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: true);
            //var indexedIdsTvp = new SqlParameter("@Entities", idsTable)
            //{
            //    TypeName = $"dbo.IndexedIdList",
            //    SqlDbType = SqlDbType.Structured
            //};

            //int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

            //// Code, Name and Name2 must be unique
            //var sqlErrors = await _db.Validation.FromSql($@"
            //    SET NOCOUNT ON;
	           //     DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	           //     WITH DeletedSet AS
	           //     (
		          //      SELECT E.[Index], T.[Node], T.[ParentNode]
		          //      FROM dbo.ProductCategories T 
		          //      JOIN @Entities E ON T.[Id] = E.[Id]
	           //     )
	           //     INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
            //        SELECT DISTINCT '[' + CAST(S.[Index] AS NVARCHAR (255)) + ']' As [Key], N'Error_CannotDeleteNodeWithChildren' As [ErrorName], NULL As [Argument1]
	           //     FROM [dbo].[ProductCategories] T -- get me every node in the table
	           //     JOIN DeletedSet S ON T.[ParentNode] = S.[Node] -- whose parent is to be deleted
	           //     WHERE T.[Node] NOT IN (SELECT [Node] FROM DeletedSet) -- but it is not going to be deleted
            //        -- OPTION(HASH JOIN);

            //SELECT TOP {remainingErrorCount} * FROM @ValidationErrors;
            //", indexedIdsTvp).ToListAsync();

            //// Loop over the errors returned from SQL and add them to ModelState
            //foreach (var sqlError in sqlErrors)
            //{
            //    var formatArguments = sqlError.ToFormatArguments();

            //    string key = sqlError.Key;
            //    string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

            //    ModelState.AddModelError(key: key, errorMessage: errorMessage);
            //}
        }

        protected override async Task DeleteExecuteAsync(List<int?> ids)
        {
            // Prepare a list of Ids to delete
            DataTable idsTable = ControllerUtilities.DataTable(ids.Select(e => new { Id = e }), addIndex: false);
            var idsTvp = new SqlParameter("Entities", idsTable)
            {
                TypeName = $"dbo.IdList",
                SqlDbType = SqlDbType.Structured
            };

            using (var trx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Delete efficiently with a SQL query
                    await _db.Database.ExecuteSqlCommandAsync(@"
	    -- Delete the entites, children Parent Id will be set to NULL
	    DELETE FROM [dbo].[ProductCategories]
	    WHERE [Id] IN (SELECT [Id] FROM @Entities);

	    -- reorganize the nodes
	    WITH Children ([Id], [ParentId], [Num]) AS (
		    SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E2.[Id])
		    FROM [dbo].[ProductCategories] E
		    LEFT JOIN [dbo].[ProductCategories] E2 ON E.[ParentId] = E2.[Id]
	    ),
	    Paths ([Node], [Id]) AS (  
		    -- This section provides the value for the roots of the hierarchy  
		    SELECT CAST(('/'  + CAST(C.Num AS VARCHAR(30)) + '/') AS HIERARCHYID) AS [Node], [Id]
		    FROM Children AS C   
		    WHERE [ParentId] IS NULL
		    UNION ALL   
		    -- This section provides values for all nodes except the root  
		    SELECT CAST(P.[Node].ToString() + CAST(C.Num AS VARCHAR(30)) + '/' AS HIERARCHYID), C.[Id]
		    FROM Children C
		    JOIN Paths P ON C.[ParentId] = P.[Id]
	    )
	    MERGE INTO [dbo].[ProductCategories] As t
	    USING Paths As s ON (t.[Id] = s.[Id] AND t.[Node] <> s.[Node])
	    WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];
", idsTvp);

                    // Commit and return
                    trx.Commit();
                    return;
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ProductCategory"]]);
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw ex;
                }
            }
        }

        protected override Task ValidateDeleteWithDescendantsAsync(List<int?> ids)
        {
            return Task.CompletedTask;
            //            // Perform SQL-side validation
            //            DataTable idsTable = ControllerUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: true);
            //            var indexedIdsTvp = new SqlParameter("@Entities", idsTable)
            //            {
            //                TypeName = $"dbo.IndexedIdList",
            //                SqlDbType = SqlDbType.Structured
            //            };

            //            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

            //            // Code, Name and Name2 must be unique
            //            var sqlErrors = await _db.Validation.FromSql($@"
            //SET NOCOUNT ON;
            //	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
            //	WITH DeletedSet AS
            //	(
            //		SELECT E.[Index], T.[Node], T.[ParentNode]
            //		FROM dbo.ProductCategories T 
            //		JOIN @Entities E ON T.[Id] = E.[Id]
            //	)
            //	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
            //    SELECT DISTINCT '[' + CAST(S.[Index] AS NVARCHAR (255)) + '].Id' As [Key], N'Error_CannotDeleteNodeWithChildren' As [ErrorName], NULL As [Argument1]
            //	FROM [dbo].[ProductCategories] T -- get me every node in the table
            //	JOIN DeletedSet S ON T.[ParentNode] = S.[Node] -- whose parent is to be deleted
            //	WHERE T.[Node] NOT IN (SELECT [Node] FROM DeletedSet) -- but it is not going to be deleted
            //	OPTION(HASH JOIN);

            //SELECT TOP {remainingErrorCount} * FROM @ValidationErrors;
            //", indexedIdsTvp).ToListAsync();

            //            // Loop over the errors returned from SQL and add them to ModelState
            //            foreach (var sqlError in sqlErrors)
            //            {
            //                var formatArguments = sqlError.ToFormatArguments();

            //                string key = sqlError.Key;
            //                string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

            //                ModelState.AddModelError(key: key, errorMessage: errorMessage);
            //            }
        }

        protected override async Task DeleteWithDescendantsAsync(List<int?> ids)
        {
            // Prepare a list of Ids to delete
            DataTable idsTable = ControllerUtilities.DataTable(ids.Select(e => new { Id = e }), addIndex: false);
            var idsTvp = new SqlParameter("Entities", idsTable)
            {
                TypeName = $"dbo.IdList",
                SqlDbType = SqlDbType.Structured
            };

            using (var trx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Delete efficiently with a SQL query
                    await _db.Database.ExecuteSqlCommandAsync(@"
	IF NOT EXISTS(SELECT * FROM @Entities) RETURN;

	-- Delete the entites, children Parent Id will be set to NULL
	WITH EntitiesWithDescendants
	AS (
		SELECT T2.[Id]
		FROM [dbo].[ProductCategories] T1
		JOIN [dbo].[ProductCategories] T2
		ON T2.[Node].IsDescendantOf(T1.[Node]) = 1
		WHERE T1.[Id] IN (SELECT [Id] FROM @Entities)
	)
	DELETE FROM [dbo].[ProductCategories]
	WHERE [Id] IN (SELECT [Id] FROM EntitiesWithDescendants);

	-- reorganize the nodes
	WITH Children ([Id], [ParentId], [Num]) AS (
		SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E2.[Id])
		FROM [dbo].[ProductCategories] E
		LEFT JOIN [dbo].[ProductCategories] E2 ON E.[ParentId] = E2.[Id]
	),
	Paths ([Node], [Id]) AS (  
		-- This section provides the value for the roots of the hierarchy  
		SELECT CAST(('/'  + CAST(C.Num AS VARCHAR(30)) + '/') AS HIERARCHYID) AS [Node], [Id]
		FROM Children AS C   
		WHERE [ParentId] IS NULL
		UNION ALL   
		-- This section provides values for all nodes except the root  
		SELECT CAST(P.[Node].ToString() + CAST(C.Num AS VARCHAR(30)) + '/' AS HIERARCHYID), C.[Id]
		FROM Children C
		JOIN Paths P ON C.[ParentId] = P.[Id]
	)
	MERGE INTO [dbo].[ProductCategories] As t
	USING Paths As s ON (t.[Id] = s.[Id] AND t.[Node] <> s.[Node])
	WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];
", idsTvp);

                    // Commit and return
                    trx.Commit();
                    return;
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ProductCategory"]]);
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw ex;
                }
            }
        }

        protected override AbstractDataGrid GetImportTemplate()
        {
            // Get the properties of the DTO for Save, excluding Id or EntityState
            var type = typeof(ProductCategoryForSave);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            // The result that will be returned
            var result = new AbstractDataGrid(props.Length, 1);

            // Add the header
            var header = result[result.AddRow()];
            int i = 0;
            foreach (var prop in props)
            {
                var display = _metadataProvider.GetMetadataForProperty(type, prop.Name)?.DisplayName ?? prop.Name;
                if (display != Constants.Hidden)
                {
                    header[i++] = AbstractDataCell.Cell(display);
                }
            }

            return result;
        }

        protected override AbstractDataGrid EntitiesToAbstractGrid(GetResponse<ProductCategory> response, ExportArguments args)
        {
            // Get all the properties without Id and EntityState
            var type = typeof(ProductCategory);
            var readProps = typeof(ProductCategory).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var saveProps = typeof(ProductCategoryForSave).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var props = saveProps.Union(readProps).ToArray();

            // The result that will be returned
            var result = new AbstractDataGrid(props.Length, response.Result.Count() + 1);

            // Add the header
            List<PropertyInfo> addedProps = new List<PropertyInfo>(props.Length);
            {
                var header = result[result.AddRow()];
                int i = 0;
                foreach (var prop in props)
                {
                    var display = _metadataProvider.GetMetadataForProperty(type, prop.Name)?.DisplayName ?? prop.Name;
                    if (display != Constants.Hidden)
                    {
                        header[i] = AbstractDataCell.Cell(display);

                        // Add the proper styling for DateTime and DateTimeOffset
                        if (prop.PropertyType.IsDateOrTime())
                        {
                            var att = prop.GetCustomAttribute<DataTypeAttribute>();
                            var isDateOnly = att != null && att.DataType == DataType.Date;
                            header[i].NumberFormat = ExportDateTimeFormat(dateOnly: isDateOnly);
                        }

                        addedProps.Add(prop);
                        i++;
                    }
                }
            }

            //// Add the rows
            //foreach (var entity in response.Ids)
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

            return result;
        }

        protected override async Task<(List<ProductCategoryForSave>, Func<string, int?>)> ToEntitiesForSave(AbstractDataGrid grid, ParseArguments args)
        {
            // Get the properties of the DTO for Save, excluding Id or EntityState
            string mode = args.Mode;
            var readType = typeof(ProductCategory);
            var saveType = typeof(ProductCategoryForSave);

            var readProps = readType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToDictionary(prop => _metadataProvider.GetMetadataForProperty(readType, prop.Name)?.DisplayName ?? prop.Name, StringComparer.InvariantCultureIgnoreCase);

            var saveProps = saveType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToDictionary(prop => _metadataProvider.GetMetadataForProperty(saveType, prop.Name)?.DisplayName ?? prop.Name, StringComparer.InvariantCultureIgnoreCase);

            // Maps the index of the grid column to a property on the DtoForSave
            var saveColumnMap = new List<(int Index, PropertyInfo Property)>(grid.RowSize);

            // Make sure all column header labels are recognizable
            // and construct the save column map
            var firstRow = grid[0];
            for (int c = 0; c < firstRow.Length; c++)
            {
                var column = firstRow[c];
                string headerLabel = column.Content?.ToString();

                // So any thing after an empty column is ignored
                if (string.IsNullOrWhiteSpace(headerLabel))
                    break;

                if (saveProps.ContainsKey(headerLabel))
                {
                    var prop = saveProps[headerLabel];
                    saveColumnMap.Add((c, prop));
                }
                else if (readProps.ContainsKey(headerLabel))
                {
                    // All good, just ignore
                }
                else
                {
                    AddRowError(1, _localizer["Error_Column0NotRecognizable", headerLabel]);
                }
            }

            // Milestone 1: columns in the abstract grid mapped
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Construct the result using the map generated earlier
            List<ProductCategoryForSave> result = new List<ProductCategoryForSave>(grid.Count - 1);
            for (int i = 1; i < grid.Count; i++) // Skip the header
            {
                var row = grid[i];

                // Anything after an empty row is ignored
                if (saveColumnMap.All((p) => string.IsNullOrWhiteSpace(row[p.Index].Content?.ToString())))
                {
                    break;
                }

                var entity = new ProductCategoryForSave();
                foreach (var (index, prop) in saveColumnMap)
                {
                    var content = row[index].Content;
                    var propName = _metadataProvider.GetMetadataForProperty(readType, prop.Name).DisplayName;

                    // Special handling for choice lists
                    if (content != null)
                    {
                        var choiceListAttr = prop.GetCustomAttribute<ChoiceListAttribute>();
                        if (choiceListAttr != null)
                        {
                            List<string> displayNames = choiceListAttr.DisplayNames.Select(e => _localizer[e].Value).ToList();
                            string stringContent = content.ToString();
                            var displayNameIndex = displayNames.IndexOf(stringContent);
                            if (displayNameIndex == -1)
                            {
                                string seperator = _localizer[", "];
                                AddRowError(i + 1, _localizer["Error_Value0IsNotValidFor1AcceptableValuesAre2", stringContent, propName, string.Join(seperator, displayNames)]);
                            }
                            else
                            {
                                content = choiceListAttr.Choices[displayNameIndex];
                            }
                        }
                    }

                    // Special handling for DateTime and DateTimeOffset
                    if (prop.PropertyType.IsDateOrTime())
                    {
                        try
                        {
                            var date = ParseImportedDateTime(content);
                            content = date;

                            if (prop.PropertyType.IsDateTimeOffset())
                            {
                                content = AddUserTimeZone(date);
                            }
                        }
                        catch (Exception)
                        {
                            AddRowError(i + 1, _localizer["Error_TheValue0IsNotValidFor1Field", content?.ToString(), propName]);
                        }
                    }

                    try
                    {
                        prop.SetValue(entity, content); // TODO casting here to be done
                    }
                    catch (ArgumentException)
                    {
                        AddRowError(i + 1, _localizer["Error_TheValue0IsNotValidFor1Field", content?.ToString(), propName]);
                    }
                }

                result.Add(entity);
            }

            // Milestone 2: DTOs created
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Prepare a dictionary of indices in order to construct any validation errors performantly
            // "IndexOf" is O(n), this brings it down to O(1)
            Dictionary<ProductCategoryForSave, int> indicesDic = result.ToIndexDictionary();

            // For each entity, set the Id and EntityState depending on import mode
            if (mode == "Insert")
            {
                // For Insert mode, all are marked inserted and all Ids are null
                // Any duplicate codes will be handled later in the validation
                result.ForEach(e => e.Id = null);
                result.ForEach(e => e.EntityState = EntityStates.Inserted);
            }
            else
            {
                // For all other modes besides Insert, we need to match the entity codes to Ids by querying the DB
                // Load the code Ids from the database
                var nonNullCodes = result.Where(e => !string.IsNullOrWhiteSpace(e.Code));
                var codesDataTable = ControllerUtilities.DataTable(nonNullCodes.Select(e => new { e.Code }));
                var entitiesTvp = new SqlParameter("@Codes", codesDataTable)
                {
                    TypeName = $"dbo.CodeList",
                    SqlDbType = SqlDbType.Structured
                };

                string sql = $@"SELECT c.Code, e.Id FROM @Codes c JOIN [dbo].[ProductCategories] e ON c.Code = e.Code WHERE e.UnitType <> 'Money';";
                var idCodesDic = await _db.CodeIds.FromSql(sql, entitiesTvp).ToDictionaryAsync(e => e.Code, e => e.Id);

                result.ForEach(e =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Code) && idCodesDic.ContainsKey(e.Code))
                    {
                        e.Id = idCodesDic[e.Code];
                    }
                    else
                    {
                        e.Id = null;
                    }
                });

                // Make sure no codes are mentioned twice, if we don't do it here, the save validation later will complain
                // about duplicated Id, but the error will not be clear since user deals with code while importing from Excel              
                var duplicateIdGroups = result.Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1);
                foreach (var duplicateIdGroup in duplicateIdGroups)
                {
                    foreach (var entity in duplicateIdGroup)
                    {
                        int index = indicesDic[entity];
                        AddRowError(index + 2, _localizer["Error_TheCode0IsDuplicated", entity.Code]);
                    }
                }

                if (mode == "Merge")
                {
                    // Merge simply inserts codes that are not found, and updates codes that are found
                    result.ForEach(e =>
                    {
                        if (e.Id != null)
                        {
                            e.EntityState = EntityStates.Updated;
                        }
                        else
                        {
                            e.EntityState = EntityStates.Inserted;
                        }
                    });
                }
                else
                {
                    // In the case of update: codes are required, and MUST match database Ids
                    if (mode == "Update")
                    {
                        for (int index = 0; index < result.Count; index++)
                        {
                            var entity = result[index];
                            if (string.IsNullOrWhiteSpace(entity.Code))
                            {
                                AddRowError(index + 2, _localizer["Error_CodeIsRequiredForImportModeUpdate"]);
                            }
                            else if (entity.Id == null)
                            {
                                AddRowError(index + 2, _localizer["Error_TheCode0DoesNotExist", entity.Code]);
                            }
                        }

                        result.ForEach(e => e.EntityState = EntityStates.Updated);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown save mode"); // Developer bug
                    }
                }
            }

            // Milestone 3: Id and EntityState are set
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Function maps any future validation errors back to specific rows
            int? errorKeyMap(string key)
            {
                int? rowNumber = null;
                if (key != null && key.StartsWith("["))
                {
                    var indexStr = key.TrimStart('[').Split(']')[0];
                    if (int.TryParse(indexStr, out int index))
                    {
                        // Add 2:
                        // 1 for the header in the abstract grid
                        // 1 for the difference between index and number
                        rowNumber = index + 2;
                    }
                }
                return rowNumber;
            }

            return (result, errorKeyMap);
        }

        protected override (string PreambleSql, string ComposableSql, List<SqlParameter> Parameters) GetAsQuery(IEnumerable<ProductCategoryForSave> entities)
        {
            // Preamble SQL
            string preambleSql =
                $@"DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
            	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
            	DECLARE @True BIT = 1;";

            // Composable SQL
            string sql =
                $@"SELECT ISNULL(E.Id, 0) AS Id, E.Name, E.Name2, E.Name3, E.Code, @True AS IsActive,
                E.ParentId, NULL As [Level], NULL As [ChildCount], 
                (SELECT CAST([Node].ToString() + CAST(1 As NVARCHAR(MAX)) + N'/' As HIERARCHYID) FROM [dbo].[ProductCategories] WHERE Id = E.ParentId) As [Node],
                (SELECT [Node] FROM [dbo].[ProductCategories] WHERE Id = E.ParentId) As [ParentNode],
                NULL As [ParentNode], @Now AS CreatedAt, @UserId AS CreatedById, @Now AS ModifiedAt, @UserId AS ModifiedById 
                FROM @Entities E";

            // Entities TVP put in a singleton
            DataTable entitiesTable = ControllerUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("Entities", entitiesTable)
            {
                TypeName = $"dbo.{nameof(ProductCategory)}List",
                SqlDbType = SqlDbType.Structured
            };

            var ps = new List<SqlParameter>() { entitiesTvp };

            // Return the result
            return (preambleSql, sql, ps);
        }
    }
}