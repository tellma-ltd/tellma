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
                var sheet = excelPackage.Workbook.Worksheets.SingleOrDefault();
                if (sheet == null)
                {
                    sheet = excelPackage.Workbook.Worksheets.SingleOrDefault(e => e.Name == _localizer["Data"]);
                }

                if (sheet == null)
                {
                    throw new FormatException(_localizer["Error_ExcelContainsMultipleSheetsNameOne{0}Short", _localizer["Data"]]);
                }

                // This code copies all the cells in the Excel field to an abstract 2-D string representation
                var cells = sheet.Cells;
                int maxCol = cells.Columns;
                int maxRow = cells.Rows;
                var abstractGrid = new AbstractDataGrid(maxCol, maxRow); // TODO verify

                // Loop over the Excel and copy the data to the abstract grid
                for (int row = 1; row <= maxRow; row++)
                {
                    abstractGrid.AddRow();
                    for (int column = 1; column <= maxCol; column++)
                    {
                        var cell = cells[row, column];
                        abstractGrid[row - 1][column - 1] = cell.Value?.ToString();
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
                        if(r == 0)
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
