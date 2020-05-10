using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tellma.Entities;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.ImportExport
{
    public class UserForeignKeys
    {
        private readonly Dictionary<Type, HashSet<string>> _strings = new Dictionary<Type, HashSet<string>>();
        private readonly Dictionary<Type, HashSet<int>> _ints = new Dictionary<Type, HashSet<int>>();

        public void AddIntKey(Type type, int id)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!_ints.TryGetValue(type, out HashSet<int> ids))
            {
                ids = _ints[type] = new HashSet<int>();
            }

            ids.Add(id);
        }

        public void AddStringKey(Type type, string id)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!_strings.TryGetValue(type, out HashSet<string> ids))
            {
                ids = _strings[type] = new HashSet<string>();
            }

            ids.Add(id);
        }
    }

    public class DataParser<TEntityForSave> where TEntityForSave : EntityWithKey
    {
        public UserForeignKeys ExtractUserForeignKeys(IEnumerable<string[]> data, MappingInfo mapping, bool skipHeader = true)
        {
            var fks = new UserForeignKeys();
            data = skipHeader ? data.Skip(1) : data;
            int rowNumber = 2;

            foreach (var row in data)
            {
                foreach (var fkMapping in mapping.GetForeignKeys())
                {
                    string stringKey = row[fkMapping.ColumnIndex];
                    if (string.IsNullOrEmpty(stringKey))
                    {
                        continue;
                    }

                    switch (fkMapping.UserKeyType)
                    {
                        case UserKeyType.String:
                            fks.AddStringKey(fkMapping.TargetType, stringKey);

                            break;

                        case UserKeyType.Int:
                            if (!int.TryParse(stringKey, out int intKey))
                            {
                                throw new InvalidOperationException(""); // TODO: Validation Errors
                            }

                            fks.AddIntKey(fkMapping.TargetType, intKey);

                            break;

                        default:
                            throw new InvalidOperationException("Bug: Only int and string IDs are supported");
                    }

                }

                rowNumber++;
            }

            return fks;
        }

        public IEnumerable<TEntityForSave> Parse(IEnumerable<string[]> data, MappingInfo mapping, bool skipHeader = true)
        {
            var list = new List<TEntityForSave>();
            data = skipHeader ? data.Skip(1) : data;
            int rowNumber = 2;

            foreach (var dataRow in data) // Foreach row
            {
                ParseRow(dataRow, rowNumber, mapping, list); // Recursive function
                rowNumber++;
            }

            return list;
        }

        private void ParseRow(string[] dataRow, int rowNumber, MappingInfo mapping, IList result)
        {
            bool entityCreated = false;
            foreach (var simpleMapping in mapping.SimpleProperties)
            {
                var stringField = dataRow[simpleMapping.ColumnIndex];
                if (!string.IsNullOrEmpty(stringField))
                {
                    if (!entityCreated)
                    {
                        mapping.Entity = mapping.CreateEntity();
                        result.Add(mapping.Entity);
                        entityCreated = true;
                    }

                    if (!simpleMapping.Ignore)
                    {
                        var parsedField = stringField.ChangeType(simpleMapping.PropertyType); // TODO: Nav properties, and validation
                        simpleMapping.SetProperty(mapping.Entity, parsedField);
                    }
                }
            }

            foreach (var listMapping in mapping.ListProperties)
            {
                if (entityCreated)
                {
                    var list = listMapping.CreateAndAssignList(mapping.Entity);
                    ParseRow(dataRow, rowNumber, listMapping, list);
                }
            }
        }
    }

    public class MappingInfo
    {
        // Used by the algorithm to remember the last created entity of a list
        public EntityWithKey Entity { get; set; }

        // Info
        public Func<EntityWithKey> CreateEntity { get; }

        // Tree
        public IEnumerable<ListMappingInfo> ListProperties { get; }
        public IEnumerable<SimpleMappingInfo> SimpleProperties { get; }

        public IEnumerable<SimpleMappingInfo> GetForeignKeys()
        {
            var thisLevelFks = SimpleProperties.Where(e => e.IsForeignKey);
            var lowerLevelsFks = ListProperties.SelectMany(e => e.GetForeignKeys());

            return thisLevelFks.Concat(lowerLevelsFks);
        }
    }

    /// <summary>
    /// Mapping info for a list step, this is always the root of the mapping
    /// </summary>
    public class ListMappingInfo : MappingInfo
    {

        public Func<EntityWithKey, IList> CreateAndAssignList { get; }
    }

    /// <summary>
    /// Mapping info for a simple property
    /// </summary>
    public class SimpleMappingInfo
    {
        public int ColumnIndex { get; }

        public bool Ignore { get; } // True for # columns that aren't mapped to anything, but trigger creating a new entity

        public Type PropertyType { get; } // Simple types like string, int, decimal, etc...

        public Action<EntityWithKey, object> SetProperty { get; } // Should be the same as the property type


        // For foreign keys

        public bool IsForeignKey { get; set; }

        /// <summary>
        /// The type of the user key (Code, Name, etc), can be either string or int
        /// </summary>
        public UserKeyType UserKeyType { get; set; }

        /// <summary>
        /// The type of the entity that the foreign key is pointing to
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// The property used as user key
        /// </summary>
        public PropertyInfo UserKey { get; set; }
    }


    public enum UserKeyType
    {
        String, Int
    }
}
