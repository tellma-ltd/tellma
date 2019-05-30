using BSharp.Controllers.Misc;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class PathValidator : Dictionary<string, PathValidator>
    {
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
