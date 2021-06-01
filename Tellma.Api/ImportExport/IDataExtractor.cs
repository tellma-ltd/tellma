using System.Collections.Generic;
using System.IO;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// Implementations can extract raw data from specific file types.
    /// </summary>
    public interface IDataExtractor
    {
        /// <summary>
        /// Extract raw data from a specific file type.
        /// </summary>
        /// <param name="stream">The stream containing the file to extract the data from.</param>
        /// <returns>The raw data in the form of an <see cref="IEnumerable{T}"/> of string arrays.</returns>
        IEnumerable<string[]> Extract(Stream stream);
    }
}
