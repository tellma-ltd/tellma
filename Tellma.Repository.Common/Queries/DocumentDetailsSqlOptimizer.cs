using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Surgically swaps the dynamically generated SQL of the document-details screen
    /// (<c>GET /api/documents/{defId}/{docId}?select=$Details</c>) for a hand-optimized,
    /// semantically identical batch that materializes the document/line/entry scope once into
    /// temp tables instead of re-deriving the giant <c>map.Entries()</c> join graph in every
    /// one of the 14 result sets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The swap is gated on an exact (whitespace-normalized) match against a captured template, so it
    /// fires <b>only</b> for the document-details batch and is otherwise a no-op. The optimized template's
    /// 14 outer <c>SELECT</c>/<c>FROM</c>/<c>JOIN</c>/<c>ORDER BY</c> clauses are copied verbatim from the
    /// captured SQL, so the result sets — their order, columns and column order — are byte-for-byte
    /// identical and the C# entity hydration in <see cref="StatementLoader"/> keeps working unchanged.
    /// </para>
    /// <para>
    /// The only part of the batch that varies between users/definitions is the <i>document-scope</i>
    /// subquery (<c>FROM [map].[Documents]() As [P] … WHERE …</c>), which carries the DefinitionId, the id
    /// parameter, <b>and</b> any row-level permission criteria/joins/params. That subquery is captured
    /// verbatim from the principal statement and reused (placeholder <c>{DocScope}</c>), so restricted and
    /// unrestricted users alike get the fast path with identical security semantics.
    /// </para>
    /// <para>
    /// The gate fails <b>closed</b>: any drift (extra select atoms, a user filter, column/join changes, a
    /// bad extraction, a future generator change) simply falls back to the original slow-but-correct SQL —
    /// it can never produce a different result set.
    /// </para>
    /// </remarks>
    public static class DocumentDetailsSqlOptimizer
    {
        /// <summary>The token both templates use in place of the document-scope subquery.</summary>
        private const string Placeholder = "{DocScope}";

        /// <summary>
        /// A table referenced only by the document-details batch. Used as a cheap pre-check to
        /// short-circuit every other query in the system before doing the full (more expensive) gate.
        /// A false positive here is harmless: the gate in <see cref="TryOptimize"/> rejects it.
        /// </summary>
        private const string DocDetailsMarker = "[map].[DocumentLineDefinitionEntries]";

        /// <summary>Anchors the start of the document-scope subquery within the principal statement.</summary>
        private const string DocScopeAnchor = "FROM [map].[Documents]() As [P]";

        private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

        // Templates are embedded resources, loaded and cached once.
        private static readonly Lazy<(string Original, string Optimized)> Templates = new(LoadTemplates);

        /// <summary>
        /// Returns <c>true</c> and an optimized replacement in <paramref name="optimizedSql"/> when
        /// <paramref name="generatedSql"/> is the document-details batch; otherwise returns <c>false</c>
        /// and leaves the caller to run the original SQL unchanged.
        /// </summary>
        public static bool TryOptimize(string generatedSql, out string optimizedSql)
        {
            optimizedSql = null;
            if (string.IsNullOrEmpty(generatedSql))
            {
                return false;
            }

            // (1) Cheap pre-check on a document-specific table. Skips the vast majority of queries fast.
            if (!generatedSql.Contains(DocDetailsMarker, StringComparison.Ordinal))
            {
                return false;
            }

            // (2) Extract {DocScope}: the slice of the principal (first) statement between the
            // "FROM [map].[Documents]() As [P]" anchor and the trailing "ORDER BY" that precedes its
            // first OPTION(RECOMPILE). This captures the DefinitionId + id param + any permission predicate.
            int scopeStart = generatedSql.IndexOf(DocScopeAnchor, StringComparison.Ordinal);
            if (scopeStart < 0)
            {
                return false;
            }

            int firstOption = generatedSql.IndexOf("OPTION(RECOMPILE)", scopeStart, StringComparison.Ordinal);
            if (firstOption < 0)
            {
                return false;
            }

            int orderBy = generatedSql.LastIndexOf("ORDER BY", firstOption, StringComparison.Ordinal);
            if (orderBy <= scopeStart)
            {
                return false;
            }

            string docScope = generatedSql[scopeStart..orderBy].Trim();
            if (!docScope.Contains("WHERE", StringComparison.Ordinal))
            {
                // Not a recognizable document filter; bail to the slow path.
                return false;
            }

            // (3) Gate: reconstruct the expected batch from the captured template + this scope and compare
            // whitespace-normalized. Anything other than an exact structural match falls back to slow SQL.
            var (original, optimized) = Templates.Value;
            string expected = original.Replace(Placeholder, docScope);
            if (!NormalizeEquals(expected, generatedSql))
            {
                return false;
            }

            // (4) Emit the optimized batch with the same captured scope substituted in.
            optimizedSql = optimized.Replace(Placeholder, docScope);
            return true;
        }

        /// <summary>Compares two SQL strings ignoring all runs of whitespace (newlines, indentation).</summary>
        private static bool NormalizeEquals(string a, string b) =>
            string.Equals(Normalize(a), Normalize(b), StringComparison.Ordinal);

        private static string Normalize(string sql) =>
            WhitespaceRegex.Replace(sql, " ").Trim();

        private static (string Original, string Optimized) LoadTemplates()
        {
            var assembly = typeof(DocumentDetailsSqlOptimizer).Assembly;
            return (
                ReadResource(assembly, ".Sql.DocumentDetails.original.sql"),
                ReadResource(assembly, ".Sql.DocumentDetails.optimized.sql"));
        }

        private static string ReadResource(Assembly assembly, string nameSuffix)
        {
            string name = Array.Find(
                assembly.GetManifestResourceNames(),
                n => n.EndsWith(nameSuffix, StringComparison.Ordinal))
                ?? throw new InvalidOperationException(
                    $"Embedded SQL resource '*{nameSuffix}' was not found in '{assembly.GetName().Name}'.");

            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Could not open resource stream '{name}'.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
