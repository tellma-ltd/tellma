using Tellma.Entities;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Collections;
using System.Threading.Tasks;

namespace Tellma.Services.Utilities
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
        /// Checks whether a certain type has a certain property name defined
        /// </summary>
        public static bool IsSameOrSubClassOf<TParent>(this Type child)
        {
            return typeof(TParent) == child || child.IsSubclassOf(typeof(TParent));
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
        /// or setting them to null if they are just empty spaces.
        /// This function cannot handle cyclic entity graphs
        /// </summary>
        public static void TrimStringProperties(this Entity entity)
        {
            if (entity == null)
            {
                // Nothing to do
                return;
            }

            // Inner recursive method that does the trimming on the entire tree
            static void TrimStringPropertiesInner(Entity entity, Entities.Descriptors.TypeDescriptor typeDesc)
            {
                // Trim all string properties
                foreach (var prop in typeDesc.SimpleProperties.Where(p => p.Type == typeof(string)))
                {
                    var originalValue = prop.GetValue(entity)?.ToString();
                    if (string.IsNullOrWhiteSpace(originalValue))
                    {
                        // No empty strings or white spaces allowed
                        prop.SetValue(entity, null);
                    }
                    else
                    {
                        // Trim
                        var trimmedValue = originalValue.Trim();
                        prop.SetValue(entity, trimmedValue);
                    }
                }

                // Recursively do nav properties
                foreach (var prop in typeDesc.NavigationProperties)
                {
                    if (prop.GetValue(entity) is Entity relatedEntity)
                    {
                        TrimStringPropertiesInner(relatedEntity, prop.TypeDescriptor);
                    }
                }

                // Recursively do the collection properties
                foreach (var prop in typeDesc.CollectionProperties)
                {
                    var collectionTypeDesc = prop.CollectionTypeDescriptor;
                    if (prop.GetValue(entity) is IList collection)
                    {
                        foreach (var obj in collection)
                        {
                            if (obj is Entity relatedEntity)
                            {
                                TrimStringPropertiesInner(relatedEntity, collectionTypeDesc);
                            }
                        }
                    }
                }
            }

            // Trim and return
            var typeDesc = Entities.Descriptors.TypeDescriptor.Get(entity.GetType());
            TrimStringPropertiesInner(entity, typeDesc);
        }

        public static bool IsList(this Type @this)
        {
            return @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(List<>);
        }

        /// <summary>
        /// Removes any trailing slashes from the specified string
        /// </summary>
        public static string WithoutTrailingSlash(this string str)
        {
            if (str is null)
            {
                return null;
            }

            while (str.EndsWith('/'))
            {
                str = str[0..^1];
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
            if (str is null)
            {
                return null;
            }

            if (!str.EndsWith("/"))
            {
                str += "/";
            }

            return str;
        }

        /// <summary>
        /// If the string is enclosed in one or more bracket pairs, this function strips away those brackets.
        /// Note: "(A) (B)" is not enclosed in a bracket pair
        /// </summary>
        public static string DeBracket(this string str)
        {
            int level = 0;
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i] == '(')
                {
                    level++;
                }

                if (str[i] == ')')
                {
                    level--;
                }

                if (level == 0)
                {
                    // There are no enclosing brackets
                    return str;
                }
            }

            if (str.EndsWith(')'))
            {
                // Remove the brackets and call recursively
                str = str[1..^1].DeBracket();
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

        public static void Set(this IHeaderDictionary dic, string key, StringValues value)
        {
            if (dic is null)
            {
                throw new ArgumentNullException(nameof(dic));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("message", nameof(key));
            }

            dic.Remove(key);
            dic.Add(key, value);
        }

        /// <summary>
        /// Removes all characters after a certain length
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxLength, bool appendEllipses = false)
        {
            const string ellipses = "...";

            if (maxLength < 0)
            {
                return value;
            }
            else if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            else if (value.Length <= maxLength)
            {
                return value;
            }
            else
            {
                var truncated = value.Substring(0, maxLength);
                if (appendEllipses)
                {
                    truncated += ellipses;
                }

                return truncated;
            }
        }
    }
}
