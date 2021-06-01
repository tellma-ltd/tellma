using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// Implementation of <see cref="IDataExtractor"/> for MS Excel files.
    /// </summary>
    public class ExcelExtractor : IDataExtractor
    {
        /// <summary>
        /// Extract raw data from a XLSX file.
        /// </summary>
        /// <param name="stream">The stream containing the XLSX file.</param>
        /// <returns>The raw data in the form of an <see cref="IEnumerable{T}"/> of string arrays.</returns>
        public IEnumerable<string[]> Extract(Stream xlsxStream)
        {
            using var spreadsheetDocument = SpreadsheetDocument.Open(xlsxStream, false);

            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
            WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();

            // Get the one and only list of shared strings
            var sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

            // Load the rows using the SAX approach
            OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
            List<List<DataCell>> data = new List<List<DataCell>>();
            int maxColIndexForSheet = 1;
            while (reader.Read())
            {
                // We're only interested in Rows
                if (reader.ElementType != typeof(Row))
                {
                    continue;
                }

                // Load the row
                var row = reader.LoadCurrentElement() as Row;

                // Determine the row index
                int rowIndex;
                if (row.RowIndex != null && row.RowIndex.HasValue)
                {
                    // Despite its name, RowIndex is actually 1-based
                    rowIndex = ((int)row.RowIndex.Value) - 1;
                }
                else
                {
                    rowIndex = data.Count;
                }

                // Add enough data rows until the index is covered
                while (rowIndex >= data.Count)
                {
                    data.Add(new List<DataCell>(maxColIndexForSheet + 1));
                }

                // Retrieve the row using index
                var dataRow = data[rowIndex];
                if (dataRow.Count > 0)
                {
                    // Basic validation, ensures rows won't overwrite each other
                    throw new ServiceException("Excel file is corrupted, multiple row elements have the same reference.");
                }

                // Get the row cells
                var cells = row.ChildElements.OfType<Cell>();
                int maxColIndexForRow = 0;
                foreach (var cell in cells)
                {
                    // Get the value
                    string value;
                    if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                    {
                        if (!int.TryParse(cell.InnerText, out int stringIndex))
                        {
                            throw new ServiceException("Excel file is corrupted, a shared string value could not be interpreted as an integer index.");
                        }

                        if (stringIndex >= sharedStrings.Length)
                        {
                            throw new ServiceException("Excel file is corrupted, a shared string index is outside the range of the shared string table.");
                        }

                        value = sharedStrings[stringIndex].Text.Text;
                    }
                    else
                    {
                        value = cell.InnerText;
                    }

                    // Discard empty cells
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    // Get the column index
                    int colIndex;
                    if (string.IsNullOrWhiteSpace(cell.CellReference))
                    {
                        colIndex = maxColIndexForRow;
                    }
                    else
                    {
                        colIndex = ColumnIndex(cell.CellReference);
                    }

                    // Add the data cell to the data row
                    dataRow.Add(new DataCell
                    {
                        ColIndex = colIndex,
                        Value = value
                    });

                    // Maintain the maximum column index in this row
                    if (maxColIndexForRow < colIndex)
                    {
                        maxColIndexForRow = colIndex;
                    }
                }

                // Maintain the maximum column index overall
                if (maxColIndexForSheet < maxColIndexForRow)
                {
                    maxColIndexForSheet = maxColIndexForRow;
                }
            }

            // The number of columns
            int numberOfColumns = maxColIndexForSheet + 1;

            // Turn the data into strings
            foreach (var dataRow in data)
            {
                string[] stringArrayRow = new string[numberOfColumns];
                bool[] populatedCells = new bool[numberOfColumns];
                foreach (var dataCell in dataRow)
                {
                    // Basic validation, ensures cells won't overwrite each other
                    if (populatedCells[dataCell.ColIndex])
                    {
                        throw new ServiceException("Excel file is corrupted, multiple cell elements have the same reference.");
                    }
                    else
                    {
                        populatedCells[dataCell.ColIndex] = true;
                    }

                    // Add the string value to the string array at the correct index
                    stringArrayRow[dataCell.ColIndex] = dataCell.Value;
                }

                yield return stringArrayRow;
            }
        }

        private static int ColumnIndex(string cellRef)
        {
            int alphabetLength = 'Z' - 'A' + 1;
            int colNumber = 0;
            foreach (var c in cellRef.ToUpper())
            {
                if (char.IsNumber(c))
                {
                    break;
                }

                colNumber = (colNumber * alphabetLength) + (c - ('A' - 1));
            }

            return colNumber - 1;
        }

        private struct DataCell
        {
            public int ColIndex { get; set; }

            public string Value { get; set; }
        }
    }
}
