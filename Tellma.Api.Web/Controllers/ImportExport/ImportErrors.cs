using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tellma.Controllers.ImportExport
{
    public class ImportErrors
    {
        private readonly Dictionary<(int Row, int? Column), HashSet<string>> _dic = new Dictionary<(int, int?), HashSet<string>>();

        public IEnumerable<(int Row, int? Column, IEnumerable<string> Errors)> AllErrors => _dic.Select(e => (e.Key.Row, e.Key.Column, (IEnumerable<string>)e.Value));

        public bool AddImportError(int row, int? column, string errorMessage)
        {
            if (!HasReachedMaxErrors)
            {
                var key = (row, column);
                if (!_dic.TryGetValue(key, out HashSet<string> set))
                {
                    set = new HashSet<string>();
                    _dic.Add(key, set);
                }

                if (set.Add(errorMessage))
                {
                    ErrorCount++;
                }

                return true;
            }

            return false;
        }

        public int MaxAllowedErrors => 100;

        public int ErrorCount { get; private set; } = 0;

        public bool IsValid => ErrorCount == 0;

        public bool HasReachedMaxErrors => ErrorCount >= MaxAllowedErrors;

        /// <summary>
        /// Concatenates the errors, each in a new line, prefixed with the cell coordinates or the row number.
        /// The cell coordinates are formated Excel-style A1, B2, C15..
        /// </summary>
        /// <param name="localizer">Localizer for row and column prefixes</param>
        public string ToString(IStringLocalizer localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            StringBuilder bldr = new StringBuilder();
            bldr.Append(localizer["Error_ImportFailedBecauseOf"]);
            foreach (var (row, column, errors) in AllErrors.OrderBy(e => e.Row).ThenBy(e => e.Column))
            {
                foreach (var errorMessage in errors)
                {
                    bldr.AppendLine();
                    bldr.Append(" - ");
                    if (column != null)
                    {
                        string columnDisplay = ExcelColumn(column.Value);
                        bldr.Append(localizer["Cell0", $"{columnDisplay}{row}"]);
                    }
                    else
                    {
                        bldr.Append(localizer["Row0", row]);
                    }

                    bldr.Append(": ");
                    bldr.Append(errorMessage);
                }
            }

            if (HasReachedMaxErrors)
            {
                bldr.AppendLine();
                bldr.Append(localizer["TruncatedList"]);
            }

            return bldr.ToString();
        }

        /// <summary>
        /// Maps a column number to an Excel column name: A, B, C etc...
        /// </summary>
        private static string ExcelColumn(int columnNumber)
        {
            if (columnNumber < 1)
            {
                throw new InvalidOperationException($"Bug: Column number {columnNumber} should not be less than 1");
            }

            int alphbetLength = 'Z' - 'A' + 1;
            int dividend = columnNumber;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % alphbetLength;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        public void ThrowIfInvalid(IStringLocalizer localizer)
        {
            if (!IsValid)
            {
                string msg = ToString(localizer);
                throw new BadRequestException(msg);
            }
        }
    }
}
