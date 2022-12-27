using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tellma.DefinitionsSynchronizer
{
    public static class SynchronizerUtils
    {
        /// <summary>
        /// In any definition script, if this line is present don't sync what's before it.
        /// </summary>
        const string commonSeparatorBegin = "--<<";

        /// <summary>
        /// In any definition script, if this line is present don't sync what's after it.
        /// </summary>
        const string commonSeparatorEnd = "-->>";

        public static string SyncedScript(string sourceScript, string targetScript)
        {
            sourceScript ??= "";
            targetScript ??= "";

            List<string> syncedLines = new();
            StringReader sourceReader = new(sourceScript);

            while (true)
            {
                string line = sourceReader.ReadLine();
                if (line == null)
                {
                    break; // End of script reached
                }
                else if (line.Trim() == commonSeparatorBegin)
                {
                    syncedLines.Clear(); // Nothing so far should be synced
                }
                else if (line.Trim() == commonSeparatorEnd)
                {
                    break; // Nothing after this should be synced
                }
                else
                {
                    syncedLines.Add(line);
                }
            }

            List<string> beforeLines = new();
            List<string> afterLines = new();
            StringReader targetReader = new(targetScript);

            List<string> acc = new();
            while (true)
            {
                string line = targetReader.ReadLine();
                if (line == null)
                {
                    break; // End of script reached
                }
                else if (line.Trim() == commonSeparatorBegin)
                {
                    beforeLines.AddRange(acc); // Everything before goes in before lines
                }
                else if (line.Trim() == commonSeparatorEnd)
                {
                    acc = afterLines; // Everything afterwards is going in after lines
                }
                else
                {
                    acc.Add(line);
                }
            }

            // Output start + synced + end
            StringBuilder result = new();
            foreach (var line in beforeLines)
                result.AppendLine(line);
            if (beforeLines.Count > 0)
                result.AppendLine(commonSeparatorBegin);
            foreach (var line in syncedLines)
                result.AppendLine(line);
            if (afterLines.Count > 0)
                result.AppendLine(commonSeparatorEnd);
            foreach (var line in afterLines)
                result.AppendLine(line);

            // Return
            return result.ToString()?.Trim();
        }
    }
}
