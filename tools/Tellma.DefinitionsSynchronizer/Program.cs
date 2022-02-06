using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;

namespace Tellma.DefinitionsSynchronizer
{
    class Program
    {
        static object _consoleLock = new();

        static void WriteLine(string s = "", ConsoleColor color = ConsoleColor.White)
        {
            lock (_consoleLock)
            {
                var original = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(s);
                Console.ForegroundColor = original;
            }
        }

        static async Task Main(string[] args)
        {
            // Build the configuration root based on project user secrets
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly)
                .AddCommandLine(args) // Higher precedence
                .Build();

            var opt = config.Get<SynchronizerOptions>();

            var client = new TellmaClient(
                baseUrl: "https://web.tellma.com",
                authorityUrl: "https://web.tellma.com",
                clientId: opt.ClientId,
                clientSecret: opt.ClientSecret);

            // Get the companies
            var masterId = opt.MasterTenantId;
            var tenantIds = new int[] { 100, 101, 102, 200, 201, 202, 203, 301, 399 };

            var dic = new ConcurrentDictionary<int, IReadOnlyList<LineDefinition>>();
            Dictionary<string, LineDefinition> masterLineDefs = null;

            WriteLine($"Loading line definitions for {tenantIds.Length} tenants...");
            await Task.WhenAll(tenantIds.Concat(new List<int> { masterId }).Select(async tenantId =>
            {
                try
                {
                    var result = await client
                        .Application(tenantId)
                        .LineDefinitions
                        .GetEntities(new GetArguments()
                        {
                            Top = int.MaxValue
                        });

                    if (tenantId == masterId)
                    {
                        // Add master definitions
                        masterLineDefs = result
                            .Data
                            .Where(e => !string.IsNullOrWhiteSpace(e.Code))
                            .ToDictionary(e => e.Code);
                    }
                    else
                    {
                        dic.TryAdd(tenantId, result.Data);
                    }
                }
                catch (AuthenticationException)
                {
                    WriteLine($"Tenant {tenantId} failed to authenticate.", ConsoleColor.Red);
                }
                catch (AuthorizationException)
                {
                    WriteLine($"Tenant {tenantId} no permission to access :'(", ConsoleColor.Red);
                }
                catch (Exception ex)
                {
                    WriteLine(ex.Message, ConsoleColor.Red);
                }
            }));

            if (masterLineDefs == null)
            {
                WriteLine($"Failed to load the line definitions from the master catalogue, cannot proceed.", ConsoleColor.Red);
            }
            else
            {
                WriteLine($"Successfully loaded definitions for master catalogue (tenant ID = {masterId}) in addition to {dic.Count} tenants: {string.Join(", ", dic.Keys)}");
                WriteLine();
                WriteLine();

                // For each company check if the definitions are different
                foreach (var tenantId in tenantIds)
                {
                    if (dic.TryGetValue(tenantId, out IReadOnlyList<LineDefinition> lineDefs))
                    {
                        WriteLine($"/////// Tenant {tenantId} Report /////// ");
                        List<string> errorLines = new();
                        int foundInMaster = 0;
                        foreach (var lineDef in lineDefs)
                        {
                            if (!string.IsNullOrWhiteSpace(lineDef.Code) && masterLineDefs.TryGetValue(lineDef.Code, out LineDefinition masterLineDef))
                            {
                                foundInMaster++;

                                // Compare
                                List<string> diffsList = new();
                                if (lineDef.GenerateScript != masterLineDef.GenerateScript)
                                {
                                    diffsList.Add("Generate Script");
                                }

                                if (lineDef.PreprocessScript != masterLineDef.PreprocessScript)
                                {
                                    diffsList.Add("Preprocess Script");
                                }

                                if (lineDef.ValidateScript != masterLineDef.ValidateScript)
                                {
                                    diffsList.Add("Validate Script");
                                }

                                // If there are errors report them
                                if (diffsList.Count > 0)
                                {
                                    string diffs;
                                    if (diffsList.Count == 1)
                                    {
                                        diffs = diffsList[0];
                                    }
                                    else if (diffsList.Count == 2)
                                    {
                                        diffs = $"{diffsList[0]} and {diffsList[1]}";
                                    }
                                    else
                                    {
                                        diffs = string.Join(", ", diffsList.SkipLast(1));
                                        diffs = $"{diffs}, and {diffsList.Last()}";
                                    }


                                    var verb = diffsList.Count > 1 ? "were" : "was";
                                    var outdated = lineDef.SavedAt > masterLineDef.SavedAt ? "Master" : "Tenant";
                                    var latest = lineDef.SavedAt > masterLineDef.SavedAt ? "Tenant" : "Master";

                                    // Tenant modified but not master
                                    errorLines.Add($"\"{lineDef.Code}\" => {diffs} {verb} modified in {latest} but not in {outdated}.");
                                    errorLines.Add($"   Tenant: https://web.tellma.com/app/{tenantId}/line-definitions/{lineDef.Id}");
                                    errorLines.Add($"   Master: https://web.tellma.com/app/{masterId}/line-definitions/{masterLineDef.Id}");
                                    errorLines.Add($""); // New line
                                }
                            }
                        }

                        WriteLine($"Total Line Definitions = {lineDefs.Count}, Found in Master = {foundInMaster}");
                        if (errorLines.Count > 0)
                        {
                            errorLines.ForEach(errorMsg => WriteLine(errorMsg, ConsoleColor.Yellow));
                        }
                        else
                        {
                            WriteLine("All good!", ConsoleColor.Green);
                        }

                        WriteLine();
                    }
                }
            }

            WriteLine();
            WriteLine("Press any key to exit...", ConsoleColor.DarkYellow);
            Console.ReadLine();
        }
    }


    public class SynchronizerOptions
    {
        public int MasterTenantId { get; set; } = 1;
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
