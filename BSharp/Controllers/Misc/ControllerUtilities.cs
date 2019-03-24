using BSharp.Controllers.DTO;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public static async Task<IEnumerable<AbstractPermission>> GetPermissions(DbQuery<AbstractPermission> q, PermissionLevel level, params string[] viewIds)
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

            // Prepare the WHERE clause that corresponds to the permission level
            string levelWhereClause;
            switch (level)
            {
                case PermissionLevel.Read:
                    levelWhereClause = $"E.Level LIKE '{Constants.Read}%' OR E.Level = '{Constants.Update}' OR E.Level = '{Constants.Sign}'";
                    break;
                case PermissionLevel.Update:
                    levelWhereClause = $"E.Level = '{Constants.Update}' OR E.Level = '{Constants.Sign}'";
                    break;
                case PermissionLevel.Create:
                    levelWhereClause = $"E.Level LIKE '%{Constants.Read}'";
                    break;
                case PermissionLevel.Sign:
                    levelWhereClause = $"E.Level = '{Constants.Sign}'";
                    break;
                default:
                    throw new Exception("Unhandled PermissionLevel enum value"); // Programmer mistake
            }

            // Retrieve the permissions
            // Note: There is another query similiar to this one in PermissionsController
            var result = await q.FromSql($@"
SELECT * FROM (
    SELECT ViewId, Criteria, Mask, Level 
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
    JOIN [dbo].[RoleMemberships] RM ON R.Id = RM.RoleId
    WHERE R.IsActive = 1 
    AND RM.UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId')) 
    AND P.ViewId IN (SELECT Code FROM @ViewIds)
    UNION
    SELECT ViewId, Criteria, Mask, Level 
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
    WHERE R.IsPublic = 1 
    AND R.IsActive = 1
    AND P.ViewId IN (SELECT Code FROM @ViewIds)
) AS E WHERE {levelWhereClause}
", viewIdsTvp).ToListAsync();

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

        //public static IQueryable<T> GetQueryFromIds<T, TKey>(IQueryable<T> baseQuery, string tableName, IEnumerable<TKey> ids) where T : DtoKeyBase<TKey>
        //{
        //    var idsString = string.Join(",", ids);
        //    var idType = Nullable.GetUnderlyingType(typeof(TKey)) ?? typeof(TKey);
        //    var select = idType == typeof(int) ? "CONVERT(INT, VALUE)" : "VALUE";
        //    var q = baseQuery.FromSql($"SELECT * FROM {tableName} WHERE Id IN (SELECT {select} AS Id FROM STRING_SPLIT({idsString}, ','))");

        //    return q;
        //}
    }
}
