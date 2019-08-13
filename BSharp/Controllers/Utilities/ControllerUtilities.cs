using BSharp.Controllers.Dto;
using BSharp.Data;
using BSharp.EntityModel;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    public static class ControllerUtilities
    {
        public const string ALL = "all";

        // TODO: Delete DataTable methods

        ///// <summary>
        ///// Constructs a SQL data table containing all the public properties of the 
        ///// entities' type and populates the data table with the provided entities
        ///// </summary>
        //public static DataTable DataTable<T>(IEnumerable<T> entities, bool addIndex = false)
        //{
        //    DataTable table = new DataTable();
        //    if (addIndex)
        //    {
        //        // The column order MUST match the column order in the user-defined table type
        //        table.Columns.Add(new DataColumn("Index", typeof(int)));
        //    }

        //    var props = GetPropertiesBaseFirst(typeof(T)).Where(e => !e.PropertyType.IsList() && e.Name != nameof(Entity.EntityMetadata));
        //    foreach (var prop in props)
        //    {
        //        var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        //        var column = new DataColumn(prop.Name, propType);
        //        if (propType == typeof(string))
        //        {
        //            // For string columns, it is more performant to explicitly specify the maximum column size
        //            // According to this article: http://www.dbdelta.com/sql-server-tvp-performance-gotchas/
        //            var stringLengthAttribute = prop.GetCustomAttribute<StringLengthAttribute>(inherit: true);
        //            if (stringLengthAttribute != null)
        //            {
        //                column.MaxLength = stringLengthAttribute.MaximumLength;
        //            }
        //        }

        //        table.Columns.Add(column);
        //    }

        //    int index = 0;
        //    foreach (var entity in entities)
        //    {
        //        DataRow row = table.NewRow();

        //        // We add an index property since SQL works with un-ordered sets
        //        if (addIndex)
        //        {
        //            row["Index"] = index++;
        //        }

        //        // Add the remaining properties
        //        foreach (var prop in props)
        //        {
        //            var propValue = prop.GetValue(entity);
        //            row[prop.Name] = propValue ?? DBNull.Value;
        //        }

        //        table.Rows.Add(row);
        //    }

        //    return table;
        //}

        ///// <summary>
        ///// Constructs a SQL data table containing all the entities in all the collections
        ///// and adds an index and a header index, this is useful for child collections that
        ///// are passed to SQL alongside their headers, the include predicate optionally filters
        ///// the items but keeps the original indexing
        ///// </summary>
        //public static DataTable DataTableWithHeaderIndex<T>(IEnumerable<(List<T> Items, int HeaderIndex)> collections, Predicate<T> include = null)
        //{
        //    include = include ?? (e => true);
        //    DataTable table = new DataTable();

        //    // The column order MUST match the column order in the user-defined table type
        //    table.Columns.Add(new DataColumn("Index", typeof(int)));
        //    table.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));

        //    var props = GetPropertiesBaseFirst(typeof(T)).Where(e => !e.PropertyType.IsList() && e.Name != nameof(DtoBase.EntityMetadata));
        //    foreach (var prop in props)
        //    {
        //        var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        //        var column = new DataColumn(prop.Name, propType);
        //        if (propType == typeof(string))
        //        {
        //            // For string columns, it is more performant to explicitly specify the maximum column size
        //            // According to this article: http://www.dbdelta.com/sql-server-tvp-performance-gotchas/
        //            var stringLengthAttribute = prop.GetCustomAttribute<StringLengthAttribute>(inherit: true);
        //            if (stringLengthAttribute != null)
        //            {
        //                column.MaxLength = stringLengthAttribute.MaximumLength;
        //            }
        //        }

        //        table.Columns.Add(column);
        //    }

        //    foreach (var (items, headerIndex) in collections)
        //    {
        //        int index = 0;
        //        foreach (var item in items)
        //        {
        //            if (include(item))
        //            {
        //                DataRow row = table.NewRow();

        //                // We add index and header index properties since SQL works with un-ordered sets
        //                row["Index"] = index;
        //                row["HeaderIndex"] = headerIndex;

        //                // Add the remaining properties
        //                foreach (var prop in props)
        //                {
        //                    var propValue = prop.GetValue(item);
        //                    row[prop.Name] = propValue ?? DBNull.Value;
        //                }

        //                table.Rows.Add(row);
        //            }

        //            index++;
        //        }
        //    }

        //    return table;
        //}

        ///// <summary>
        ///// This is alternative for <see cref="Type.GetProperties"/>
        ///// that returns base class properties before inherited class properties
        ///// Credit: https://bit.ly/2UGAkKj
        ///// </summary>
        //public static PropertyInfo[] GetPropertiesBaseFirst(Type type)
        //{
        //    var orderList = new List<Type>();
        //    var iteratingType = type;
        //    do
        //    {
        //        orderList.Insert(0, iteratingType);
        //        iteratingType = iteratingType.BaseType;
        //    } while (iteratingType != null);

        //    var props = type.GetProperties()
        //        .OrderBy(x => orderList.IndexOf(x.DeclaringType))
        //        .ToArray();

        //    return props;
        //}

        //        /// <summary>
        //        /// Passing the view Id "all" to this method will result in an Exception
        //        /// </summary>
        //        /// <returns>The list abstract user permissions that pertain to the current user and specified level and view Ids</returns>
        //        public static async Task<IEnumerable<AbstractPermission>> GetPermissions(DbQuery<AbstractPermission> q, string action, params string[] viewIds)
        //        {
        //            // Validate parameters
        //            if (q == null)
        //            {
        //                // Programmer mistake
        //                throw new ArgumentNullException(nameof(q));
        //            }

        //            if (viewIds == null)
        //            {
        //                // Programmer mistake
        //                throw new ArgumentNullException(nameof(viewIds));
        //            }

        //            if (viewIds.Any(e => e == ALL))
        //            {
        //                // Programmer mistake
        //                throw new BadRequestException("'GetPermissions' cannot handle the 'all' case");
        //            }

        //            // Add all and prepare the TVP
        //            viewIds = viewIds.Union(new[] { ALL }).ToArray();
        //            var viewIdsTable = DataTable(viewIds.Select(e => new { Code = e }));
        //            var viewIdsTvp = new SqlParameter("@ViewIds", viewIdsTable)
        //            {
        //                TypeName = $"dbo.CodeList",
        //                SqlDbType = SqlDbType.Structured
        //            };

        //            var actionParameter = new SqlParameter("Action", action);

        //            // Prepare the WHERE clause that corresponds to the permission level
        //            string levelWhereClause;
        //            switch (action)
        //            {
        //                case Constants.Read:
        //                    levelWhereClause = "";
        //                    break;

        //                default:
        //                    levelWhereClause = $"WHERE E.Action = @Action OR E.Action = 'All'";
        //                    break;
        //            }

        //            // Retrieve the permissions
        //            // Note: There is another query similiar to this one in PermissionsController
        //            var result = await q.FromSql($@"
        //SELECT * FROM (
        //    SELECT ViewId, Criteria, Mask, Level As Action 
        //    FROM [dbo].[Permissions] P
        //    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
        //    JOIN [dbo].[RoleMemberships] RM ON R.Id = RM.RoleId
        //    WHERE R.IsActive = 1 
        //    AND RM.UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId')) 
        //    AND P.ViewId IN (SELECT Code FROM @ViewIds)
        //    UNION
        //    SELECT ViewId, Criteria, Mask, Level As Action 
        //    FROM [dbo].[Permissions] P
        //    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
        //    WHERE R.IsPublic = 1 
        //    AND R.IsActive = 1
        //    AND P.ViewId IN (SELECT Code FROM @ViewIds)
        //) AS E {levelWhereClause}
        //", actionParameter, viewIdsTvp).ToListAsync();

        //            return result;
        //        }

        /// <summary>
        /// Calls the provided function and handles the special exceptions by turning them into <see cref="ActionResult"/>s.
        /// Action implementations can then throw these exceptions when there is an error, making the implementatio neasier
        /// </summary>
        public static async Task<ActionResult<T>> InvokeActionImpl<T>(Func<Task<ActionResult<T>>> func, ILogger logger)
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
            catch (NotFoundException<int> ex)
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
            catch (NotFoundException<int> ex)
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

        public static void ValidateUniqueIds<TEntity>(List<TEntity> entities, ModelStateDictionary modelState, IStringLocalizer localizer) where TEntity : EntityWithKey
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (modelState is null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            // Check that Ids are unique
            var duplicateIds = entities.Where(e => !e.GetId().Equals(0)).GroupBy(e => e.GetId()).Where(g => g.Count() > 1);
            if(duplicateIds.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<TEntity, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateIds in duplicateIds)
                {
                    foreach (var entity in groupWithDuplicateIds)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        modelState.AddModelError($"[{index}].Id", localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", entity.GetId()]);
                    }
                }
            }
        }

        /// <summary>
        /// SQL validation may return error message names (for localization) as well as some arguments 
        /// this method parses those arguments into objects based on their prefix for example date:2019-01-13
        /// will be parsed to datetime object suitable for formatting in C# into the error message
        /// </summary>
        public static object[] ToFormatArguments(this ValidationError @this)
        {
            object Parse(string str)
            {
                // TODO Implement properly
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

            object[] formatArguments = {
                    Parse(@this.Argument1),
                    Parse(@this.Argument2),
                    Parse(@this.Argument3),
                    Parse(@this.Argument4),
                    Parse(@this.Argument5)
                };

            return formatArguments;
        }

        /// <summary>
        /// The method localizes every error in the collection and adds it to the <see cref="ModelStateDictionary"/>
        /// </summary>
        public static void AddLocalizedErrors(this ModelStateDictionary modelState, IEnumerable<ValidationError> errors, IStringLocalizer localizer)
        {
            foreach (var error in errors)
            {
                var formatArguments = error.ToFormatArguments();

                string key = error.Key;
                string errorMessage = localizer[error.ErrorName, formatArguments];

                modelState.AddModelError(key: key, errorMessage: errorMessage);
            }
        }
    }
}
