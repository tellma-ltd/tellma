using Tellma.Controllers;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using Tellma.Entities.Descriptors;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Efficiently validates a collection of paths against a root type
    /// </summary>
    public class PathValidator : Dictionary<string, PathValidator>
    {
        /// <summary>
        /// Adds a path to the <see cref="PathValidator"/> tree
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
        /// Adds all the paths and properties in the provided collection of column accesses
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
        /// Validate the tree of paths against a root type, throwing localized exceptions if a path contains a non-existent property
        /// </summary>
        /// <param name="desc">The <see cref="TypeDescriptor"/> of the root type of the <see cref="PathValidator"/> tree</param>
        /// <param name="localizer">Used to localize the error messages</param>
        /// <param name="argName">The name of the <see cref="Query"/> argument whose paths we are currently validated, used in the error messages</param>
        /// <param name="allowLists">Pass true to allow list navigation properties</param>
        /// <param name="allowSimpleTerminals">Pass true to allow paths that terminate with simple properties (non navigation)</param>
        /// <param name="allowNavigationTerminals">Pass true to allow paths that terminate with navigation properties</param>
        public void Validate(TypeDescriptor desc, IStringLocalizer localizer, string argName, bool allowLists, bool allowSimpleTerminals, bool allowNavigationTerminals)
        {
            foreach (var key in Keys)
            {
                var prop = desc.Property(key);
                if (prop == null)
                {
                    // Validation taking place
                    string message = localizer["Error_Property0DoesNotExistOnType1", key, desc.Name];
                    throw new BadRequestException(message);
                }

                // Gather some information
                bool isNavigation = prop is NavigationPropertyDescriptor;
                bool isList = prop is CollectionPropertyDescriptor collProp;
                bool isSimple = !isNavigation && !isList;

                // Validate list property
                if (!allowLists && isList)
                {
                    // Validation taking place
                    string message = localizer["Error_Property0OnType1IsACollection2", key, desc.Name, argName];
                    throw new BadRequestException(message);
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
                            string message = localizer["Error_A0PathCannotTerminateWithANavigationField1", argName, key];
                            throw new BadRequestException(message);
                        }
                    }

                    if (isSimple)
                    {
                        if (!allowSimpleTerminals)
                        {
                            // Validation taking place
                            string message = localizer["Error_A0PathCannotTerminateWithASimpleField1", argName, key];
                            throw new BadRequestException(message);
                        }
                    }
                }
                else // Not leav
                {
                    if (isSimple)
                    {
                        // Validation taking place
                        string message = localizer["Error_A0PathContainsSimpleField1UsedLikeNavigation", argName, key];
                        throw new BadRequestException(message);
                    }
                    else
                    {
                        // Validate recursively
                        TypeDescriptor nextDescriptor = prop.GetEntityDescriptor();

                        next.Validate(nextDescriptor, localizer, argName,
                            allowLists, allowSimpleTerminals, allowNavigationTerminals);
                    }
                }
            }
        }
    }
}
