using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BSharp.Services.OData
{
    /// <summary>
    /// A data structure that represents a collection of OData paths (Where none of the steps are navigation lists),
    /// the structure maps every path to a symbol, a type and a foreign key name. And provides a facility to transform 
    /// the tree into an SQL JOIN expression
    /// </summary>
    public class JoinTree : Dictionary<string, JoinTree>
    {
        public JoinTree(Type type, string foreignKeyName = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            ForeignKeyName = foreignKeyName;
        }

        /// <summary>
        /// The DTO type of the current node
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// The foreign key on the *parent* DTO
        /// </summary>
        public string ForeignKeyName { get; private set; } // e.g. 'AgentId'

        /// <summary>
        /// The symbol of the path leading up to the current node, root node usually has the symbol "P"
        /// </summary>
        public string Symbol { get; private set; } // e.g. 'P1', 'P2'

        public JoinTree this[ArraySegment<string> path]
        {
            get
            {
                JoinTree current = this;
                for (int i = 0; i < path.Count; i++)
                {
                    var step = path[i];
                    if (!current.ContainsKey(step))
                    {
                        return null;
                    }
                    else
                    {
                        current = current[step];
                    }
                }

                return current;
            }
        }

        public string GetSql(Func<Type, string> sources, string parentSymbol = null)
        {
            if (sources == null)
            {
                // Developer mistake
                throw new ArgumentNullException(nameof(sources));
            }

            var source = sources(Type);
            if (string.IsNullOrWhiteSpace(source))
            {
                // Developer mistake
                throw new InvalidOperationException($"Type {Type.Name} does not have a valid source string");
            }

            StringBuilder builder = new StringBuilder();
            if (parentSymbol == null)
            {
                if (Symbol == null)
                {
                    // Only once, the root node initializes the symbols of the entire tree
                    InitializeSymbols();
                }

                builder.Append($"FROM {source} As [{Symbol}]");
            }
            else
            {
                builder.AppendLine();
                builder.Append($"LEFT JOIN {source} As [{Symbol}] ON [{parentSymbol}].[{ForeignKeyName}] = [{Symbol}].[Id]");
            }

            foreach (var key in Keys)
            {
                builder.Append(this[key].GetSql(sources, Symbol));
            }

            return builder.ToString();
        }

        private int InitializeSymbols(int id = 0)
        {
            if (id == 0)
            {
                Symbol = "P";
            }
            else
            {
                Symbol = $"P{id}";
            }

            foreach (var key in Keys)
            {
                id++;
                id = this[key].InitializeSymbols(id);
            }

            return id;
        }

        public static JoinTree Make(Type type, IEnumerable<string[]> paths)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            var result = new JoinTree(type, foreignKeyName: null);
            foreach (var path in paths)
            {
                var currentType = type;
                var currentTree = result;
                foreach (var step in path)
                {
                    var prop = currentType.GetProperty(step);
                    if (prop == null)
                    {
                        // Programmer mistake
                        throw new InvalidOperationException($"Property '{step}' does not exist on type {currentType.Name}");
                    }

                    if (!currentTree.ContainsKey(step))
                    {
                        string foreignKeyName = prop.GetCustomAttribute<NavigationPropertyAttribute>()?.ForeignKey;
                        if (string.IsNullOrWhiteSpace(foreignKeyName))
                        {
                            // Programmer mistake
                            throw new InvalidOperationException($"Navigation property '{step}' on type {currentType.Name} is not adorned with the name of the foreign key property");
                        }

                        currentTree[step] = new JoinTree(prop.PropertyType, foreignKeyName: foreignKeyName);
                    }

                    currentType = prop.PropertyType;
                    currentTree = currentTree[step];
                }
            }

            result.InitializeSymbols();
            return result;
        }
    }
}
