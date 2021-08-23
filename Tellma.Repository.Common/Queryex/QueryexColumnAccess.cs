using System;
using System.Linq;
using Tellma.Model.Common;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Represents an expression that refers to an entity column.
    /// <para/>
    /// Examples:<br/> 
    /// - Name<br/>
    /// - Resource.Lookup1.Code<br/>
    /// - Account.Center
    /// </summary>
    public class QueryexColumnAccess : QueryexBase
    {
        public QueryexColumnAccess(string[] path, string prop)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Property = prop;
        }

        /// <summary>
        /// An path, ie string array, of navigation property names, each a step leading
        /// up to the entity containing the accessed column. 
        /// <para/>
        /// Example: ["Account", "Center"].
        /// </summary>
        public string[] Path { get; }

        /// <summary>
        /// The name of the accessed column.
        /// <para/>
        /// Example: "Name"
        /// </summary>
        public string Property { get; }

        public override string ToString()
        {
            var path = string.Join(".", Path) ?? "";
            var prop = Property ?? "";
            var dot = string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(prop) ? "" : ".";

            return $"{path}{dot}{prop}";
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(Property))
            {
                // Developer mistake
                throw new InvalidOperationException($"Bug: Invoking {nameof(CompileNative)} on a {nameof(QueryexColumnAccess)} that does not have a property.");
            }

            var propName = Property;

            // (A) Calculate Nullity (the entire path foreign keys + the final property must be all NOT NULL)
            bool pathNotNull = true;
            var join = ctx.Joins;
            foreach (var step in Path)
            {
                var navPropDesc = join.EntityDescriptor.NavigationProperty(step);
                pathNotNull = pathNotNull && navPropDesc.ForeignKey.IsNotNull;
                join = join[step];
            }

            var propDesc = join.EntityDescriptor.Property(propName);
            if (propDesc == null)
            {
                throw new QueryException($"Property '{propName}' does not exist on type {join.EntityDescriptor.Name}.");
            }

            QxNullity nullity = pathNotNull && propDesc.IsNotNull ? QxNullity.NotNull : QxNullity.Nullable;

            // (B) Calculate the type
            QxType type;
            var propType = Nullable.GetUnderlyingType(propDesc.Type) ?? propDesc.Type;
            switch (propType.Name)
            {
                case nameof(Char):
                case nameof(String):
                    type = QxType.String;
                    break;

                case nameof(Byte):
                case nameof(SByte):
                case nameof(Int16):
                case nameof(UInt16):
                case nameof(Int32):
                case nameof(UInt32):
                case nameof(Int64):
                case nameof(UInt64):
                case nameof(Single):
                case nameof(Double):
                case nameof(Decimal):
                    type = QxType.Numeric;
                    break;

                case nameof(Boolean):
                    type = QxType.Bit;
                    break;

                case nameof(DateTime):
                    type = propDesc.IncludesTime ? QxType.DateTime : QxType.Date;
                    break;

                case nameof(DateTimeOffset):
                    type = QxType.DateTimeOffset;
                    break;

                case nameof(HierarchyId):
                    type = QxType.HierarchyId;
                    break;

                case nameof(Geography):
                    type = QxType.Geography;
                    break;

                default:
                    if (propDesc is NavigationPropertyDescriptor || propDesc is CollectionPropertyDescriptor)
                    {
                        throw new QueryException($"A column access cannot terminate with a navigation property like {propDesc.Name}.");
                    }
                    else
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"[Bug] Could not map type {propType.Name} to a {nameof(QxType)}"); // Future proofing
                    }
            }

            // (C) Calculate the SQL
            var sql = $"[{join.Symbol}].[{propName}]";

            // Return the result
            return (sql, type, nullity);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexColumnAccess ca &&
                ca.Property == Property &&
                ca.Path.Length == Path.Length &&
                Enumerable.Range(0, Path.Length)
                    .All(i => ca.Path[i] == Path[i]);
        }

        public override int GetHashCode()
        {
            int propCode = Property?.GetHashCode() ?? 0;

            return Path.Select(s => s.GetHashCode())
                .Aggregate(propCode, (code1, code2) => code1 ^ code2);
        }

        public override QueryexBase Clone(string[] prefix = null)
        {
            if (prefix == null || prefix.Length == 0 || !PathStartsWith(prefix))
            {
                return new QueryexColumnAccess(Path[..], Property);
            }
            else
            {
                return new QueryexColumnAccess(Path[prefix.Length..], Property);
            }
        }

        /// <summary>
        /// Helper function to check if this <see cref="QueryexColumnAccess"/> starts with the same steps of <paramref name="prefix"/>.
        /// </summary>
        public bool PathStartsWith(string[] prefix)
        {
            return prefix != null && prefix.Length <= Path.Length &&
                Enumerable.Range(0, prefix.Length).All(i => prefix[i] == Path[i]);
        }

        /// <summary>
        /// Helper function to check if the given path contains the same steps as the path of this <see cref="QueryexColumnAccess"/>.
        /// </summary>
        public bool PathEquals(string[] path)
        {
            return path != null && path.Length == Path.Length &&
                Enumerable.Range(0, path.Length).All(i => path[i] == Path[i]);
        }

        #region Column Access Validation

        /// <summary>
        /// First character of a column access must be a letter.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <returns>True if the first character of the column access is valid according to the condition above, false otherwise.</returns>
        public static bool ProperFirstChar(string token)
        {
            return !string.IsNullOrEmpty(token) && char.IsLetter(token[0]);
        }

        /// <summary>
        /// All characters of a column access must be letters, numbers, underscores or periods.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <returns>True if the characters of the column access are valid according to the condition above, false otherwise.</returns>
        public static bool ProperChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }

        /// <summary>
        /// All characters of a column access must be letters, numbers, underscores or dots.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the characters of the column access are valid according to the condition above, false otherwise.</returns>
        public static bool ProperChars(string token)
        {
            return !string.IsNullOrEmpty(token) && token.All(ProperChar);
        }

        /// <summary>
        /// The column access must not be one of the reserved keywords
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>False if the column access is one of the keywords, true otherwise.</returns>
        public static bool NotReservedKeyword(string token)
        {
            return token.ToLower() switch
            {
                "null" or "true" or "false" or "asc" or "desc" => false,
                _ => true,
            };
        }

        /// <summary>
        /// Validates the column access against all the rules: <see cref="ProperFirstChar(string)"/>,
        /// <see cref="ProperChars(string)"/> and <see cref="NotReservedKeyword(string)"/>.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <returns>True if it passes all the validation rules, false otherwise.</returns>
        public static bool IsValidColumnAccess(string token, bool expectPathsOnly, out string[] pathArray, out string propName)
        {
            bool match = ProperFirstChar(token) && ProperChars(token) && NotReservedKeyword(token);
            if (match)
            {
                var steps = token
                    .Split('.')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e));

                if (expectPathsOnly)
                {
                    pathArray = steps.ToArray();
                    propName = null;
                }
                else
                {
                    pathArray = steps.SkipLast(1).ToArray();
                    propName = steps.Last();
                }
            }
            else
            {
                pathArray = null;
                propName = null;
            }

            return match;
        }

        #endregion
    }
}
