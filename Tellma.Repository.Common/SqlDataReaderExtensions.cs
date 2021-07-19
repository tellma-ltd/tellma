using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Repository.Common
{
    public static class SqlDataReaderExtensions
    {
        #region Null Handling

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetValue(int)"/> but also handles the null case.
        /// </summary
        public static object Value(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetValue(index);
        }

        /// <summary>
        /// Retrieves the value of the column that has the given <see cref="name"/> while handling the null case.
        /// </summary>
        public static object Value(this SqlDataReader reader, string name)
        {
            var val = reader[name];
            return val == DBNull.Value ? null : val;
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetInt32(int)"/> but also handles the null case.
        /// </summary
        public static int? Int32(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetInt32(index);
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetBoolean(int)"/> but also handles the null case.
        /// </summary
        public static bool? Boolean(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetBoolean(index);
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetInt16(int)"/> but also handles the null case.
        /// </summary
        public static short? Int16(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetInt16(index);
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetDecimal(int)"/> but also handles the null case.
        /// </summary
        public static decimal? Decimal(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetDecimal(index);
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetString(int)"/> but also handles the null case.
        /// </summary
        public static string String(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetString(index);
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetDateTime(int)"/> but also handles the null case.
        /// </summary
        public static DateTime? DateTime(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetDateTime(index);
        }

        /// <summary>
        /// Equivalent to <see cref="SqlDataReader.GetGuid(int)"/> but also handles the null case.
        /// </summary
        public static Guid? Guid(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetGuid(index);
        }

        #endregion

        #region Reading

        /// <summary>
        /// Loads a list of <see cref="ValidationError"/>s from the <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="reader">The reader to load the data from.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<IEnumerable<ValidationError>> LoadErrors(this SqlDataReader reader, CancellationToken cancellation = default)
        {
            var errors = new List<ValidationError>();
            while (await reader.ReadAsync(cancellation))
            {
                int i = 0;
                errors.Add(new ValidationError
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

            return errors;
        }

        /// <summary>
        /// Expects a list of [Index], [Id], both int, from the <see cref="SqlDataReader"/>. Loads the ids and sorts them by index.
        /// </summary>
        /// <param name="reader">The reader to load the data from.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<List<int>> LoadIds(this SqlDataReader reader, CancellationToken cancellation = default)
        {
            // (1) Retrieve the Ids and their indices from SQL data reader
            var indexedIds = new List<IndexedId>();
            while (await reader.ReadAsync(cancellation))
            {
                int i = 0;
                indexedIds.Add(new IndexedId
                {
                    Index = reader.GetInt32(i++),
                    Id = reader.GetInt32(i++)
                });
            }

            // (2) Sort the the Ids according to their index
            var sortedIdArray = new int[indexedIds.Count];
            indexedIds.ForEach(e =>
            {
                sortedIdArray[e.Index] = e.Id;
            });

            // (3) Return the sorted Ids
            return new List<int>(sortedIdArray);
        }

        /// <summary>
        /// First loads the <see cref="ValidationError"/>s, if none are returned and returnIds is true it moves
        /// to the next result set and loads the ids sorted by index. Returns both the errors and the ids in a <see cref="SaveResult"/> object.
        /// </summary>
        /// <param name="returnIds">Whether or not to return the Ids.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<SaveResult> LoadSaveResult(this SqlDataReader reader, bool returnIds, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);

            // (2) If no errors => load the Ids
            List<int> ids = null;
            if (returnIds && !errors.Any())
            {
                await reader.NextResultAsync(cancellation);
                ids = await reader.LoadIds(cancellation);
            }

            // (3) Return the result
            return new SaveResult(errors, ids);
        }

        /// <summary>
        /// Loads the <see cref="ValidationError"/>s, if any, and returns them in a <see cref="DeleteResult"/> object.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<DeleteResult> LoadDeleteResult(this SqlDataReader reader, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);

            // (2) Return the result
            return new DeleteResult(errors);
        }

        /// <summary>
        /// Loads the <see cref="ValidationError"/>s, if any, and returns them in a <see cref="OperationResult"/> object.
        /// </summary>
        /// <param name="returnIds">Whether or not to return the Ids.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<OperationResult> LoadOperationResult(this SqlDataReader reader, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);

            // (2) Return the result
            return new OperationResult(errors);
        }

        #endregion
    }
}
