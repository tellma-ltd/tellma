using BSharp.EntityModel;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        /// <summary>
        /// Traverses the <see cref="Entity"/> tree trimming all string properties
        /// or setting them to null if they are just empty spaces
        /// </summary>
        public static void TrimStringProperties(this Entity entity)
        {
            var dtoType = entity.GetType();
            foreach (var prop in dtoType.GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    var originalValue = prop.GetValue(entity)?.ToString();
                    if(string.IsNullOrWhiteSpace(originalValue))
                    {
                        // No empty strings or white spaces allowed
                        prop.SetValue(entity, null);
                    }
                    else
                    {
                        // Trim
                        var trimmed = originalValue.Trim();
                        prop.SetValue(entity, trimmed);
                    }
                }
                else if (prop.PropertyType.IsEntity())
                {
                    var dtoForSave = prop.GetValue(entity);
                    if (dtoForSave != null)
                    {
                        (dtoForSave as Entity).TrimStringProperties();
                    }
                }
                else
                {
                    var propType = prop.PropertyType;
                    var isDtoList = propType.IsList() &&
                        propType.GenericTypeArguments[0].IsEntity();

                    if (isDtoList)
                    {
                        var dtoList = prop.GetValue(entity);
                        if (dtoList != null)
                        {
                            foreach (var row in dtoList.Enumerate<Entity>())
                            {
                                row.TrimStringProperties();
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
                str += "/";
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
                    if (sourceTimeZone != null)
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
        /// Retrieves all the properties that are adorned with the <see cref="AlwaysAccessibleAttribute"/>s
        /// </summary>
        public static IEnumerable<PropertyInfo> AlwaysAccessibleFields(this Type dtoType)
        {
            foreach (var prop in dtoType.GetProperties())
            {
                if (prop.GetCustomAttribute<AlwaysAccessibleAttribute>() != null)
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
            return prop.GetCustomAttribute<ForeignKeyAttribute>() != null;
        }

        /// <summary>
        /// Useful for reflection, allows you to iterate over a collection that is typed as an object
        /// </summary>
        /// <typeparam name="T">The type of the resulting <see cref="IEnumerable{T}"/></typeparam>
        /// <param name="collection">A collection object typically retrieved via reflection</param>
        public static IEnumerable<T> Enumerate<T>(this object collection)
        {
            foreach (var item in collection.Enumerate())
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

        public static bool IsPrefixOf(this string[] potentialPrefix, ArraySegment<string> segment)
        {
            if (potentialPrefix.Length > segment.Count)
            {
                return false;
            }

            for (int i = 0; i < potentialPrefix.Length; i++)
            {
                if (potentialPrefix[i] != segment[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if this type derives from <see cref="Entity"/>
        /// </summary>
        public static bool IsEntity(this Type type)
        {
            return type.IsSubclassOf(typeof(Entity));
        }

        /// <summary>
        /// Checks if this type is adorned with <see cref="StrongEntityAttribute"/>
        /// </summary>
        public static bool IsStrongEntity(this Type type)
        {
            return type.GetCustomAttribute<StrongEntityAttribute>() != null;
        }

        /// <summary>
        /// Returns the root type which corresponds to the SQL table of this type
        /// </summary>
        public static Type GetRootType(this Type type)
        {
            return type.GetCustomAttribute<StrongEntityAttribute>()?.Type ?? type;
        }
    }
}
