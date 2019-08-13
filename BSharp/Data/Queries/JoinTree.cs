using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// A data structure that represents a collection of paths (Where none of the steps are navigation collections),
    /// the structure maps every path to a symbol, a type and a foreign key name. And provides a facility to transform 
    /// the tree into an SQL JOIN expression.
    /// IMPORTANT: This class is used internally in <see cref="QueryInternal"/> and <see cref="AggregateQueryInternal"/>
    /// and is not to be used directly anywhere else in the solution
    /// </summary>
    internal class JoinTree : Dictionary<string, JoinTree>
    {
        /// <summary>
        /// Creates a new <see cref="JoinTree"/>
        /// </summary>
        /// <param name="type">The root <see cref="Entity"/> type of this join tree</param>
        /// <param name="foreignKeyName">Optionally: the foreign key pointing to the parent join tree</param>
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

        /// <summary>
        /// Gets the child tree reachable through the provided path
        /// </summary>
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

        /// <summary>
        /// Transforms the <see cref="JoinTree"/> into an SQL JOIN clause. For example: <c>FROM [dbo].[Table1] AS [P] LEFT JOIN [dbo].[Table2] AS [P1] ON [P].[Table2Id] = [P2].[Id]</c>
        /// </summary>
        public string GetSql(Func<Type, string> sources, string parentSymbol = null)
        {
            if (sources == null)
            {
                // Developer mistake
                throw new ArgumentNullException(nameof(sources));
            }

            string source = sources(Type);
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

        /// <summary>
        /// Creates a <see cref="JoinTree"/> from a list of paths
        /// </summary>
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
