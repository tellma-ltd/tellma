using System;
using System.Collections.Generic;
using System.Text;
using Tellma.Model.Common;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// A data structure that represents a collection of paths (Where none of the steps are navigation collections),
    /// the structure maps every path to a symbol, a type and a foreign key name. And provides a facility to transform 
    /// the tree into an SQL JOIN expression.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: This class is used internally in <see cref="EntityQueryInternal"/> and <see cref="AggregateQueryInternal"/>
    /// and is not to be used directly anywhere else in the solution.
    /// </remarks>
    public class JoinTrie : Dictionary<string, JoinTrie>
    {
        /// <summary>
        /// Creates a new <see cref="JoinTrie"/>
        /// </summary>
        /// <param name="entityDescriptor">The <see cref="EntityDescriptor"/> of the root type of this join tree</param>
        /// <param name="foreignKeyName">Optionally: the foreign key pointing to the parent join tree</param>
        public JoinTrie(TypeDescriptor entityDescriptor, string foreignKeyName = null)
        {
            EntityDescriptor = entityDescriptor ?? throw new ArgumentNullException(nameof(entityDescriptor));
            ForeignKeyName = foreignKeyName;
        }

        /// <summary>
        /// The <see cref="TypeDescriptor"/> of the current node.
        /// </summary>
        public TypeDescriptor EntityDescriptor { get; private set; }

        /// <summary>
        /// The foreign key on the *parent* Entity.
        /// </summary>
        public string ForeignKeyName { get; } // e.g. 'RelationId'

        /// <summary>
        /// The symbol of the path leading up to the current node, root node usually has the symbol "P"
        /// </summary>
        public string Symbol { get; private set; } // e.g. 'P1', 'P2'

        /// <summary>
        /// Gets the child tree reachable through the provided path
        /// </summary>
        public JoinTrie this[ArraySegment<string> path]
        {
            get
            {
                JoinTrie current = this;
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
        /// Transforms the <see cref="JoinTrie"/> into an SQL JOIN clause. For example: <c>FROM [dbo].[Table1] AS [P] LEFT JOIN [dbo].[Table2] AS [P1] ON [P].[Table2Id] = [P2].[Id]</c>
        /// </summary>
        public string GetSql(Func<Type, string> sources, string parentSymbol = null)
        {
            if (sources == null)
            {
                // Developer mistake
                throw new ArgumentNullException(nameof(sources));
            }

            var builder = new StringBuilder();
            bool isRoot = parentSymbol == null;
            if (isRoot)
            {
                if (Symbol == null)
                {
                    // Only once, the root node initializes the symbols of the entire tree
                    InitializeSymbols();
                }

                string source = sources(EntityDescriptor.Type);
                builder.Append($"FROM {source} As [{Symbol}]");
            }
            else
            {
                string source = sources(EntityDescriptor.Type);
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
        /// Creates a <see cref="JoinTrie"/> from a list of paths
        /// </summary>
        public static JoinTrie Make(TypeDescriptor rootDesc, IEnumerable<string[]> paths)
        {
            if (rootDesc == null)
            {
                throw new ArgumentNullException(nameof(rootDesc));
            }

            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            var result = new JoinTrie(rootDesc, foreignKeyName: null);
            foreach (var path in paths)
            {
                var currentDesc = rootDesc;
                var currentTree = result;
                foreach (var step in path)
                {
                    var navProp = currentDesc.NavigationProperty(step);
                    if (navProp == null)
                    {
                        // Programmer mistake
                        throw new InvalidOperationException($"Navigation property '{step}' does not exist on type {currentDesc.Name}");
                    }

                    if (navProp.ForeignKey == null)
                    {
                        throw new InvalidOperationException($"Navigation property '{step}' on type {currentDesc.Name} is missing its foreign key");
                    }

                    if (!currentTree.ContainsKey(step))
                    {
                        string foreignKeyName = navProp.ForeignKey.Name;

                        currentTree[step] = new JoinTrie(navProp.TypeDescriptor, foreignKeyName: foreignKeyName);
                    }

                    currentTree = currentTree[step];
                    currentDesc = currentTree.EntityDescriptor;
                }
            }

            result.InitializeSymbols();
            return result;
        }
    }
}
