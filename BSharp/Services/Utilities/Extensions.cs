using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data.Model;
using BSharp.Services.MultiTenancy;
using BSharp.Services.OData;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BSharp.Services.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Checks whether a certain type has a certain property name defined
        /// </summary>
        public static bool HasProperty(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName) != null;
        }

        /// <summary>
        /// Retrieves the username of the authenticated claims principal
        /// </summary>
        public static string ExternalUserId(this ClaimsPrincipal user)
        {
            string externalId = user.FindFirstValue(JwtClaimTypes.Subject);
            return externalId;
        }

        /// <summary>
        /// Retrieves the email of the authenticated claims principal
        /// </summary>
        public static string Email(this ClaimsPrincipal user)
        {
            string email = user.FindFirstValue(JwtClaimTypes.Email);
            return email;
        }

        /// <summary>
        /// Returns the internal user Id of the currently signed in user according to the tenant database
        /// </summary>
        public static int UserId(this ITenantUserInfoAccessor @this)
        {
            return @this.GetCurrentInfo().UserId.Value;
        }

        /// <summary>
        /// Extracts all errors inside an IdentityResult and concatenates them together, 
        /// falling back to a default message if no errors were found in the IdentityResult object
        /// </summary>
        public static string ErrorMessage(this IdentityResult result, string defaultMessage)
        {
            string errorMessage = defaultMessage;
            if (result.Errors.Any())
                errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));

            return errorMessage;
        }

        /// <summary>
        /// Creates a dictionary that maps each entity to its index in the list,
        /// this is a much faster alternative to <see cref="List{T}.IndexOf(T)"/>
        /// if it is expected that it will be performed N times, since it performs 
        /// a linear search resulting in O(N^2) complexity
        /// </summary>
        public static Dictionary<T, int> ToIndexDictionary<T>(this List<T> @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var result = new Dictionary<T, int>(@this.Count);
            for (int i = 0; i < @this.Count; i++)
            {
                var entity = @this[i];
                result[entity] = i;
            }

            return result;
        }

        public static void TrimStringProperties(this DtoBase entity)
        {
            var dtoType = entity.GetType();
            foreach (var prop in dtoType.GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    var originalValue = prop.GetValue(entity)?.ToString();
                    if (originalValue != null)
                    {
                        var trimmed = originalValue.Trim();
                        prop.SetValue(entity, trimmed);
                    }
                }
                else if (prop.PropertyType.IsSubclassOf(typeof(DtoBase)))
                {
                    var dtoForSave = prop.GetValue(entity);
                    if (dtoForSave != null)
                    {
                        (dtoForSave as DtoBase).TrimStringProperties();
                    }
                }
                else
                {
                    var propType = prop.PropertyType;
                    var isDtoList = propType.IsList() &&
                        propType.GenericTypeArguments[0].IsSubclassOf(typeof(DtoBase));

                    if (isDtoList)
                    {
                        var dtoList = prop.GetValue(entity);
                        if (dtoList != null)
                        {
                            foreach (var dto in dtoList.Enumerate<DtoBase>())
                            {
                                dto.TrimStringProperties();
                            }
                        }
                    }
                }
            }
        }

        public static bool IsList(this Type @this)
        {
            return @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(List<>);
        }

        /// <summary>
        /// Returns true if the property name is "Parent" and there is another
        /// property on the same type called "Node" with a type of HierarchyId
        /// </summary>
        public static bool IsParent(this PropertyInfo @this)
        {
            return @this.Name == "Parent" && @this.DeclaringType.GetProperty("Node")?.PropertyType == typeof(HierarchyId);
        }

        public static object[] ToFormatArguments(this SqlValidationResult @this)
        {
            /// <summary>
            /// SQL validation may return error message names (for localization) as well as some arguments 
            /// this method parses those arguments into objects based on their prefix for example date:2019-01-13
            /// will be parsed to datetime object suitable for formatting in C# into the error message
            /// </summary>
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
        /// Determines whether this type is <see cref="DateTime"/> or a 
        /// <see cref="DateTimeOffset"/> or a nullable version thereof
        /// </summary>
        public static bool IsDateOrTime(this Type @this)
        {
            var t = Nullable.GetUnderlyingType(@this) ?? @this;
            return t == typeof(DateTime) || t == typeof(DateTimeOffset);
        }

        /// <summary>
        /// Determines whether this type is a
        /// <see cref="DateTimeOffset"/> or a nullable version thereof
        /// </summary>
        public static bool IsDateTimeOffset(this Type @this)
        {
            var t = Nullable.GetUnderlyingType(@this) ?? @this;
            return t == typeof(DateTimeOffset);
        }

        /// <summary>
        /// Removes any trailing slashes from the specified string
        /// </summary>
        public static string WithoutTrailingSlash(this string str)
        {
            if (str == null)
            {
                return null;
            }

            while (str.EndsWith('/'))
            {
                str = str.Substring(0, str.Length - 1);
            }

            return str;
        }

        /// <summary>
        /// Adds one trailing slash to the specified string if one is not already there
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string WithTrailingSlash(this string str)
        {
            if (str == null)
            {
                return null;
            }

            if (!str.EndsWith("/"))
            {
                str = str + "/";
            }

            return str;
        }

        public static MethodCallExpression Contains(this Expression @this, ConstantExpression constant)
        {
            return Expression.Call(
                instance: @this,
                methodName: nameof(string.Contains),
                typeArguments: null,
                arguments: new[] { constant }
                );
        }

        /// <summary>
        /// Attempts to intelligently parse an object (that comes from an imported file) to a DateTime
        /// </summary>
        public static DateTime? ParseToDateTime(this object @this)
        {
            if (@this == null)
            {
                return null;
            }

            DateTime dateTime;

            if (@this.GetType() == typeof(double))
            {
                // Double indicates the OLE Automation date typically represented in excel
                dateTime = DateTime.FromOADate((double)@this);
            }
            else
            {
                // Parse the import value into a DateTime
                var valueString = @this.ToString();
                dateTime = DateTime.Parse(valueString);
            }

            return dateTime;
        }

        public static DateTimeOffset? AddTimeZone(this DateTime? dateTime, TimeZoneInfo timeZone)
        {
            if (dateTime == null)
            {
                return null;
            }

            // The date time supplied in the import does not contain time zone offset
            // The code below adds the current user time zone to the date time supplied
            var offset = timeZone.GetUtcOffset(DateTimeOffset.Now);
            var dtOffset = new DateTimeOffset(dateTime.Value, offset);

            return dtOffset;
        }

        /// <summary>
        /// The default Convert.ChangeType cannot handle converting types to
        /// nullable types also it cannot handle DateTimeOffset
        /// this method overcomes these limitations, credit: https://bit.ly/2DgqJmL
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="conversion"></param>
        /// <param name="sourceTimeZone"></param>
        /// <returns></returns>
        public static object ChangeType(this object obj, Type conversion, TimeZoneInfo sourceTimeZone = null)
        {
            var t = conversion;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (obj == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            if (t.IsDateOrTime())
            {
                var date = obj.ParseToDateTime();
                if (t.IsDateTimeOffset())
                {
                    if(sourceTimeZone != null)
                    {
                        return date.AddTimeZone(sourceTimeZone);
                    }
                    else
                    {
                        return date.AddTimeZone(TimeZoneInfo.Utc);
                    }
                }

                return date;
            }

            if (t == typeof(HierarchyId))
            {
                return obj.ToString();
            }

            return Convert.ChangeType(obj, t);
        }

        /// <summary>
        /// Determines whether this type is a List
        /// </summary>
        public static bool IsCollection(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>);
        }

        /// <summary>
        /// Determines whether this type is a collection
        /// </summary>
        public static Type CollectionType(this Type type)
        {
            return type.GenericTypeArguments.Length > 0 ? type.GenericTypeArguments[0] : null;
        }

        /// <summary>
        /// Retrieves all the properties that are adorned with <see cref="BasicFieldAttribute"/>s
        /// </summary>
        public static IEnumerable<PropertyInfo> BasicFields(this Type dtoType)
        {
            foreach (var prop in dtoType.GetProperties())
            {
                if (prop.GetCustomAttribute<BasicFieldAttribute>() != null)
                {
                    yield return prop;
                }
            }
        }

        /// <summary>
        /// Determines whether this property is adorned with <see cref="NavigationPropertyAttribute"/> which indicates
        /// that this is a navigation property to another collection
        /// </summary>
        public static bool IsNavigationField(this PropertyInfo prop)
        {
            return prop.GetCustomAttribute<NavigationPropertyAttribute>() != null;
        }

        /// <summary>
        /// Indicates whether the property is adorned with the <see cref="ForeignKeyAttribute"/> which means that
        /// this is a foreign key and it has an associated navigation property
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool IsForeignKey(this PropertyInfo prop)
        {
            return prop.GetCustomAttribute<ForeignKeyAttribute>() != null;
        }

        /// <summary>
        /// Determines if the property is adorned with the <see cref="IgnoreInMetadataAttribute"/> which indicates that 
        /// the property should not be included in the DTO metadata
        /// </summary>
        public static bool IsIgnored(this PropertyInfo prop)
        {
            return prop.GetCustomAttribute<IgnoreInMetadataAttribute>() != null;
        }

        /// <summary>
        /// Useful for reflection, allows you to iterate over a collection that is typed as an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<T> Enumerate<T>(this object collection)
        {
            foreach(var item in collection.Enumerate())
            {
                yield return (T)item;
            }
        }

        public static IEnumerable<object> Enumerate(this object collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            var enumerator = collection.GetType().GetMethod("GetEnumerator").Invoke(collection, new object[0]);
            var moveNextMethod = enumerator.GetType().GetMethod("MoveNext");
            var currentProp = enumerator.GetType().GetProperty("Current");
            while ((bool)moveNextMethod.Invoke(enumerator, new object[0]))
            {
                var item = currentProp.GetValue(enumerator);
                yield return item;
            }
        }

        public static async Task<T> ExecuteScalarSqlCommandAsync<T>(this DatabaseFacade db, string sql, params SqlParameter[] ps)
        {
            DbConnection connection = db.GetDbConnection();
            bool ownsConnection = false;
            T result;

            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;

                foreach(var p in ps)
                {
                    cmd.Parameters.Add(p);
                }

                if(connection.State != System.Data.ConnectionState.Open)
                {
                    ownsConnection = true;
                    connection.Open();
                }

                var dbValue = await cmd.ExecuteScalarAsync();

                if(dbValue == DBNull.Value)
                {
                    result = default(T);
                }
                else
                {
                    result = (T)dbValue;
                }
            }

            if(ownsConnection)
            {
                connection.Close();
            }

            return result;
        }
    }
}
