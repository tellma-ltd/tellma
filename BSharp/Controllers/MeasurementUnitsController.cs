using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/measurement-units")]
    public class MeasurementUnitsController : CrudControllerBase<M.MeasurementUnit, MeasurementUnit, MeasurementUnitForSave, int?>
    {
        private readonly ApplicationContext _db;
        private readonly IStringLocalizer<MeasurementUnitsController> _localizer;

        public MeasurementUnitsController(ApplicationContext db, ILogger<MeasurementUnitsController> logger,
            IStringLocalizer<MeasurementUnitsController> localizer, IMapper mapper) : base(logger, localizer, mapper)
        {
            _db = db;
            _localizer = localizer;
        }

        protected override IQueryable<M.MeasurementUnit> GetBaseQuery()
        {
            return _db.MeasurementUnits.Where(e => e.UnitType != "Currency");
        }

        protected override IQueryable<M.MeasurementUnit> Search(IQueryable<M.MeasurementUnit> query, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name1.Contains(search) || e.Name2.Contains(search) || e.Code.Contains(search)); // Custom
            }

            return query;
        }

        protected override IQueryable<M.MeasurementUnit> SingletonQuery(IQueryable<M.MeasurementUnit> query, int? id)
        {
            return query.Where(e => e.Id == id);
        }

        protected override IQueryable<M.MeasurementUnit> IncludeInactive(IQueryable<M.MeasurementUnit> query, bool includeInactive)
        {
            if (!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }

            return query;
        }

        protected override async Task ValidateAsync(List<MeasurementUnitForSave> entities)
        {
            // Hash the indices for performance
            var indices = new Dictionary<MeasurementUnitForSave, int>();
            int i = 0;
            foreach (var entity in entities)
            {
                indices[entity] = i++;
            }

            // Detect if the incoming collection has any duplicate codes
            var duplicateCodes = entities.GroupBy(e => e.Code).Where(g => g.Count() > 1);
            foreach (var groupWithDuplicateCodes in duplicateCodes)
            {
                foreach (var entity in groupWithDuplicateCodes)
                {
                    var index = indices[entity];
                    ModelState.AddModelError($"[{index}].{nameof(entity.Code)}", _localizer["TheCode{0}IsDuplicated", entity.Code]);
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // Perform SQL-side validation
            DataTable entitiesTable = DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable) { TypeName = $"dbo.{nameof(MeasurementUnitForSave)}List", SqlDbType = SqlDbType.Structured };
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

            // (1) Code must be unique
            var sqlErrors = await _db.Validation.FromSql($@"
SET NOCOUNT ON;
DECLARE @ValidationErrors dbo.ValidationErrorList;
	-- Code must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Code' As [Key], N'TheCode{{0}}IsUsed' As [ErrorName],
		FE.Code AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @MeasurementUnits FE 
	JOIN [dbo].MeasurementUnits BE ON FE.Code = BE.Code
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id);
	-- Name must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Name' As [Key], N'TheName{{0}}IsUsed' As [ErrorName],
		FE.[Name] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @MeasurementUnits FE 
	JOIN [dbo].MeasurementUnits BE ON FE.[Name] = BE.[Name]
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id);
		-- Add further logic

SELECT TOP {remainingErrorCount} [Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]
FROM @ValidationErrors;
", entitiesTvp).ToListAsync();

            // Local function for intelligently parsing strings into objects
            object Parse(string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }

                if (DateTime.TryParse(str, out DateTime dResult))
                {
                    return dResult;
                }

                return str;
            }

            // Loop over the errors returned from SQL and add them to ModelState
            foreach (var sqlError in sqlErrors)
            {
                object[] formatArguments = {
                    Parse(sqlError.Argument1),
                    Parse(sqlError.Argument2),
                    Parse(sqlError.Argument3),
                    Parse(sqlError.Argument4),
                    Parse(sqlError.Argument5),
                };

                string key = sqlError.Key;
                string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

                ModelState.AddModelError(key: key, errorMessage: errorMessage);
            }
        }

        protected override async Task<List<M.MeasurementUnit>> SaveAsync(List<MeasurementUnitForSave> entities, bool returnEntities)
        {
            // Add created entities
            DataTable entitiesTable = DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"dbo.{nameof(MeasurementUnitForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            string saveSql = $@"
-- Procedure: MeasurementUnitsSave
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId NVARCHAR(450) = CONVERT(NVARCHAR(450), SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].MeasurementUnits AS t
		USING (
			SELECT [Index], [Id], [Code], [UnitType], [Name1], [Name2], [UnitAmount], [BaseAmount]
			FROM @MeasurementUnits 
			WHERE [EntityState] IN (N'Inserted', N'Updated')
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[UnitType]	= s.[UnitType],
				t.[Name1]		= s.[Name1],
				t.[Name2]		= s.[Name2],
				t.[UnitAmount]	= s.[UnitAmount],
				t.[BaseAmount]	= s.[BaseAmount],
				t.[Code]		= s.[Code],
				t.[ModifiedAt]	= @Now,
				t.[ModifiedBy]	= @UserId
		WHEN NOT MATCHED THEN
				INSERT ([TenantId], [UnitType], [Name1], [Name2], [UnitAmount], [BaseAmount], [Code], [CreatedAt], [CreatedBy], [ModifiedAt], [ModifiedBy])
				VALUES (@TenantId, s.[UnitType], s.[Name1], s.[Name2], s.[UnitAmount], s.[BaseAmount], s.[Code], @Now, @UserId, @Now, @UserId)
			OUTPUT s.[Index], inserted.[Id] 
	) As x
";
            // Optimization
            if(!returnEntities)
            {
                // IF no returned items are expected, simply execute a non-Query and return an empty list;
                await _db.Database.ExecuteSqlCommandAsync(saveSql, entitiesTvp);
                return new List<M.MeasurementUnit>();
            }
            else
            {
                // If returned items are expected, append a select statement to the SQL command
                saveSql = saveSql += "SELECT * FROM @IndexedIds;";

                // Retrieve the map from Indexes to Ids
                var indexedIds = await _db.Saving.FromSql(saveSql, entitiesTvp).ToListAsync();

                // Load the entities using their Ids
                DataTable idsTable = DataTable(indexedIds.Select(e => new { e.Id }), addIndex: false);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"dbo.IdList",
                    SqlDbType = SqlDbType.Structured
                };
                var savedEntities = await _db.MeasurementUnits.FromSql("SELECT * FROM dbo.[MeasurementUnits] WHERE Id IN @Ids", idsTvp).ToListAsync();


                // SQL Server does not guarantee order, so make sure the result is sorted according to the initial index
                Dictionary<int, int> indices = indexedIds.ToDictionary(e => e.Id, e => e.Index);
                var sortedSavedEntities = new List<M.MeasurementUnit>(savedEntities.Count);
                foreach (var item in savedEntities)
                {
                    int index = indices[item.Id];
                    sortedSavedEntities[index] = item;
                }

                // Return the sorted collection
                return sortedSavedEntities;
            }
        }

        protected override async Task DeleteAsync(List<int?> ids)
        {
            // Prepare a list of Ids to delete
            DataTable idsTable = DataTable(ids.Select(e => new { Id = e }), addIndex: false);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"dbo.IdList",
                SqlDbType = SqlDbType.Structured
            };

            // Delete efficiently with a SQL query
            await _db.Database.ExecuteSqlCommandAsync("DELETE FROM dbo.[MeasurementUnits] WHERE Id IN @Ids", idsTvp);
        }

        protected override Task<List<M.MeasurementUnit>> ActionAsync(List<int?> entities, string action)
        {
            throw new NotImplementedException();
        }
    }
}
