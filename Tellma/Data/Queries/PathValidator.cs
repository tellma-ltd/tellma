using Tellma.Controllers;
using Tellma.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;

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
        /// Validate the tree of paths against a root type, throwing localized exceptions if a path contains a non-existent property
        /// </summary>
        /// <param name="type">The root type of the <see cref="PathValidator"/> tree</param>
        /// <param name="localizer">Used to localize the error messages</param>
        /// <param name="argName">The name of the <see cref="Query"/> argument whose paths we are currently validated, used in the error messages</param>
        /// <param name="allowLists">Pass true to allow list navigation properties</param>
        /// <param name="allowSimpleTerminals">Pass true to allow paths that terminate with simple properties (non navigation)</param>
        /// <param name="allowNavigationTerminals">Pass true to allow paths that terminate with navigation properties</param>
        public void Validate(Type type, IStringLocalizer localizer, string argName, bool allowLists, bool allowSimpleTerminals, bool allowNavigationTerminals)
        {
            foreach (var key in Keys)
            {
                var prop = type.GetProperty(key);
                if (prop == null)
                {
                    // Validation taking place
                    string message = localizer["Error_Property0DoesNotExistOnType1", key, type.Name];
                    throw new BadRequestException(message);
                }

                var isList = prop.PropertyType.IsList();
                if (!allowLists && isList)
                {
                    // Validation taking place
                    string message = localizer["Error_Property0OnType1IsACollection2", key, type.Name, argName];
                    throw new BadRequestException(message);
                }

                var propType = isList ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;
                var propTree = this[key];

                if(propTree.Keys.Count == 0)
                {
                    // terminal
                    bool isComplex = propType.GetProperty("Id") != null;
                    if (isComplex)
                    {
                        if (!allowNavigationTerminals)
                        {
                            // Validation taking place
                            string message = localizer["Error_A0PathCannotTerminateWithANavigationField1", argName, key];
                            throw new BadRequestException(message);
                        }
                    }
                    else
                    {
                        if (!allowSimpleTerminals)
                        {
                            // Validation taking place
                            string message = localizer["Error_A0PathCannotTerminateWithASimpleField1", argName, key];
                            throw new BadRequestException(message);
                        }
                    }
                }

                // Validate recursively
                this[key].Validate(propType, localizer, argName,
                    allowLists, allowSimpleTerminals, allowNavigationTerminals);
            }
        }
    }
}
