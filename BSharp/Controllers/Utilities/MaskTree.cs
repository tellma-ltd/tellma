using BSharp.Entities;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Controllers.Utilities
{
    /// <summary>
    /// Represents a permission mask in an efficient-to-manipulate data structure, and provides utilties for common operations on mask trees
    /// </summary>
    public class MaskTree : Dictionary<string, MaskTree>
    {
        // [Nomenclature]
        // - mask: a string of the general form "Prop1,Prop2/Prop3,Prop4/Prop5/Prop6"
        // - path: a string of the general form "Prop2/Prop3"
        // - step: a string of the general form "Prop3"
        // - mask tree: a data structure representing a mask as a tree of dictionaries
        // - basic fields: fields on every DTO that are always accessible if you have access to the DTO as a whole (usually: Name, Code and IsActive)

        /// <summary>
        /// Keyword that represnts all the basic fields of a DTO, i.e those fields that are always accessible if you have access to the DTO as a whole
        /// </summary>
        public const string BASIC_FIELDS_KEYWORD = "BasicFields";

        /// <summary>
        /// Returns the fields of the level represented by this <see cref="MaskTree"/>
        /// </summary>
        public ICollection<string> Fields
        {
            get { return Keys; }
        }

        /// <summary>
        /// Function that splits the mask into a list of paths
        /// </summary>
        public static IEnumerable<string> Split(string mask)
        {
            if (string.IsNullOrWhiteSpace(mask))
            {
                return null;
            }

            return mask.Split(',').Select(e => e?.Trim()).Where(e => !string.IsNullOrWhiteSpace(e));
        }

        /// <summary>
        /// Function that turns a list of paths into a path tree
        /// </summary>
        public static MaskTree GetMaskTree(IEnumerable<string> paths)
        {
            var tree = new MaskTree();
            if (paths != null)
            {
                foreach (var path in paths)
                {
                    var currentTree = tree;
                    var steps = path.Split('/').Select(e => e.Trim());
                    foreach (var step in steps)
                    {
                        if (!currentTree.ContainsKey(step))
                        {
                            currentTree[step] = new MaskTree();
                        }

                        currentTree = currentTree[step];
                    }
                }
            }

            return tree;
        }

        public static MaskTree GetMaskTree(IEnumerable<(string[], string)> paths)
        {
            var tree = new MaskTree();
            if (paths != null)
            {
                foreach (var (path, prop) in paths)
                {
                    var currentTree = tree;

                    void Traverse(string step)
                    {

                        if (!currentTree.ContainsKey(step))
                        {
                            currentTree[step] = new MaskTree();
                        }

                        currentTree = currentTree[step];
                    }

                    // first the path
                    foreach (var step in path)
                    {
                        Traverse(step);
                    }

                    // then the property
                    if (!string.IsNullOrWhiteSpace(prop))
                    {
                        Traverse(prop);
                    }
                }
            }

            return tree;
        }

        /// <summary>
        /// Parses a mask string into a <see cref="MaskTree"/>. A mask string is a comma separated
        /// list of paths, which are in tern a slash (/) separated list of path steps, for example A/B,C,D/E/F contains 3 paths
        /// </summary>
        public static MaskTree Parse(string mask)
        {
            return GetMaskTree(Split(mask));
        }

        /// <summary>
        /// Finds the intersection between two mask trees, i.e the fields that are accessible in both trees
        /// </summary>
        public MaskTree IntersectionWith(MaskTree tree)
        {
            // For readability
            var first = this;
            var second = tree;

            // The intersection returns the least restrictive:
            // If one of the two is fully unmasked, return the other
            // If of the two is fully masked, return it
            if (first == second)
            {
                return first; // optimization
            }
            else if (first.IsUnrestricted || second.IsBasicFields)
            {
                return second;
            }
            else if (second.IsUnrestricted || first.IsBasicFields)
            {
                return first;
            }
            else
            {
                // (1) Find the common properties on the current level
                var commonSteps = first.Keys.Intersect(second.Keys);

                // (2) Then recursively go down to the next level (breadth-first traversal)
                if (commonSteps.Count() == 0)
                {
                    // If there are no common properties then the intersection is the base properties of the parent
                    return BasicFieldsMaskTree();
                }
                else
                {
                    MaskTree result = new MaskTree();
                    foreach (var step in commonSteps)
                    {
                        result[step] = first[step].IntersectionWith(second[step]);
                    }
                    return result;
                }

            }
        }

        /// <summary>
        /// Finds the union of two mask trees, i.e the fields that are accessible in either tree
        /// </summary>
        public MaskTree UnionWith(MaskTree tree)
        {
            // For readability
            var first = this;
            var second = tree;

            if (first == second)
            {
                return first; // optimization
            }
            else if (first.IsUnrestricted || second.IsBasicFields)
            {
                return first;
            }
            else if (second.IsUnrestricted || first.IsBasicFields)
            {
                return second;
            }
            else
            {
                // (1) Collect all propertes from both trees on the current level
                var allSteps = first.Keys.Union(second.Keys);

                // (2) Recursively go down to the next level (breadth-first traversal)
                MaskTree result = new MaskTree();
                foreach (var step in allSteps)
                {
                    if (!second.ContainsKey(step))
                    {
                        result[step] = first[step];
                    }
                    else if (!first.ContainsKey(step))
                    {
                        result[step] = second[step];
                    }
                    else
                    {
                        result[step] = first[step].UnionWith(second[step]);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Determines if the tree covers all the paths of another given tree, a normalized 
        /// tree must not cotain a path referring to a basic field directly, the last step 
        /// of such a path must be replaced with "BasicFields"
        /// </summary>
        public bool Covers(MaskTree normalizedTree)
        {
            return IsUnrestricted || normalizedTree.IsBasicFields ||
                normalizedTree.Keys.All(key => ContainsKey(key) && this[key].Covers(normalizedTree[key]));
        }

        /// <summary>
        /// Verifies that the paths in this mask tree are all valid paths with respect to a certain DTO type
        /// </summary>
        public void Validate(Type type, IStringLocalizer localizer)
        {
            foreach (var key in Keys)
            {
                // This is a valid key that doesn't correspond to any real property
                if (key == BASIC_FIELDS_KEYWORD)
                {
                    continue;
                }

                var prop = type.GetProperty(key);
                if (prop == null)
                {
                    // Validation taking place
                    string message = localizer["Error_Property0DoesNotExistOnType1", key, type.Name];
                    throw new InvalidOperationException(message);
                }

                var isList = prop.PropertyType.IsList();
                var propType = isList ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;

                // Validate recursively
                this[key].Validate(prop.PropertyType, localizer);
            }
        }

        /// <summary>
        /// Finds all paths that point to basic properties and replaces the last step with BasicProperties, note: always call Validate before calling Normalize
        /// </summary>
        /// <param name="type">The type of the principle DTO on which the Mask tree operates</param>
        public void Normalize(Type type)
        {
            if (!IsUnrestricted && !ContainsKey(BASIC_FIELDS_KEYWORD))
            {
                this[BASIC_FIELDS_KEYWORD] = new MaskTree();
            }

            List<string> toRemove = new List<string>();
            foreach (var key in Keys)
            {
                // This is a valid key that doesn't correspond to any real property
                if (key == BASIC_FIELDS_KEYWORD)
                {
                    continue;
                }

                var prop = type.GetProperty(key);
                if (prop == null)
                {
                    // Should not reach here if the developer did his/her job right, i.e called Validate before calling Normalize
                    throw new InvalidOperationException($"The property {key} does not exist on type {type.Name}");
                }

                if (prop.GetCustomAttributes(inherit: true).OfType<AlwaysAccessibleAttribute>().Any())
                {
                    toRemove.Add(key);
                }
                else
                {
                    var isList = prop.PropertyType.IsList();
                    var propType = isList ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;

                    // Normalize recursively
                    this[key].Normalize(prop.PropertyType);
                }
            }

            // Removal takes place outside the loop to avoid the dreadful "IEnumerable Was Modified" exception
            if (toRemove.Any())
            {
                toRemove.ForEach(key =>
                {
                    Remove(key);
                });
            }
        }

        /// <summary>
        /// Returns true if the mask is empty, i.e. this DTO is fully accessible
        /// </summary>
        public bool IsUnrestricted
        {
            get
            {
                return Keys.Count == 0;
            }
        }

        /// <summary>
        /// Returns true if the mask represents a basic fields only mask (the most restrictive kind)
        /// Basic fields are properties that users always have access to as long as they have access to the DTO as a whole
        /// </summary>
        public bool IsBasicFields
        {
            get
            {
                return Keys.Count == 1 && Keys.Single() == BASIC_FIELDS_KEYWORD;
            }
        }

        /// <summary>
        /// Convenience method for creating the most restrictive mask, one that allows access to the basic fields only
        /// </summary>
        public static MaskTree BasicFieldsMaskTree()
        {
            return new MaskTree
            {
                [BASIC_FIELDS_KEYWORD] = new MaskTree()
            };
        }

        /// <summary>
        /// For debugging purposes
        /// </summary>
        public override string ToString()
        {
            return string.Join(",", Paths());
        }

        private IEnumerable<string> Paths()
        {
            return this.SelectMany(pair => pair.Value.Keys.Count == 0 ? (new string[] { pair.Key }) : pair.Value.Paths().Select(path => $"{pair.Key}/{path}"));
        }
    }
}
