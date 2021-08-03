using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// A data structure that maps collections of error messages to specific 
    /// cell locations in an excel/csv worksheet.
    /// </summary>
    public class ImportErrors
    {
        /// <summary>
        /// The errors of every cell are stored in this dictionary.
        /// </summary>
        private readonly Dictionary<(int Row, int? Column), HashSet<string>> _dic = new();

        /// <summary>
        /// Returns all the errors stored in this <see cref="ImportErrors"/>, each with the row and column of the cell it pertains to.
        /// </summary>
        public IEnumerable<(int Row, int? Column, IEnumerable<string> Errors)> AllErrors => _dic.Select(e => (e.Key.Row, e.Key.Column, (IEnumerable<string>)e.Value));

        /// <summary>
        /// Adds the <paramref name="errorMessage"/> error message to the list of errors messages 
        /// that pertain to the cell (<paramref name="row"/>, <paramref name="column"/>) if
        /// <see cref="MaxAllowedErrors"/> has not been reached.
        /// </summary>
        /// <param name="row">The row number of the problematic cell.</param>
        /// <param name="column">The column number of the problematic cell.</param>
        /// <param name="errorMessage">The error message to add.</param>
        /// <returns>True if the maximum limit has not been reached.</returns>
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

        /// <summary>
        /// The maximum number of error messages that can be added to this <see cref="ImportErrors"/>.
        /// </summary>
        public static int MaxAllowedErrors => 100;

        /// <summary>
        /// The number of error messages currently added to this <see cref="ImportErrors"/>.
        /// </summary>
        public int ErrorCount { get; private set; } = 0;

        /// <summary>
        /// Syntactic sugar for <see cref="ErrorCount"/> === 0.
        /// </summary>
        public bool IsValid => ErrorCount == 0;

        /// <summary>
        /// Syntactic sugar for <see cref="ErrorCount"/> >= <see cref="MaxAllowedErrors"/>.
        /// </summary>
        public bool HasReachedMaxErrors => ErrorCount >= MaxAllowedErrors;

        /// <summary>
        /// Concatenates the errors, each in a new line, prefixed with the cell coordinates or the row number.
        /// The cell coordinates are formated Excel-style: A1, B2, C15.
        /// </summary>
        /// <param name="localizer">Localizer for row and column prefixes.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="localizer"/> is null.</exception>
        public string ToString(IStringLocalizer localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            var bldr = new StringBuilder();
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
                throw new InvalidOperationException($"Bug: Column number {columnNumber} should not be less than 1.");
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

        /// <summary>
        /// If the structre contains any errors it stringifies them and throws them in a <see cref="ServiceException"/>.
        /// </summary>
        /// <param name="localizer">Used to localizer the error messages.</param>
        /// <exception cref="ServiceException">If the structure contains errors.</exception>
        public void ThrowIfInvalid(IStringLocalizer localizer)
        {
            if (!IsValid)
            {
                string msg = ToString(localizer);
                throw new ServiceException(msg);
            }
        }
    }
}
