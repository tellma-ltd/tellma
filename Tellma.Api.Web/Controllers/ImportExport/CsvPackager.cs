using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tellma.Controllers.ImportExport
{
    /// <summary>
    /// Contains utility methods for packaging 
    /// </summary>
    public class CsvPackager
    {
        /// <summary>
        /// Transforms an <see cref="IEnumerable{T}"/> of string arrays into a CSV file stream with UTF-8 encoding.
        /// This method automatically escapes quotation marks, commas and new lines. It throws an error unless all string arrays are the same length.
        /// IMPORTANT: Keep in sync with TS function on the client
        /// </summary>
        /// <param name="data">The data to package as CSV, every string array is a new record/line, and every string in the array is a field in the row</param>
        /// <returns>The CSV file stream, encoded as UTF-8</returns>
        public Stream Package(IEnumerable<string[]> data)
        {
            // Handle null data
            if (data is null)
            {
                throw new InvalidOperationException("Bug: the data is null");
            }

            // Handle empty data
            if (!data.Any())
            {
                throw new InvalidOperationException("Bug: Must supply the CSV header at least");
            }

            // Get column count
            int columnCount = data.First().Length;

            // Build the CSV
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                foreach (var row in data)
                {
                    if (row.Length != columnCount)
                    {
                        // Double check
                        throw new InvalidOperationException("Bug: Number of columns is inconsistent across rows");
                    }

                    bool notFirstField = false;
                    foreach (var field in row)
                    {
                        if (notFirstField)
                        {
                            writer.Write(',');
                        }

                        notFirstField = true;

                        var processedField = ProcessFieldForCsv(field);
                        writer.Write(processedField);
                    }

                    writer.WriteLine();
                }
            }

            // Seek back to beginning of the CSV stream and return it
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Escapes quotation marks, commas and new lines by surrounding the value with quotations.
        /// IMPORTANT: Keep in sync with TS function on the client
        /// </summary>
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

            // Return the processed field
            return field;
        }
    }
}
