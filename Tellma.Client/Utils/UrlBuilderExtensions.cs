using System;
using System.Text.Encodings.Web;

namespace Tellma.Client
{
    internal static class UrlBuilderExtensions
    {
        /// <summary>
        /// Adds a new parameter to the query in the <see cref="UriBuilder"/>,
        /// with the given <paramref name="name"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="bldr">The <see cref="UriBuilder"/> whose query to modify.</param>
        /// <param name="name">The name of the query parameter.</param>
        /// <param name="value">The value of the query parameter.</param>
        /// <remarks>
        /// This function URL-encodes <paramref name="name"/> and <paramref name="value"/> before
        /// adding them to the query. Also if either <see cref="name"/> or <see cref="value"/> are
        /// null or whitespace nothing is added.
        /// </remarks>
        internal static void AddQueryParameter(this UriBuilder bldr, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            // Original query
            var query = bldr.Query.Trim();

            // Separator
            string s = "";
            if (string.IsNullOrEmpty(query))
            {
                s = "?";
            }
            else if (query != "?")
            {
                s = "&";
            }

            // Name + Value
            name = UrlEncoder.Default.Encode(name);
            value = UrlEncoder.Default.Encode(value);

            // Set the final result
            bldr.Query = $"{query}{s}{name}={value}";
        }
    }
}
