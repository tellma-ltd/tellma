using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace Tellma.Controllers.ImportExport
{

    /// <summary>
    /// Implementation of <see cref="IDataExtractor"/> for CSV files
    /// </summary>
    public class CsvExtractor : IDataExtractor
    {
        /// <summary>
        /// Extract raw data from a CSV file into an <see cref="IEnumerable{T}"/> of string arrays
        /// </summary>
        /// <param name="stream">The stream containing the CSV file</param>
        /// <returns>The raw data in the form of an <see cref="IEnumerable{T}"/> of string arrays</returns>
        public IEnumerable<string[]> Extract(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            while (csvReader.Read())
            {
                yield return csvReader.Context.Record;
            }
        }
    }
}
