using System;
using System.Collections.Generic;
using Tellma.Model.Common;
using Tellma.Repository.Common.Queryex;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Efficiently validates a collection of paths against a root type.
    /// </summary>
    public class PathValidator : Dictionary<string, PathValidator>
    {
        /// <summary>
        /// Adds a path to the <see cref="PathValidator"/> tree.
        /// </summary>
        public void AddPath(ArraySegment<string> path, string property = null)
        {
            // Add the path if it has any steps
            var currentTree = this;
            foreach (var step in path)
            {
                if (!currentTree.ContainsKey(step))
                {
                    currentTree[step] = new PathValidator();
                }

                currentTree = currentTree[step];
            }

            // Add the property if it isn't null
            if (!string.IsNullOrWhiteSpace(property))
            {
                currentTree[property] = new PathValidator();
            }
        }

        /// <summary>
        /// Adds all the paths and properties in the provided collection of column accesses.
        /// </summary>
        /// <param name="columnAccesses"></param>
        public void AddAll(IEnumerable<QueryexColumnAccess> columnAccesses)
        {
            foreach (var columnAccess in columnAccesses)
            {
                AddPath(columnAccess.Path, columnAccess.Property);
            }
        }

        /// <summary>
        /// Validate the tree of paths against a root type, throwing localized exceptions if a path contains a non-existent property.
        /// </summary>
        /// <param name="desc">The <see cref="TypeDescriptor"/> of the root type of the <see cref="PathValidator"/> tree.</param>
        /// <param name="argName">The name of the <see cref="Query"/> argument whose paths we are currently validated, used in the error messages.</param>
        /// <param name="allowLists">Pass true to allow list navigation properties.</param>
        /// <param name="allowSimpleTerminals">Pass true to allow paths that terminate with simple properties (non navigation).</param>
        /// <param name="allowNavigationTerminals">Pass true to allow paths that terminate with navigation properties.</param>
        public void Validate(TypeDescriptor desc, string argName, bool allowLists, bool allowSimpleTerminals, bool allowNavigationTerminals)
        {
            foreach (var key in Keys)
            {
                var prop = desc.Property(key);
                if (prop == null)
                {
                    // Validation taking place
                    throw new QueryException($"Property {key} does not exist on type {desc.Name}.");
                }

                // Gather some information
                bool isNavigation = prop is NavigationPropertyDescriptor;
                bool isList = prop is CollectionPropertyDescriptor collProp;
                bool isSimple = !isNavigation && !isList;

                // Validate list property
                if (!allowLists && isList)
                {
                    // Validation taking place
                    throw new QueryException($"Property {key} on type {desc.Name} is a collection, and therefore not allowed in a {argName} path.");
                }

                var next = this[key];
                bool nextIsLeaf = next.Keys.Count == 0;
                if (nextIsLeaf)
                {
                    // terminal
                    if (isNavigation)
                    {
                        if (!allowNavigationTerminals)
                        {
                            // Validation taking place
                            throw new QueryException($"A {argName} path cannot terminate with a navigation field {key}.");
                        }
                    }

                    if (isSimple)
                    {
                        if (!allowSimpleTerminals)
                        {
                            // Validation taking place
                            throw new QueryException($"A {argName} path cannot terminate with a simple field {key}.");
                        }
                    }
                }
                else // Not leav
                {
                    if (isSimple)
                    {
                        // Validation taking place
                        throw new QueryException($"The {argName} path contains a simple field {key} that was used like a navigation field.");
                    }
                    else
                    {
                        // Validate recursively
                        TypeDescriptor nextDescriptor = prop.GetEntityDescriptor();

                        next.Validate(
                            nextDescriptor, 
                            argName,
                            allowLists, 
                            allowSimpleTerminals, 
                            allowNavigationTerminals);
                    }
                }
            }
        }
    }
}
