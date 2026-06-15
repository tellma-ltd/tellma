using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace Tellma.Repository.Common.Tests.Queries
{
    public class DocumentDetailsSqlOptimizerTests
    {
        /// <summary>The real captured document-details batch (definition 46, unrestricted user).</summary>
        private static readonly string GeneratedSql = LoadGeneratedSql();

        // The document-scope WHERE that the generator embeds identically in all 14 statements.
        private const string DocScopeWhere = "WHERE (([P].[DefinitionId] = 46)) AND ([P].[Id] = @Param__1)";

        [Fact]
        public void TryOptimize_CapturedBatch_IsOptimized()
        {
            Assert.True(DocumentDetailsSqlOptimizer.TryOptimize(GeneratedSql, out var optimized));

            // Prelude materializes the scope once into temp tables...
            Assert.Contains("INTO #Docs", optimized);
            Assert.Contains("INTO #Lines", optimized);
            Assert.Contains("INTO #Entries", optimized);

            // ...and the giant nested re-derivations are gone, replaced by temp-table reads.
            Assert.Contains("IN (SELECT [Id] FROM #Docs)", optimized);
            Assert.Contains("IN (SELECT [Id] FROM #Lines)", optimized);
            Assert.Contains("IN (SELECT [AccountTypeId] FROM #Entries)", optimized);
            Assert.Contains("IN (SELECT [AccountResourceId] FROM #Entries)", optimized);
            Assert.Contains("IN (SELECT [EntryResourceId] FROM #Entries)", optimized);
            Assert.Contains("IN (SELECT [EntryNotedResourceId] FROM #Entries)", optimized);

            // All 14 result sets are still present and in the same shape (their SELECT lists are verbatim).
            Assert.Equal(14, CountOccurrences(optimized, "ORDER BY"));
            Assert.Contains("[map].[DocumentAssignmentsHistory]", optimized);

            // The captured scope (definition 46) is carried into the #Docs materialization.
            Assert.Contains(DocScopeWhere, optimized);

            // map.Entries() should be evaluated once (the materialization) plus once for the Entries result
            // set — not 7+ times as in the original.
            Assert.Equal(2, CountOccurrences(optimized, "FROM [map].[Entries]()"));
        }

        [Fact]
        public void TryOptimize_DifferentDefinitionId_IsOptimized()
        {
            // A document of a different definition produces the same shape with a different literal.
            string sql = GeneratedSql.Replace("[P].[DefinitionId] = 46", "[P].[DefinitionId] = 50");

            Assert.True(DocumentDetailsSqlOptimizer.TryOptimize(sql, out var optimized));
            Assert.Contains("[P].[DefinitionId] = 50", optimized);
            Assert.DoesNotContain("[P].[DefinitionId] = 46", optimized);
            Assert.Contains("INTO #Entries", optimized);
        }

        [Fact]
        public void TryOptimize_RestrictedUser_CapturesPermissionScopeVerbatim()
        {
            // Simulate a row-level-restricted user: the generator AND-s the permission criteria into the
            // document-scope WHERE and may add a join to that subquery, in every one of the 14 statements.
            const string restrictedWhere =
                "WHERE (([P].[DefinitionId] = 46)) AND ([P].[Id] = @Param__1) AND ([P].[CreatedById] = @P_UserId)";

            string sql = GeneratedSql
                .Replace(
                    "FROM [map].[Documents]() As [P]",
                    "FROM [map].[Documents]() As [P]\r\nLEFT JOIN [map].[Centers]() As [P15] ON [P].[CenterId] = [P15].[Id]")
                .Replace(DocScopeWhere, restrictedWhere);

            Assert.True(DocumentDetailsSqlOptimizer.TryOptimize(sql, out var optimized));

            // The permission predicate, the extra join and the @P_UserId parameter all survive into #Docs.
            Assert.Contains("[P].[CreatedById] = @P_UserId", optimized);
            Assert.Contains("LEFT JOIN [map].[Centers]() As [P15]", optimized);
            Assert.Contains("INTO #Docs", optimized);
        }

        [Fact]
        public void TryOptimize_UnrelatedQuery_IsNotOptimized()
        {
            const string sql =
                "SELECT [P].[Id], [P].[Name]\r\n" +
                "FROM [map].[Agents]() As [P]\r\n" +
                "WHERE [P].[DefinitionId] = 7\r\n" +
                "ORDER BY [P].[Id] ASC\r\n" +
                "OPTION(RECOMPILE) ";

            Assert.False(DocumentDetailsSqlOptimizer.TryOptimize(sql, out var optimized));
            Assert.Null(optimized);
        }

        [Fact]
        public void TryOptimize_DocBatchWithExtraSelectAtom_FallsBackToSlowPath()
        {
            // An extra select atom changes the principal SELECT list -> structural mismatch -> slow path.
            // (Falling back is correct: the optimized template's columns would no longer match hydration.)
            string sql = GeneratedSql.Replace(
                "[P].[Id], [P].[CurrencyId], [P].[CenterId],",
                "[P].[Id], [P].[CurrencyId], [P].[CenterId], [P].[SomeExtraColumn],");

            Assert.False(DocumentDetailsSqlOptimizer.TryOptimize(sql, out var optimized));
            Assert.Null(optimized);
        }

        [Fact]
        public void TryOptimize_Null_ReturnsFalse()
        {
            Assert.False(DocumentDetailsSqlOptimizer.TryOptimize(null, out var optimized));
            Assert.Null(optimized);
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            int count = 0, i = 0;
            while ((i = haystack.IndexOf(needle, i, StringComparison.Ordinal)) >= 0)
            {
                count++;
                i += needle.Length;
            }

            return count;
        }

        private static string LoadGeneratedSql()
        {
            var assembly = typeof(DocumentDetailsSqlOptimizerTests).Assembly;
            string name = Array.Find(
                assembly.GetManifestResourceNames(),
                n => n.EndsWith(".DocumentDetails.generated.sql", StringComparison.Ordinal))
                ?? throw new InvalidOperationException("Test resource 'DocumentDetails.generated.sql' not found.");

            using var stream = assembly.GetManifestResourceStream(name);
            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }
    }
}
