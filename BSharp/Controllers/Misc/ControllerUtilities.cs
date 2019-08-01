using BSharp.Controllers.DTO;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers.Misc
{
    public static class ControllerUtilities
    {
        public const string ALL = "all";

        /// <summary>
        /// Constructs a SQL data table containing all the public properties of the 
        /// entities' type and populates the data table with the provided entities
        /// </summary>
        public static DataTable DataTable<T>(IEnumerable<T> entities, bool addIndex = false)
        {
            DataTable table = new DataTable();
            if (addIndex)
            {
                // The column order MUST match the column order in the user-defined table type
                table.Columns.Add(new DataColumn("Index", typeof(int)));
            }

            var props = GetPropertiesBaseFirst(typeof(T)).Where(e => !e.PropertyType.IsList() && e.Name != nameof(DtoBase.EntityMetadata));
            foreach (var prop in props)
            {
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var column = new DataColumn(prop.Name, propType);
                if (propType == typeof(string))
                {
                    // For string columns, it is more performant to explicitly specify the maximum column size
                    // According to this article: http://www.dbdelta.com/sql-server-tvp-performance-gotchas/
                    var stringLengthAttribute = prop.GetCustomAttribute<StringLengthAttribute>(inherit: true);
                    if (stringLengthAttribute != null)
                    {
                        column.MaxLength = stringLengthAttribute.MaximumLength;
                    }
                }

                table.Columns.Add(column);
            }

            int index = 0;
            foreach (var entity in entities)
            {
                DataRow row = table.NewRow();

                // We add an index property since SQL works with un-ordered sets
                if (addIndex)
                {
                    row["Index"] = index++;
                }

                // Add the remaining properties
                foreach (var prop in props)
                {
                    var propValue = prop.GetValue(entity);
                    row[prop.Name] = propValue ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Constructs a SQL data table containing all the entities in all the collections
        /// and adds an index and a header index, this is useful for child collections that
        /// are passed to SQL alongside their headers, the include predicate optionally filters
        /// the items but keeps the original indexing
        /// </summary>
        public static DataTable DataTableWithHeaderIndex<T>(IEnumerable<(List<T> Items, int HeaderIndex)> collections, Predicate<T> include = null)
        {
            include = include ?? (e => true);
            DataTable table = new DataTable();

            // The column order MUST match the column order in the user-defined table type
            table.Columns.Add(new DataColumn("Index", typeof(int)));
            table.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));

            var props = GetPropertiesBaseFirst(typeof(T)).Where(e => !e.PropertyType.IsList() && e.Name != nameof(DtoBase.EntityMetadata));
            foreach (var prop in props)
            {
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var column = new DataColumn(prop.Name, propType);
                if (propType == typeof(string))
                {
                    // For string columns, it is more performant to explicitly specify the maximum column size
                    // According to this article: http://www.dbdelta.com/sql-server-tvp-performance-gotchas/
                    var stringLengthAttribute = prop.GetCustomAttribute<StringLengthAttribute>(inherit: true);
                    if (stringLengthAttribute != null)
                    {
                        column.MaxLength = stringLengthAttribute.MaximumLength;
                    }
                }

                table.Columns.Add(column);
            }

            foreach (var (items, headerIndex) in collections)
            {
                int index = 0;
                foreach (var item in items)
                {
                    if (include(item))
                    {
                        DataRow row = table.NewRow();

                        // We add index and header index properties since SQL works with un-ordered sets
                        row["Index"] = index;
                        row["HeaderIndex"] = headerIndex;

                        // Add the remaining properties
                        foreach (var prop in props)
                        {
                            var propValue = prop.GetValue(item);
                            row[prop.Name] = propValue ?? DBNull.Value;
                        }

                        table.Rows.Add(row);
                    }

                    index++;
                }
            }

            return table;
        }

        /// <summary>
        /// This is alternative for <see cref="Type.GetProperties"/>
        /// that returns base class properties before inherited class properties
        /// Credit: https://bit.ly/2UGAkKj
        /// </summary>
        public static PropertyInfo[] GetPropertiesBaseFirst(Type type)
        {
            var orderList = new List<Type>();
            var iteratingType = type;
            do
            {
                orderList.Insert(0, iteratingType);
                iteratingType = iteratingType.BaseType;
            } while (iteratingType != null);

            var props = type.GetProperties()
                .OrderBy(x => orderList.IndexOf(x.DeclaringType))
                .ToArray();

            return props;
        }

        /// <summary>
        /// Passing the view Id "all" to this method will result in an Exception
        /// </summary>
        /// <returns>The list abstract user permissions that pertain to the current user and specified level and view Ids</returns>
        public static async Task<IEnumerable<AbstractPermission>> GetPermissions(DbQuery<AbstractPermission> q, string action, params string[] viewIds)
        {
            // Validate parameters
            if (q == null)
            {
                // Programmer mistake
                throw new ArgumentNullException(nameof(q));
            }

            if (viewIds == null)
            {
                // Programmer mistake
                throw new ArgumentNullException(nameof(viewIds));
            }

            if (viewIds.Any(e => e == ALL))
            {
                // Programmer mistake
                throw new BadRequestException("'GetPermissions' cannot handle the 'all' case");
            }

            // Add all and prepare the TVP
            viewIds = viewIds.Union(new[] { ALL }).ToArray();
            var viewIdsTable = DataTable(viewIds.Select(e => new { Code = e }));
            var viewIdsTvp = new SqlParameter("@ViewIds", viewIdsTable)
            {
                TypeName = $"dbo.CodeList",
                SqlDbType = SqlDbType.Structured
            };

            var actionParameter = new SqlParameter("Action", action);

            // Prepare the WHERE clause that corresponds to the permission level
            string levelWhereClause;
            switch (action)
            {
                case Constants.Read:
                    levelWhereClause = "";
                    break;

                default:
                    levelWhereClause = $"WHERE E.Action = @Action OR E.Action = 'All'";
                    break;
            }

            // Retrieve the permissions
            // Note: There is another query similiar to this one in PermissionsController
            var result = await q.FromSql($@"
SELECT * FROM (
    SELECT ViewId, Criteria, Mask, Level As Action 
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
    JOIN [dbo].[RoleMemberships] RM ON R.Id = RM.RoleId
    WHERE R.IsActive = 1 
    AND RM.UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId')) 
    AND P.ViewId IN (SELECT Code FROM @ViewIds)
    UNION
    SELECT ViewId, Criteria, Mask, Level As Action 
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
    WHERE R.IsPublic = 1 
    AND R.IsActive = 1
    AND P.ViewId IN (SELECT Code FROM @ViewIds)
) AS E {levelWhereClause}
", actionParameter, viewIdsTvp).ToListAsync();

            return result;
        }

        public static async Task<ActionResult<T>> ExecuteAndHandleErrorsAsync<T>(Func<Task<ActionResult<T>>> func, ILogger logger)
        {
            try
            {
                return await func();
            }
            catch (ForbiddenException)
            {
                return new StatusCodeResult(403);
            }
            catch (NotFoundException<int?> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<string> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (UnprocessableEntityException ex)
            {
                return new UnprocessableEntityObjectResult(ex.ModelState);
            }
            catch (BadRequestException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public static async Task<ActionResult> ExecuteAndHandleErrorsAsync(Func<Task<ActionResult>> func, ILogger logger)
        {
            try
            {
                return await func();
            }
            catch (ForbiddenException)
            {
                return new StatusCodeResult(403);
            }
            catch (NotFoundException<int?> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<string> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (UnprocessableEntityException ex)
            {
                return new UnprocessableEntityObjectResult(ex.ModelState);
            }
            catch (BadRequestException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public static Func<Type, string> GetApplicationSources(IStringLocalizer localizer, string lang1, string lang2, string lang3)
        {
            var loc1 = lang1 == null ? null : localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang1));
            var loc2 = lang2 == null ? null : localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang2));
            var loc3 = lang3 == null ? null : localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang3));

            // TODO Do something about SQL injection risk
            string localize1(string s) => loc1 == null ? "NULL" : $"N'{loc1[s]?.ToString().Replace("'", "''")}'";
            string localize2(string s) => loc2 == null ? "NULL" : $"N'{loc2[s]?.ToString().Replace("'", "''")}'";
            string localize3(string s) => loc3 == null ? "NULL" : $"N'{loc3[s]?.ToString().Replace("'", "''")}'";

            string localize(string s) => $"{localize1(s)},  {localize2(s)},  {localize3(s)}";

            return (t) =>
            {
                switch (t.Name)
                {
                    case nameof(Agent):
                        return "(SELECT * FROM [dbo].[Custodies] WHERE [CustodyType] = 'Agent')";

                    case nameof(Custody):
                        return "[dbo].[Custodies]";

                    case nameof(LocalUser):
                        return "(SELECT *, IIF(ExternalId IS NULL, 'New', 'Confirmed') As [State] FROM [dbo].[LocalUsers])";

                    case nameof(MeasurementUnit):
                        return "(SELECT * FROM [dbo].[MeasurementUnits] WHERE UnitType <> 'Money')";

                    // Temporary TODO: remove
                    case nameof(MeasurementUnitFact):
                        return "(SELECT [TenantId],[Name],[Name2],[Code],[UnitType],[UnitAmount],[BaseAmount],[IsActive],[CreatedAt],[CreatedById],[ModifiedAt],[ModifiedById] FROM [dbo].[MeasurementUnits] WHERE UnitType <> 'Money')";

                    case nameof(Permission):
                        return "(SELECT *, [Level] As [Action] FROM [dbo].[Permissions] WHERE Level <> 'Sign')"; // TODO fix the Level column

                    case nameof(RequiredSignature):
                        return "(SELECT * FROM [dbo].[Permissions] WHERE Level = 'Sign')";

                    case nameof(RoleMembership):
                        return "[dbo].[RoleMemberships]";

                    case nameof(Role):
                        return "[dbo].[Roles]";

                    case nameof(ProductCategory):
                        return @"(SELECT [Q].*,
    (SELECT COUNT(*) FROM [dbo].[ProductCategories] WHERE [IsActive] = 1 AND [Node].IsDescendantOf([Q].[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[ProductCategories] WHERE [Node].IsDescendantOf([Q].[Node]) = 1) As [ChildCount]
FROM [dbo].[ProductCategories] As [Q])";

                    case nameof(IfrsNote):
                        return @"(SELECT 
	[C].*, 
	[N].[Node] As [Node],
	[N].[Level],
	[N].[ParentNode] As [ParentNode],
	[N].[IsAggregate],
	[N].[ForDebit],
	[N].[ForCredit],
	(SELECT COUNT(*) FROM [dbo].[IfrsNotes] As [NI] JOIN [dbo].[IfrsConcepts] As [CI] ON [CI].[Id] = [NI].[Id] WHERE [CI].[IsActive] = 1 AND [NI].[Node].IsDescendantOf([N].[Node]) = 1) As [ActiveChildCount],
	(SELECT COUNT(*) FROM [dbo].[IfrsNotes] As [NI] JOIN [dbo].[IfrsConcepts] As [CI] ON [CI].[Id] = [NI].[Id] WHERE [NI].[Node].IsDescendantOf([N].[Node]) = 1) As [ChildCount],
	(SELECT [Id] FROM [dbo].[IfrsNotes] WHERE [N].[Node].GetAncestor(1) = [Node]) As [ParentId]
FROM [dbo].[IfrsConcepts] As [C] JOIN [dbo].[IfrsNotes] As [N] ON [C].[Id] = [N].[Id])";

                    case nameof(View):
                        var builtInValuesCollection = _builtInViews.Select(e => $"('{e.Id}', {localize(e.Name)})");
                        var builtInValuesString = builtInValuesCollection.Aggregate((s1, s2) => $@"{s1},
{s2}");

                        return $@"(SELECT
 V.[Id], 
 V.Name AS [Name], 
 V.Name2 AS [Name2], 
 V.Name3 AS [Name3], 
 V.[Id] AS [Code], 
 CASE WHEN V.[Id] = 'all' THEN CAST(1 AS BIT) ELSE IsNULL(T.[IsActive], CAST(0 AS BIT)) END AS [IsActive]
FROM 
  (
  VALUES
    {builtInValuesString}
  ) 
AS V ([Id], [Name], [Name2], [Name3])
LEFT JOIN [dbo].[Views] AS T ON V.Id = T.Id)";

                    case nameof(ViewAction):

                        // This takes the original list and transforms it into a friendly format, adding the very common "Read" and "Update" permissions if they are needed
                        var builtInValueActionsCollections = _builtInViews.SelectMany(x =>
                             x.Levels.Select(y => new { Id = $"{x.Id}|{y.Action}", ViewId = x.Id, y.Action, SupportsCriteria = y.Criteria, SupportsMask = false })
                            .Concat(Enumerable.Repeat(new { Id = $"{x.Id}|{Constants.Update}", ViewId = x.Id, Action = Constants.Update, SupportsCriteria = true, SupportsMask = true }, x.Update ? 1 : 0))
                            .Concat(Enumerable.Repeat(new { Id = $"{x.Id}|{Constants.Read}", ViewId = x.Id, Action = Constants.Read, SupportsCriteria = true, SupportsMask = true }, x.Read ? 1 : 0))
                        )
                        .Select(e => $"('{e.Id}', '{e.ViewId}', '{e.Action}', {(e.SupportsCriteria ? "1" : "0")}, {(e.SupportsMask ? "1" : "0")})");

                        var builtInValueActionsString = builtInValueActionsCollections.Aggregate((s1, s2) => $@"{s1},
{s2}");

                        return $@"(SELECT
 [V].[Id], 
 [V].[ViewId] AS [ViewId], 
 [V].[Action] AS [Action], 
 CAST(V.[SupportsCriteria] AS BIT) AS [SupportsCriteria], 
 CAST(V.[SupportsMask] AS BIT) AS [SupportsMask]
FROM 
  (
  VALUES
    {builtInValueActionsString}
  ) 
AS [V] ([Id], [ViewId], [Action], [SupportsCriteria], [SupportsMask])
LEFT JOIN [dbo].[Views] AS [T] ON V.Id = T.Id)";

                }

                return null;
            };
        }

        private static readonly ViewInfo[] _builtInViews = new ViewInfo[]
        {
            new ViewInfo { Id = "all", Name = "View_All", Levels = new LevelInfo[] { Li("Read", false), Li("Update", false) } },
            new ViewInfo { Id = "measurement-units", Name = "MeasurementUnits", Read = true, Update = true , Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "roles", Name = "Roles", Read = true, Update = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "local-users", Name = "Users", Read = true, Update = true, Levels = new LevelInfo[] { Li("IsActive"), Li("ResendInvitationEmail") } },
            new ViewInfo { Id = "views", Name = "Views", Read = true, Update = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "ifrs-notes", Name = "IfrsNotes", Read = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "product-categories", Name = "ProductCategories", Read = true, Update = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "settings", Name = "Settings", Levels = new LevelInfo[] { Li("Read", false), Li("Update", false) } },
        };

        private static LevelInfo Li(string name, bool criteria = true)
        {
            return new LevelInfo { Action = name, Criteria = criteria };
        }
    }

    public class ViewInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Indicates that this view is an endpoint that supports read level, both with Mask and Criteria: OData style
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Indicates that this view is an endpoint that supports read level, both with Mask and Criteria: OData style
        /// </summary>
        public bool Update { get; set; }

        public LevelInfo[] Levels { get; set; }
    }

    public class LevelInfo
    {
        public string Action { get; set; }

        public bool Criteria { get; set; }
    }
}
