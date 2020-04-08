using Tellma.Data.Queries;
using Tellma.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;

namespace Tellma.Data
{
    public static class RepositoryUtilities
    {
        /// <summary>
        /// Constructs a SQL data table containing all the public properties of the 
        /// entities' type and populates the data table with the provided entities
        /// </summary>
        public static DataTable DataTable<T>(IEnumerable<T> entities, bool addIndex = false)
        {
            DataTable table = new DataTable();
            if (addIndex)
            {
                // The column order MUST match the column order in the user-defined table type
                table.Columns.Add(new DataColumn("Index", typeof(int)));
            }

            var props = AddColumnsFromProperties<T>(table);

            int index = 0;
            foreach (var entity in entities)
            {
                DataRow row = table.NewRow();

                // We add an index property since SQL works with un-ordered sets
                if (addIndex)
                {
                    row["Index"] = index++;
                }

                // Add the remaining properties
                foreach (var prop in props)
                {
                    var propValue = prop.GetValue(entity);
                    row[prop.Name] = propValue ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        public static DataTable DataTableWithHeaderIndex<THeader, TLines>(IEnumerable<THeader> entities, Func<THeader, List<TLines>> linesFunc)
        {
            DataTable table = new DataTable();

            // The column order MUST match the column order in the user-defined table type
            table.Columns.Add(new DataColumn("Index", typeof(int)));
            table.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));

            var props = AddColumnsFromProperties<TLines>(table);

            int headerIndex = 0;
            foreach (var entity in entities)
            {
                int index = 0;
                var lines = linesFunc(entity);
                if (lines != null)
                {
                    foreach (var line in linesFunc(entity))
                    {
                        DataRow row = table.NewRow();

                        // We add an index property since SQL works with un-ordered sets
                        row["Index"] = index++;
                        row["HeaderIndex"] = headerIndex;

                        // Add the remaining properties
                        foreach (var prop in props)
                        {
                            var propValue = prop.GetValue(line);
                            row[prop.Name] = propValue ?? DBNull.Value;
                        }

                        table.Rows.Add(row);
                    }
                }

                headerIndex++;
            }

            return table;
        }

        public static DataTable DataTableWithParentIndex<T>(IEnumerable<T> entities, Func<T, int?> parentIndexFunc)
        {
            DataTable table = new DataTable();

            // The column order MUST match the column order in the user-defined table type
            table.Columns.Add(new DataColumn("Index", typeof(int)));
            table.Columns.Add(new DataColumn("ParentIndex", typeof(int)));

            var props = AddColumnsFromProperties<T>(table);

            int index = 0;
            foreach (var entity in entities)
            {
                DataRow row = table.NewRow();

                // We add an index properties since SQL works with un-ordered sets
                row["Index"] = index++;
                row["ParentIndex"] = (object)parentIndexFunc(entity) ?? DBNull.Value;

                // Add the remaining properties
                foreach (var prop in props)
                {
                    var propValue = prop.GetValue(entity);
                    row[prop.Name] = propValue ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        public static (DataTable Documents, DataTable Lines, DataTable Entries) DataTableFromDocuments(IEnumerable<DocumentForSave> documents)
        {
            // Prepare the documents table skeleton
            DataTable docsTable = new DataTable();
            docsTable.Columns.Add(new DataColumn("Index", typeof(int)));
            var docsProps = AddColumnsFromProperties<DocumentForSave>(docsTable);

            // Prepare the lines table skeleton
            DataTable linesTable = new DataTable();
            linesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            linesTable.Columns.Add(new DataColumn("DocumentIndex", typeof(int)));
            var linesProps = AddColumnsFromProperties<LineForSave>(linesTable);

            // Prepare the entries table skeleton
            DataTable entriesTable = new DataTable();
            entriesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            entriesTable.Columns.Add(new DataColumn("LineIndex", typeof(int)));
            entriesTable.Columns.Add(new DataColumn("DocumentIndex", typeof(int)));
            var entriesProps = AddColumnsFromProperties<EntryForSave>(entriesTable);

            // Add the docs
            int docsIndex = 0;
            foreach (var doc in documents)
            {
                DataRow docsRow = docsTable.NewRow();

                docsRow["Index"] = docsIndex;

                foreach (var docsProp in docsProps)
                {
                    var docsPropValue = docsProp.GetValue(doc);
                    docsRow[docsProp.Name] = docsPropValue ?? DBNull.Value;
                }

                // Add the lines if any
                if (doc.Lines != null)
                {
                    int linesIndex = 0;
                    doc.Lines.ForEach(line =>
                    {
                        DataRow linesRow = linesTable.NewRow();

                        linesRow["Index"] = linesIndex;
                        linesRow["DocumentIndex"] = docsIndex;

                        foreach (var linesProp in linesProps)
                        {
                            var linesPropValue = linesProp.GetValue(line);
                            linesRow[linesProp.Name] = linesPropValue ?? DBNull.Value;
                        }

                        if (line.Entries != null)
                        {
                            int entriesIndex = 0;
                            line.Entries.ForEach(entry =>
                            {
                                DataRow entriesRow = entriesTable.NewRow();

                                entriesRow["Index"] = entriesIndex;
                                entriesRow["LineIndex"] = linesIndex;
                                entriesRow["DocumentIndex"] = docsIndex;

                                foreach (var entriesProp in entriesProps)
                                {
                                    var entriesPropValue = entriesProp.GetValue(entry);
                                    entriesRow[entriesProp.Name] = entriesPropValue ?? DBNull.Value;
                                }

                                entriesTable.Rows.Add(entriesRow);
                                entriesIndex++;
                            });
                        }

                        linesTable.Rows.Add(linesRow);
                        linesIndex++;
                    });
                }

                docsTable.Rows.Add(docsRow);
                docsIndex++;
            }

            return (docsTable, linesTable, entriesTable);
        }

        private static IEnumerable<PropertyInfo> AddColumnsFromProperties<T>(DataTable table)
        {
            var props = typeof(T).GetMappedProperties();
            foreach (var prop in props)
            {
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var column = new DataColumn(prop.Name, propType);
                if (propType == typeof(string))
                {
                    // For string columns, it is more performant to explicitly specify the maximum column size
                    // According to this article: http://www.dbdelta.com/sql-server-tvp-performance-gotchas/
                    var stringLengthAttribute = prop.GetCustomAttribute<StringLengthAttribute>(inherit: true);
                    if (stringLengthAttribute != null)
                    {
                        column.MaxLength = stringLengthAttribute.MaximumLength;
                    }
                }

                table.Columns.Add(column);
            }

            return props;
        }


        /// <summary>
        /// Determines whether the given exception is a foreign key violation on delete
        /// </summary>
        public static bool IsForeignKeyViolation(SqlException ex)
        {
            return ex.Number == 547;
        }

        /// <summary>
        /// Enlists the transaction in the ambient transaction, or in the transactionOverride if active
        /// </summary>
        public static void EnlistInTransaction(this SqlConnection conn, Transaction transactionOverride = null)
        {
            if (transactionOverride?.TransactionInformation.Status == TransactionStatus.Active)
            {
                conn.EnlistTransaction(transactionOverride);
            }
            else
            {
                conn.EnlistTransaction(Transaction.Current);
            }
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetInt32(int)"/> but also handles the null case
        /// </summary
        public static int? Int32(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? (int?)null : reader.GetInt32(index);
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetString(int)"/> but also handles the null case
        /// </summary
        public static string String(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetString(index);
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetGuid(int)"/> but also handles the null case
        /// </summary
        public static Guid? Guid(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? (Guid?)null : reader.GetGuid(index);
        }

        /// <summary>
        /// Loads the results of a validation stored procedure into a list of <see cref="ValidationError"/>
        /// </summary>
        public static async Task<List<ValidationError>> LoadErrors(SqlCommand cmd)
        {
            var result = new List<ValidationError>();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int i = 0;
                    result.Add(new ValidationError
                    {
                        Key = reader.String(i++),
                        ErrorName = reader.String(i++),
                        Argument1 = reader.String(i++),
                        Argument2 = reader.String(i++),
                        Argument3 = reader.String(i++),
                        Argument4 = reader.String(i++),
                        Argument5 = reader.String(i++)
                    });
                }
            }

            return result;
        }

        public static async Task<List<InboxNotificationInfo>> LoadAssignmentNotificationInfos(SqlDataReader reader, List<InboxNotificationInfo> result = null)
        {
            result ??= new List<InboxNotificationInfo>();

            while (await reader.ReadAsync())
            {
                int i = 0;
                result.Add(new InboxNotificationInfo
                {
                    ExternalId = reader.GetString(i++),
                    Count = reader.GetInt32(i++),
                    UnknownCount = reader.GetInt32(i++)
                });
            }

            return result;
        }
    }
}
