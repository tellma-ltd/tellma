using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Localization;

namespace Tellma.Controllers.ImportExport
{
    /// <summary>
    /// Able to extract raw data from a specific file type into an <see cref="IEnumerable{T}"/> of string arrays
    /// </summary>
    public interface IDataExtracter
    {
        IEnumerable<string[]> Extract(Stream csvStream);
    }

    public class CsvHandler : IDataExtracter
    {
        public IEnumerable<string[]> Extract(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            while (csvReader.Read())
            {
                yield return csvReader.Context.Record;
            }
        }

        public Stream Package(IEnumerable<string[]> data)
        {
            // Handle null data
            if (data is null)
            {
                throw new InvalidOperationException("Bug: the data is null");
            }

            // Handle empty data
            if (!data.Skip(1).Any())
            {
                throw new InvalidOperationException("Bug: Must supply the CSV header at least");
            }

            // Get column count
            int columnCount = -1;
            if (data.Any())
            {
                columnCount = data.First().Length;
            }

            // Build the CSV
            var builder = new StringBuilder();
            foreach (var row in data)
            {
                if (row.Length != columnCount)
                {
                    // Double check
                    throw new InvalidOperationException("Bug: Number of columns is inconsistent across rows");
                }

                for (int i = 0; i < row.Length; i++)
                {
                    var processedField = ProcessFieldForCsv(row[i]);
                    builder.Append(processedField);

                    if (i < row.Length)
                    {
                        builder.Append(',');
                    }
                }

                builder.AppendLine();
            }

            // Package as bytes
            var csvString = builder.ToString();
            byte[] result = Encoding.UTF8.GetBytes(csvString);

            // Return the bytes
            return new MemoryStream(result);
        }

        private string ProcessFieldForCsv(string field)
        {
            if (field == null)
            {
                // Null in - Null out
                return null;
            }

            // Escape every double quote with another double quote
            field = field.Replace("\"", "\"\"");

            // Surround any field that contains double quotes, new lines, or commas with double quotes
            if (field.Contains('"') || field.Contains(',') || field.Contains('\n') || field.Contains('\r'))
            {
                field = $"\"{field}\"";
            }

            // Yield the processed field
            return field;
        }
    }

    public class ExcelHandler : IDataExtracter
    {
        public IEnumerable<string[]> Extract(Stream csvStream)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
