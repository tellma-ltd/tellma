using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;
using System.Linq;

namespace BSharp.Services.ImportExport
{
    public class ExcelHandler : FileHandlerBase
    {
        private readonly IStringLocalizer _localizer;

        public ExcelHandler(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        public override AbstractDataGrid ToAbstractGrid(Stream fileStream)
        {
            using (var excelPackage = new ExcelPackage(fileStream))
            {
                // Determine which sheet is to be imported
                var sheets = excelPackage.Workbook.Worksheets;
                ExcelWorksheet sheet;
                if(sheets.Count == 1)
                {
                    sheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                }
                else
                {
                    sheet = excelPackage.Workbook.Worksheets.FirstOrDefault(e => e.Name == _localizer["Data"]);
                }

                if (sheet == null)
                {
                    throw new FormatException(_localizer["Error_ExcelContainsMultipleSheetsNameOne0", _localizer["Data"]]);
                }

                // This code copies all the cells in the Excel field to an abstract 2-D string representation
                var cells = sheet.Cells;
                var range = cells.Where(e => !string.IsNullOrWhiteSpace(e.Value?.ToString()));

                // If the Excel file is empty then return an empty grid
                if(range.Count() == 0)
                {
                    return new AbstractDataGrid(0, 0);
                }

                int maxCol = range.Max(e => e.End.Column);
                int maxRow = range.Max(e => e.End.Row);
                var abstractGrid = new AbstractDataGrid(maxCol, maxRow);

                // Loop over the Excel and copy the data to the abstract grid
                for (int row = 1; row <= maxRow; row++)
                {
                    abstractGrid.AddRow();
                    for (int column = 1; column <= maxCol; column++)
                    {
                        var cell = cells[row, column];
                        abstractGrid[row - 1][column - 1] = AbstractDataCell.Cell(cell.Value);
                    }
                }

                return abstractGrid;
            }
        }

        public override Stream ToFileStream(AbstractDataGrid abstractGrid)
        {
            // The memory stream will contain the Excel
            var fileStream = new MemoryStream();
            using (var excelPackage = new ExcelPackage(fileStream))
            {
                // Prepare the sheet and set RTL direction
                var sheet = excelPackage.Workbook.Worksheets.Add(_localizer["Data"]);
                sheet.View.RightToLeft = abstractGrid.IsRightToLeft;
                
                // Loop over the abstract grid and copy its contents to the Excel sheet
                for (int r = 0; r < abstractGrid.Count; r++)
                {
                    var row = abstractGrid[r];
                    for (int c = 0; c < row.Length; c++)
                    {
                        sheet.Cells[r + 1, c + 1].Value = row[c]?.Content;

                        // Apply the horizontal alignment and number format styling of the first row on the entire column
                        if (r == 0)
                        {
                            var alignment = row[c]?.HorizontalAlignment ?? HorizontalAlignment.Left;
                            if (alignment != HorizontalAlignment.Default)
                            {
                                // The enum values of HorizontalAlignment members were chosen to match those of ExcelHorizontalAlignment
                                sheet.Column(c + 1).Style.HorizontalAlignment = (ExcelHorizontalAlignment)alignment;
                            }

                            var numberFormat = row[c]?.NumberFormat;
                            if(numberFormat != null)
                            {
                                sheet.Column(c + 1).Style.Numberformat.Format = numberFormat;
                            }
                        }
                    }
                }

                // Save to the memory stream
                excelPackage.Save();
            }

            return fileStream;
        }
    }
}
