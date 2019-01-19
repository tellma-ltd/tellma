using BSharp.Controllers.DTO;
using BSharp.Data.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

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
        public static string UserId(this ClaimsPrincipal user)
        {
            return "4F7785F2-5942-4CFB-B5AD-85AB72F7EB35"; // TODO
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

        public static void TrimStringProperties(this DtoForSaveBase entity)
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
                else if (prop.PropertyType.IsSubclassOf(typeof(DtoForSaveBase)))
                {
                    var dtoForSave = prop.GetValue(entity);
                    if (dtoForSave != null)
                    {
                        (dtoForSave as DtoForSaveBase).TrimStringProperties();
                    }
                }
                else
                {
                    var isDtoList = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                    if (isDtoList)
                    {
                        // TODO trim all children in a navigation collection
                        throw new NotImplementedException("Trimming navigation collection is not implemented yet");
                    }
                }
            }
        }

        /// <summary>
        /// Returns only the part of the stream that is not enclosed inside brackets,
        /// And for everything else returns null
        /// </summary>
        public static IEnumerable<string> OutsideBrackets(this IEnumerable<string> @this)
        {
            int level = 0;
            foreach (var item in @this)
            {
                if (item == "(")
                {
                    level++;
                }

                if (item == ")")
                {
                    level--;
                }

                if (level <= 0)
                {
                    yield return item;
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Checks that the first token is an opening bracket, the last token is a closing bracket
        /// and that they are matched pair, for example: this will return false '(A and B) or (C and D)'
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsEnclosedInPairBrackets(this IEnumerable<string> @this)
        {
            // Check that the first token is opening bracket
            if (@this.FirstOrDefault() != "(")
            {
                return false;
            }

            // Check that the last token is closing bracket
            if (@this.LastOrDefault() != ")")
            {
                return false;
            }

            // Check that the first and last brackets are a pair
            var inner = @this.Skip(1).Take(@this.Count() - 2);
            int level = 1;
            foreach (var item in inner)
            {
                if (item == "(")
                {
                    level++;
                }

                if (item == ")")
                {
                    level--;
                }

                if (level == 0)
                {
                    // This means that the first brackte and last bracket are not a pair
                    // For example '(A and B) or (C and D)'
                    return false;
                }
            }

            return true;
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
    }
}
