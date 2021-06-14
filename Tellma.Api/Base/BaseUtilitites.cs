using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tellma.Api.ImportExport;
using Tellma.Utilities.Common;

namespace Tellma.Api.Base
{
    public static class BaseUtilitites
    {
        /// <summary>
        /// Takes an XLSX or a CSV stream and unpackages its content into a 2-D table of strings.
        /// </summary>
        /// <param name="stream">The contents of the XLSX or CSV file.</param>
        /// <param name="fileName">The name of the file to extract if available.</param>
        /// <param name="contentType">The mime type of the file to extract if available.</param>
        /// <param name="localizer">To localize error messages.</param>
        /// <returns>A 2-D grid of strings representing the contents of the XLSX or the CSV file.</returns>
        public static IEnumerable<string[]> ExtractStringsFromFile(Stream stream, string fileName, string contentType, IStringLocalizer localizer)
        {
            IDataExtractor extracter;
            if (contentType == MimeTypes.Csv || (fileName?.ToLower()?.EndsWith(".csv") ?? false))
            {
                extracter = new CsvExtractor();
            }
            else if (contentType == MimeTypes.Excel || (fileName?.ToLower()?.EndsWith(".xlsx") ?? false))
            {
                extracter = new ExcelExtractor();
            }
            else
            {
                throw new FormatException(localizer["Error_OnlyCsvOrExcelAreSupported"]);
            }

            // Extrat and return
            try
            {
                return extracter.Extract(stream).ToList();
            }
            catch (Exception ex)
            {
                // Report any errors during extraction
                string msg = localizer["Error_FailedToParseFileError0", ex.Message];
                throw new ServiceException(msg);
            }
        }
    }
}
