using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;

namespace Tellma.DefinitionsSynchronizer
{
    class Program
    {
        /// <summary>
        /// Don't sync the parameters of an auto-generate script that starts with this prefix.
        /// </summary>
        const string ignoreParamsPrefix = "--##";

        static async Task Main(string[] args)
        {
            WriteLine($"Operation started at {DateTime.Now:hh:mm:ss tt}.", ConsoleColor.Cyan);

            #region Options

            // Build the configuration root based on project user secrets
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly)
                .AddCommandLine(args) // Higher precedence
                .Build();

            var opt = config.Get<SynchronizerOptions>();

            // Read the parameters
            int masterId;
            IEnumerable<int> tenantIds;
            IEnumerable<string> definitionCodesParameter;
            {
                // Master Id
                masterId = opt.MasterTenantId ??
                    throw new SynchronizerException($"The {nameof(opt.MasterTenantId)} parameter is required.");

                // Tenant Ids
                tenantIds = (opt.TenantIds ?? "")
                    .Split(",")
                    .Select(e => e.Trim())
                    .Where(e => int.TryParse(e, out _))
                    .Select(int.Parse)
                    .Except(new List<int> { masterId });

                if (!tenantIds.Any())
                {
                    throw new SynchronizerException($"The {nameof(opt.TenantIds)} parameter must contain at least one tenant Id.");
                }

                // Definition Codes
                definitionCodesParameter = (opt.DefinitionCodes ?? "")
                    .Split(",")
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim());

                if (!definitionCodesParameter.Any())
                {
                    throw new SynchronizerException($"The {nameof(opt.DefinitionCodes)} parameter must contain at least one definition code.");
                }
            }

            #endregion

            // Create the Tellma Client
            var client = new TellmaClient(
                baseUrl: "https://web.tellma.com",
                authorityUrl: "https://web.tellma.com",
                clientId: opt.ClientId,
                clientSecret: opt.ClientSecret);

            var masterAndTenantIds = tenantIds.Union(new List<int> { masterId });

            HashSet<string> definitionCodesToSync = null;
            Dictionary<string, int> masterCodeToIdMap = null;
            SettingsForClient masterSettings = null;
            DefinitionsForClient masterDefs = null;
            var tenantCodeToIdMaps = new ConcurrentDictionary<int, Dictionary<string, int>>();

            WriteLine($"Loading line definitions from {masterAndTenantIds.Count()} tenants...");
            await Task.WhenAll(masterAndTenantIds.Select(async tenantId =>
            {
                try
                {
                    // Get Id and Code for all line definitions
                    var result = await client.Application(tenantId)
                        .LineDefinitions
                        .GetEntities(new GetArguments()
                        {
                            Select = nameof(LineDefinition.Code),
                            Filter = "Code != 'ManualLine'",
                            Top = int.MaxValue
                        });

                    if (tenantId == masterId)
                    {
                        // Also get the 
                        masterSettings = (await client.Application(tenantId)
                            .GeneralSettings.SettingsForClient()).Data;

                        masterDefs = (await client.Application(tenantId)
                            .Definitions.DefinitionsForClient()).Data;

                        // Add master definitions
                        masterCodeToIdMap = result
                            .Data
                            .Where(e => !string.IsNullOrWhiteSpace(e.Code))
                            .ToDictionary(e => e.Code, e => e.Id, StringComparer.OrdinalIgnoreCase);

                        if (definitionCodesParameter.Any(e => e.ToLower() == "all"))
                        {
                            definitionCodesToSync = masterCodeToIdMap
                                .Keys
                                .ToHashSet();
                        }
                        else
                        {
                            definitionCodesToSync = definitionCodesParameter
                                .ToHashSet();

                            // Make sure all supplied definitions are found in master
                            foreach (var defCode in definitionCodesToSync)
                            {
                                if (!masterCodeToIdMap.ContainsKey(defCode))
                                {
                                    throw new SynchronizerException(
                                        $"Could not find definition code '{defCode}' in master catalog (tenant ID = {masterId}).");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Add tenant definitions
                        tenantCodeToIdMaps.TryAdd(tenantId,
                            result.Data
                            .Where(e => !string.IsNullOrWhiteSpace(e.Code))
                            .ToDictionary(e => e.Code, e => e.Id, StringComparer.OrdinalIgnoreCase));
                    }
                }
                catch (AuthenticationException)
                {
                    WriteLine($"Tenant {tenantId} failed to authenticate.", ConsoleColor.Red);
                }
                catch (AuthorizationException)
                {
                    WriteLine($"Need Read permissions on Line Definitions in TenantId = {tenantId} :'(", ConsoleColor.Red);
                }
                catch (Exception ex)
                {
                    WriteLine(ex.Message, ConsoleColor.Red);
                }
            }));

            if (masterSettings == null)
            {
                WriteLine($"Failed to load master (TenantId = {masterId}) settings, cannot proceed.", ConsoleColor.Red);
                return;
            }

            if (masterCodeToIdMap == null)
            {
                WriteLine($"Failed to load the line definitions from the master (TenantId = {masterId}), cannot proceed.", ConsoleColor.Red);
                return;
            }

            WriteLine($"Successfully loaded definitions for master (tenant ID = {masterId}) in addition to {tenantCodeToIdMaps.Count} tenant(s): {string.Join(", ", tenantCodeToIdMaps.Keys)}");

            #region Confirmation

            if (!opt.SkipConfirmation)
            {
                string confirmed = "Confirmed";

                WriteLine();
                Write($"Confirm synchronizing {(definitionCodesParameter.Any(e => e.ToLower() == "all") ? "all" : definitionCodesParameter.Count())} Line Definition(s) by typing ");
                Write($"\"{confirmed}\"");
                Write($": ");

                var answer = Console.ReadLine();
                if (answer.ToLower() != confirmed.ToLower())
                {
                    Write($"X", ConsoleColor.Red);
                    Write($" Did not confirm");
                    WriteLine();

                    return;
                }
                else
                {
                    Write($"\u2713 ", ConsoleColor.Green);
                    Write("Confirmation acquired.");
                    WriteLine();
                }
            }

            #endregion

            // For each company check if the definitions are different

            WriteLine();
            WriteLine();

            ConcurrentDictionary<string, Task<LineDefinitionForSave>> masterDefinitions = new(StringComparer.OrdinalIgnoreCase);
            ConcurrentDictionary<int, TenantReport> reports = new();

            ConcurrentDictionary<int, Task<string>> masterAccountTypeMap = new();
            ConcurrentDictionary<int, Task<string>> masterEntryTypeMap = new();
            ConcurrentDictionary<int, Task<string>> masterDocumentDefinitionMap = new();
            ConcurrentDictionary<int, Task<string>> masterAgentDefinitionMap = new();
            ConcurrentDictionary<int, Task<string>> masterResourceDefinitionMap = new();
            ConcurrentDictionary<int, Task<string>> masterLookupDefinitionMap = new();

            // Do all tenants in parallel
            await Task.WhenAll(tenantCodeToIdMaps.Select(async pair =>
            {
                var tenantId = pair.Key;
                var tenantCodeToIdMap = pair.Value;

                TenantReport report = new(); // Will contain all the results that we will print at the end.
                reports.TryAdd(tenantId, report);

                SettingsForClient tenantSettings;
                try
                {
                    tenantSettings = (await client
                        .Application(tenantId)
                        .GeneralSettings
                        .SettingsForClient()).Data;

                    // Report that the tenant has failed
                }
                catch (Exception ex)
                {
                    report.Errors.Add($"Failed to load tenant settings: {ex.Message}");
                    return;
                }

                #region Id Mapper Functions

                // Map concepts to ids
                ConcurrentDictionary<string, Task<int>> tenantAccountTypeMap = new(StringComparer.OrdinalIgnoreCase);
                ConcurrentDictionary<string, Task<int>> tenantEntryTypeMap = new(StringComparer.OrdinalIgnoreCase);
                ConcurrentDictionary<string, Task<int>> tenantDocumentDefinitionMap = new(StringComparer.OrdinalIgnoreCase);
                ConcurrentDictionary<string, Task<int>> tenantAgentDefinitionMap = new(StringComparer.OrdinalIgnoreCase);
                ConcurrentDictionary<string, Task<int>> tenantResourceDefinitionMap = new(StringComparer.OrdinalIgnoreCase);
                ConcurrentDictionary<string, Task<int>> tenantLookupDefinitionMap = new(StringComparer.OrdinalIgnoreCase);


                async Task<int?> GetTenantAccountTypeId(int? idInMaster)
                {
                    if (idInMaster == null)
                        return null;

                    string concept = await masterAccountTypeMap
                        .GetOrAdd(idInMaster.Value, async id =>
                        {
                            var result = await client
                                .Application(masterId)
                                .AccountTypes
                                .GetById(id, new GetByIdArguments
                                {
                                    Select = nameof(AccountType.Concept),
                                });
                            return result.Entity.Concept;
                        });

                    return await tenantAccountTypeMap.GetOrAdd(concept, async c =>
                    {
                        var response = await client
                           .Application(tenantId)
                           .AccountTypes
                           .GetEntities(new GetArguments
                           {
                               Select = nameof(AccountType.Id),
                               Filter = nameof(AccountType.Concept) + $" eq '{c.Replace("'", "''")}'",
                               Top = 1,
                               CountEntities = true,
                           });

                        if (response.Count == 0)
                        {
                            throw new SynchronizerException($"Did not find an {nameof(AccountType)} in tenant ID = {tenantId} where concept = '{concept}'");
                        }
                        else if (response.Count > 1)
                        {
                            throw new SynchronizerException($"Found more than one {nameof(AccountType)} in tenant ID = {tenantId} where concept = '{concept}'");
                        }
                        else
                        {
                            return response.Data[0].Id;
                        }
                    });
                }

                async Task<int?> GetTenantEntryTypeId(int? idInMaster)
                {
                    if (idInMaster == null)
                        return null;

                    string concept = await masterEntryTypeMap
                        .GetOrAdd(idInMaster.Value, async id =>
                        {
                            var result = await client
                                .Application(masterId)
                                .EntryTypes
                                .GetById(id, new GetByIdArguments
                                {
                                    Select = nameof(EntryType.Concept),
                                });
                            return result.Entity.Concept;
                        });

                    return await tenantEntryTypeMap.GetOrAdd(concept, async c =>
                    {
                        var response = await client
                           .Application(tenantId)
                           .EntryTypes
                           .GetEntities(new GetArguments
                           {
                               Select = nameof(EntryType.Id),
                               Filter = nameof(EntryType.Concept) + $" eq '{c.Replace("'", "''")}'",
                               Top = 1,
                               CountEntities = true,
                           });

                        if (response.Count == 0)
                        {
                            throw new SynchronizerException($"Did not find an {nameof(EntryType)} in tenant ID = {tenantId} where concept = '{c}'");
                        }
                        else if (response.Count > 1)
                        {
                            throw new SynchronizerException($"Found more than one {nameof(EntryType)} in tenant ID = {tenantId} where concept = '{c}'");
                        }
                        else
                        {
                            return response.Data[0].Id;
                        }
                    });
                }

                async Task<int?> GetTenantDocumentId(int? idInMaster)
                {
                    if (idInMaster == null)
                        return null;

                    string code = await masterDocumentDefinitionMap
                        .GetOrAdd(idInMaster.Value, async id =>
                        {
                            var result = await client
                                .Application(masterId)
                                .DocumentDefinitions
                                .GetById(id, new GetByIdArguments
                                {
                                    Select = nameof(DocumentDefinition.Code),
                                });
                            return result.Entity.Code;
                        });

                    return await tenantDocumentDefinitionMap.GetOrAdd(code, async c =>
                    {
                        var response = await client
                           .Application(tenantId)
                           .DocumentDefinitions
                           .GetEntities(new GetArguments
                           {
                               Select = nameof(DocumentDefinition.Id),
                               Filter = nameof(DocumentDefinition.Code) + $" eq '{c.Replace("'", "''")}'",
                               Top = 1,
                               CountEntities = true,
                           });

                        if (response.Count == 0)
                        {
                            throw new SynchronizerException($"Did not find an {nameof(DocumentDefinition)} in tenant ID = {tenantId} where code = '{code}'");
                        }
                        else if (response.Count > 1)
                        {
                            throw new SynchronizerException($"Found more than one {nameof(DocumentDefinition)} in tenant ID = {tenantId} where code = '{code}'");
                        }
                        else
                        {
                            return response.Data[0].Id;
                        }
                    });
                }

                async Task<int?> GetTenantAgentId(int? idInMaster)
                {
                    if (idInMaster == null)
                        return null;

                    string code = await masterAgentDefinitionMap
                        .GetOrAdd(idInMaster.Value, async id =>
                        {
                            var result = await client
                                .Application(masterId)
                                .AgentDefinitions
                                .GetById(id, new GetByIdArguments
                                {
                                    Select = nameof(AgentDefinition.Code),
                                });
                            return result.Entity.Code;
                        });

                    return await tenantAgentDefinitionMap.GetOrAdd(code, async c =>
                    {
                        var response = await client
                           .Application(tenantId)
                           .AgentDefinitions
                           .GetEntities(new GetArguments
                           {
                               Select = nameof(AgentDefinition.Id),
                               Filter = nameof(AgentDefinition.Code) + $" eq '{c.Replace("'", "''")}'",
                               Top = 1,
                               CountEntities = true,
                           });

                        if (response.Count == 0)
                        {
                            throw new SynchronizerException($"Did not find an {nameof(AgentDefinition)} in tenant ID = {tenantId} where code = '{code}'");
                        }
                        else if (response.Count > 1)
                        {
                            throw new SynchronizerException($"Found more than one {nameof(AgentDefinition)} in tenant ID = {tenantId} where code = '{code}'");
                        }
                        else
                        {
                            return response.Data[0].Id;
                        }
                    });
                }

                async Task<int?> GetTenantResourceId(int? idInMaster)
                {
                    if (idInMaster == null)
                        return null;

                    string code = await masterResourceDefinitionMap
                        .GetOrAdd(idInMaster.Value, async id =>
                        {
                            var result = await client
                                .Application(masterId)
                                .ResourceDefinitions
                                .GetById(id, new GetByIdArguments
                                {
                                    Select = nameof(ResourceDefinition.Code),
                                });
                            return result.Entity.Code;
                        });

                    return await tenantResourceDefinitionMap.GetOrAdd(code, async c =>
                    {
                        var response = await client
                           .Application(tenantId)
                           .ResourceDefinitions
                           .GetEntities(new GetArguments
                           {
                               Select = nameof(ResourceDefinition.Id),
                               Filter = nameof(ResourceDefinition.Code) + $" eq '{c.Replace("'", "''")}'",
                               Top = 1,
                               CountEntities = true,
                           });

                        if (response.Count == 0)
                        {
                            throw new SynchronizerException($"Did not find an {nameof(ResourceDefinition)} in tenant ID = {tenantId} where code = '{code}'");
                        }
                        else if (response.Count > 1)
                        {
                            throw new SynchronizerException($"Found more than one {nameof(ResourceDefinition)} in tenant ID = {tenantId} where code = '{code}'");
                        }
                        else
                        {
                            return response.Data[0].Id;
                        }
                    });
                }

                async Task<int?> GetTenantLookupId(int? idInMaster)
                {
                    if (idInMaster == null)
                        return null;

                    string code = await masterLookupDefinitionMap
                        .GetOrAdd(idInMaster.Value, async id =>
                        {
                            var result = await client
                                .Application(masterId)
                                .LookupDefinitions
                                .GetById(id, new GetByIdArguments
                                {
                                    Select = nameof(LookupDefinition.Code),
                                });
                            return result.Entity.Code;
                        });

                    return await tenantLookupDefinitionMap.GetOrAdd(code, async c =>
                    {
                        var response = await client
                           .Application(tenantId)
                           .LookupDefinitions
                           .GetEntities(new GetArguments
                           {
                               Select = nameof(LookupDefinition.Id),
                               Filter = nameof(LookupDefinition.Code) + $" eq '{c.Replace("'", "''")}'",
                               Top = 1,
                               CountEntities = true,
                           });

                        if (response.Count == 0)
                        {
                            throw new SynchronizerException($"Did not find an {nameof(LookupDefinition)} in tenant ID = {tenantId} where code = '{code}'");
                        }
                        else if (response.Count > 1)
                        {
                            throw new SynchronizerException($"Found more than one {nameof(LookupDefinition)} in tenant ID = {tenantId} where code = '{code}'");
                        }
                        else
                        {
                            return response.Data[0].Id;
                        }
                    });
                }

                #endregion

                foreach (var defCode in definitionCodesToSync)
                {
                    if (tenantCodeToIdMap.TryGetValue(defCode, out int tenantDefId))
                    {
                        // WriteLine($"Started syncing {tenantId}: '{defCode}'");
                        report.TotalDefs++;

                        int masterDefId = masterCodeToIdMap[defCode];

                        LineDefinitionForSave masterDef = null;
                        try
                        {
                            masterDef = await masterDefinitions.GetOrAdd(defCode, (defCode) =>
                                client.Application(masterId).LineDefinitions.GetByIdForSave(masterDefId));
                        }
                        catch (AuthorizationException)
                        {
                            report.Errors.Add($"Need Read permissions on Line Definitions in TenantId = {masterId}");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            report.Errors.Add(ex.Message);
                            continue;
                        }

                        LineDefinitionForSave tenantDef = null;
                        try
                        {
                            tenantDef = await client.Application(tenantId).LineDefinitions.GetByIdForSave(tenantDefId);
                        }
                        catch (AuthorizationException)
                        {
                            report.Errors.Add($"Need Read permissions on Line Definitions in TenantId = {tenantId}");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            report.Errors.Add(ex.Message);
                            continue;
                        }

                        HashSet<string> defErrors = new();
                        report.DefinitionsErrors.Add(defCode, defErrors);

                        // Sync
                        tenantDef.LineType = masterDef.LineType;

                        if (TryGetString(masterSettings, tenantSettings.PrimaryLanguageId, masterDef.Description, masterDef.Description2, masterDef.Description3, out string res))
                            tenantDef.Description = res;
                        if (TryGetString(masterSettings, tenantSettings.SecondaryLanguageId, masterDef.Description, masterDef.Description2, masterDef.Description3, out res))
                            tenantDef.Description2 = res;
                        if (TryGetString(masterSettings, tenantSettings.TernaryLanguageId, masterDef.Description, masterDef.Description2, masterDef.Description3, out res))
                            tenantDef.Description3 = res;

                        if (TryGetString(masterSettings, tenantSettings.PrimaryLanguageId, masterDef.TitleSingular, masterDef.TitleSingular2, masterDef.TitleSingular3, out res))
                            tenantDef.TitleSingular = res;
                        if (TryGetString(masterSettings, tenantSettings.SecondaryLanguageId, masterDef.TitleSingular, masterDef.TitleSingular2, masterDef.TitleSingular3, out res))
                            tenantDef.TitleSingular2 = res;
                        if (TryGetString(masterSettings, tenantSettings.TernaryLanguageId, masterDef.TitleSingular, masterDef.TitleSingular2, masterDef.TitleSingular3, out res))
                            tenantDef.TitleSingular3 = res;

                        if (TryGetString(masterSettings, tenantSettings.PrimaryLanguageId, masterDef.TitlePlural, masterDef.TitlePlural2, masterDef.TitlePlural3, out res))
                            tenantDef.TitlePlural = res;
                        if (TryGetString(masterSettings, tenantSettings.SecondaryLanguageId, masterDef.TitlePlural, masterDef.TitlePlural2, masterDef.TitlePlural3, out res))
                            tenantDef.TitlePlural2 = res;
                        if (TryGetString(masterSettings, tenantSettings.TernaryLanguageId, masterDef.TitlePlural, masterDef.TitlePlural2, masterDef.TitlePlural3, out res))
                            tenantDef.TitlePlural3 = res;

                        tenantDef.AllowSelectiveSigning = masterDef.AllowSelectiveSigning;
                        tenantDef.ViewDefaultsToForm = masterDef.ViewDefaultsToForm;
                        tenantDef.BarcodeColumnIndex = masterDef.BarcodeColumnIndex;
                        tenantDef.BarcodeProperty = masterDef.BarcodeProperty;
                        tenantDef.BarcodeExistingItemHandling = masterDef.BarcodeExistingItemHandling;
                        tenantDef.BarcodeBeepsEnabled = masterDef.BarcodeBeepsEnabled;

                        if (TryGetString(masterSettings, tenantSettings.PrimaryLanguageId, masterDef.GenerateLabel, masterDef.GenerateLabel2, masterDef.GenerateLabel3, out res))
                            tenantDef.GenerateLabel = res;
                        if (TryGetString(masterSettings, tenantSettings.SecondaryLanguageId, masterDef.GenerateLabel, masterDef.GenerateLabel2, masterDef.GenerateLabel3, out res))
                            tenantDef.GenerateLabel2 = res;
                        if (TryGetString(masterSettings, tenantSettings.TernaryLanguageId, masterDef.GenerateLabel, masterDef.GenerateLabel2, masterDef.GenerateLabel3, out res))
                            tenantDef.GenerateLabel3 = res;

                        tenantDef.GenerateScript = SynchronizerUtils.SyncedScript(masterDef.GenerateScript, tenantDef.GenerateScript);
                        tenantDef.PreprocessScript = SynchronizerUtils.SyncedScript(masterDef.PreprocessScript, tenantDef.PreprocessScript);
                        tenantDef.ValidateScript = SynchronizerUtils.SyncedScript(masterDef.ValidateScript, tenantDef.ValidateScript);
                        tenantDef.SignValidateScript = SynchronizerUtils.SyncedScript(masterDef.SignValidateScript, tenantDef.SignValidateScript);
                        tenantDef.UnsignValidateScript = SynchronizerUtils.SyncedScript(masterDef.UnsignValidateScript, tenantDef.UnsignValidateScript);

                        // Entries
                        MatchCount(tenantDef.Entries, masterDef.Entries.Count);
                        for (int i = 0; i < tenantDef.Entries.Count; i++)
                        {
                            var masterEntry = masterDef.Entries[i];
                            var tenantEntry = tenantDef.Entries[i];

                            tenantEntry.Direction = masterEntry.Direction;

                            // Account Type
                            try
                            {
                                tenantEntry.ParentAccountTypeId = await GetTenantAccountTypeId(masterEntry.ParentAccountTypeId);
                            }
                            catch (AuthorizationException)
                            {
                                defErrors.Add($"Need Read permissions on Account Types");
                                continue;
                            }
                            catch (Exception ex)
                            {
                                defErrors.Add(ex.Message);
                                continue;
                            }

                            // Entry Type
                            try
                            {
                                tenantEntry.EntryTypeId = await GetTenantEntryTypeId(masterEntry.EntryTypeId);
                            }
                            catch (AuthorizationException)
                            {
                                defErrors.Add($"Need Read permissions on Entry Types");
                                continue;
                            }
                            catch (Exception ex)
                            {
                                defErrors.Add(ex.Message);
                                continue;
                            }

                            // Agent definitions
                            try
                            {
                                MatchCount(tenantEntry.AgentDefinitions, masterEntry.AgentDefinitions.Count);
                                for (int j = 0; j < tenantEntry.AgentDefinitions.Count; j++)
                                {
                                    var masterRow = masterEntry.AgentDefinitions[j];
                                    var tenantRow = tenantEntry.AgentDefinitions[j];

                                    tenantRow.AgentDefinitionId = await GetTenantAgentId(masterRow.AgentDefinitionId);
                                }
                            }
                            catch (Exception ex)
                            {
                                defErrors.Add(ex.Message);
                                continue;
                            }

                            // Resource definitions
                            try
                            {
                                MatchCount(tenantEntry.ResourceDefinitions, masterEntry.ResourceDefinitions.Count);
                                for (int j = 0; j < tenantEntry.ResourceDefinitions.Count; j++)
                                {
                                    var masterRow = masterEntry.ResourceDefinitions[j];
                                    var tenantRow = tenantEntry.ResourceDefinitions[j];

                                    tenantRow.ResourceDefinitionId = await GetTenantResourceId(masterRow.ResourceDefinitionId);
                                }
                            }
                            catch (Exception ex)
                            {
                                defErrors.Add(ex.Message);
                                continue;
                            }

                            // Noted Agent definitions
                            try
                            {
                                MatchCount(tenantEntry.NotedAgentDefinitions, masterEntry.NotedAgentDefinitions.Count);
                                for (int j = 0; j < tenantEntry.NotedAgentDefinitions.Count; j++)
                                {
                                    var masterRow = masterEntry.NotedAgentDefinitions[j];
                                    var tenantRow = tenantEntry.NotedAgentDefinitions[j];

                                    tenantRow.NotedAgentDefinitionId = await GetTenantAgentId(masterRow.NotedAgentDefinitionId);
                                }
                            }
                            catch (Exception ex)
                            {
                                defErrors.Add(ex.Message);
                                continue;
                            }

                            // Noted Resource definitions
                            try
                            {
                                MatchCount(tenantEntry.NotedResourceDefinitions, masterEntry.NotedResourceDefinitions.Count);
                                for (int j = 0; j < tenantEntry.NotedResourceDefinitions.Count; j++)
                                {
                                    var masterRow = masterEntry.NotedResourceDefinitions[j];
                                    var tenantRow = tenantEntry.NotedResourceDefinitions[j];

                                    tenantRow.NotedResourceDefinitionId = await GetTenantResourceId(masterRow.NotedResourceDefinitionId);
                                }
                            }
                            catch (Exception ex)
                            {
                                defErrors.Add(ex.Message);
                                continue;
                            }
                        }

                        // Columns
                        {
                            var secondaryTranslations = tenantDef.Columns.GroupBy(c => c.Label).ToDictionary(g => g.Key, g => g.First().Label2, StringComparer.OrdinalIgnoreCase);
                            var ternaryTranslations = tenantDef.Columns.GroupBy(c => c.Label).ToDictionary(g => g.Key, g => g.First().Label3, StringComparer.OrdinalIgnoreCase);
                            MatchCount(tenantDef.Columns, masterDef.Columns.Count);
                            for (int i = 0; i < masterDef.Columns.Count; i++)
                            {
                                var masterColumn = masterDef.Columns[i];
                                var tenantColumn = tenantDef.Columns[i];

                                tenantColumn.ColumnName = masterColumn.ColumnName;
                                tenantColumn.EntryIndex = masterColumn.EntryIndex;

                                if (TryGetString(masterSettings, tenantSettings.PrimaryLanguageId, masterColumn.Label, masterColumn.Label2, masterColumn.Label3, out res))
                                    tenantColumn.Label = res;

                                if (TryGetString(masterSettings, tenantSettings.SecondaryLanguageId, masterColumn.Label, masterColumn.Label2, masterColumn.Label3, out res))
                                    tenantColumn.Label2 = res;
                                else if (secondaryTranslations.TryGetValue(tenantColumn.Label, out res)) // In case the rows are re-ordered 
                                    tenantColumn.Label2 = res;

                                if (TryGetString(masterSettings, tenantSettings.TernaryLanguageId, masterColumn.Label, masterColumn.Label2, masterColumn.Label3, out res))
                                    tenantColumn.Label3 = res;
                                else if (ternaryTranslations.TryGetValue(tenantColumn.Label, out res)) // In case the rows are re-ordered 
                                    tenantColumn.Label3 = res;

                                tenantColumn.Filter = masterColumn.Filter;
                                tenantColumn.InheritsFromHeader = masterColumn.InheritsFromHeader;
                                tenantColumn.VisibleState = masterColumn.VisibleState;
                                tenantColumn.RequiredState = masterColumn.RequiredState;
                                tenantColumn.ReadOnlyState = masterColumn.ReadOnlyState;
                            }
                        }

                        // State Reasons
                        {
                            var secondaryTranslations = tenantDef.StateReasons.GroupBy(c => c.Name).ToDictionary(g => g.Key, g => g.First().Name2, StringComparer.OrdinalIgnoreCase);
                            var ternaryTranslations = tenantDef.StateReasons.GroupBy(c => c.Name).ToDictionary(g => g.Key, g => g.First().Name3, StringComparer.OrdinalIgnoreCase);
                            MatchCount(tenantDef.StateReasons, masterDef.StateReasons.Count);
                            for (int i = 0; i < masterDef.StateReasons.Count; i++)
                            {
                                var masterStateReason = masterDef.StateReasons[i];
                                var tenantStateReason = tenantDef.StateReasons[i];

                                tenantStateReason.State = masterStateReason.State;
                                tenantStateReason.IsActive = masterStateReason.IsActive;

                                if (TryGetString(masterSettings, tenantSettings.PrimaryLanguageId, masterStateReason.Name, masterStateReason.Name2, masterStateReason.Name3, out res))
                                    tenantStateReason.Name = res;

                                if (TryGetString(masterSettings, tenantSettings.SecondaryLanguageId, masterStateReason.Name, masterStateReason.Name2, masterStateReason.Name3, out res))
                                    tenantStateReason.Name2 = res;
                                else if (secondaryTranslations.TryGetValue(tenantStateReason.Name, out res)) // In case the rows are re-ordered 
                                    tenantStateReason.Name2 = res;

                                if (TryGetString(masterSettings, tenantSettings.TernaryLanguageId, masterStateReason.Name, masterStateReason.Name2, masterStateReason.Name3, out res))
                                    tenantStateReason.Name3 = res;
                                else if (ternaryTranslations.TryGetValue(tenantStateReason.Name, out res)) // In case the rows are re-ordered 
                                    tenantStateReason.Name3 = res;
                            }
                        }

                        // Generate Parameters
                        if (!(tenantDef.GenerateScript ?? "").Trim().StartsWith(ignoreParamsPrefix))
                        {
                            // Collect translations
                            var secondaryTranslations = tenantDef.GenerateParameters.GroupBy(c => c.Label).ToDictionary(g => g.Key, g => g.First().Label2, StringComparer.OrdinalIgnoreCase);
                            var ternaryTranslations = tenantDef.GenerateParameters.GroupBy(c => c.Label).ToDictionary(g => g.Key, g => g.First().Label3, StringComparer.OrdinalIgnoreCase);

                            var choices = tenantDef.GenerateParameters
                                .Select(e => Api.ControlOptionsUtil.Deserialize(e.Control, e.ControlOptions))
                                .Where(e => e is Api.ChoiceControlOptions)
                                .Cast<Api.ChoiceControlOptions>()
                                .SelectMany(e => e.choices);

                            var secondaryTranslationsForChoices = choices.GroupBy(e => e.name).ToDictionary(g => g.Key, g => g.First().name2, StringComparer.OrdinalIgnoreCase);
                            var ternaryTranslationsForChoices = choices.GroupBy(e => e.name).ToDictionary(g => g.Key, g => g.First().name3, StringComparer.OrdinalIgnoreCase);

                            MatchCount(tenantDef.GenerateParameters, masterDef.GenerateParameters.Count);
                            for (int i = 0; i < masterDef.GenerateParameters.Count; i++)
                            {
                                var masterGenerateParam = masterDef.GenerateParameters[i];
                                var tenantGenerateParam = tenantDef.GenerateParameters[i];

                                tenantGenerateParam.Key = masterGenerateParam.Key;

                                if (TryGetString(masterSettings, tenantSettings.PrimaryLanguageId, masterGenerateParam.Label, masterGenerateParam.Label2, masterGenerateParam.Label3, out res))
                                    tenantGenerateParam.Label = res;

                                if (TryGetString(masterSettings, tenantSettings.SecondaryLanguageId, masterGenerateParam.Label, masterGenerateParam.Label2, masterGenerateParam.Label3, out res))
                                    tenantGenerateParam.Label2 = res;
                                else if (secondaryTranslations.TryGetValue(tenantGenerateParam.Label, out res)) // In case the rows are re-ordered 
                                    tenantGenerateParam.Label2 = res;

                                if (TryGetString(masterSettings, tenantSettings.TernaryLanguageId, masterGenerateParam.Label, masterGenerateParam.Label2, masterGenerateParam.Label3, out res))
                                    tenantGenerateParam.Label3 = res;
                                else if (ternaryTranslations.TryGetValue(tenantGenerateParam.Label, out res)) // In case the rows are re-ordered 
                                    tenantGenerateParam.Label3 = res;

                                tenantGenerateParam.Visibility = masterGenerateParam.Visibility;
                                tenantGenerateParam.Control = masterGenerateParam.Control;

                                var ctrlOptions = Api.ControlOptionsUtil.Deserialize(masterGenerateParam.Control, masterGenerateParam.ControlOptions);
                                if (ctrlOptions is Api.ChoiceControlOptions tenantChoiceOptions)
                                {
                                    var masterChoiceOptions = (Api.ChoiceControlOptions)Api.ControlOptionsUtil.Deserialize(masterGenerateParam.Control, masterGenerateParam.ControlOptions);
                                    for (int k = 0; k < tenantChoiceOptions.choices.Count; k++)
                                    {
                                        var masterChoice = masterChoiceOptions.choices[k];
                                        var tenantChoice = tenantChoiceOptions.choices[k];

                                        if (TryGetString(masterSettings, tenantSettings.PrimaryLanguageId, masterChoice.name, masterChoice.name2, masterChoice.name3, out res))
                                            tenantChoice.name = res;

                                        if (TryGetString(masterSettings, tenantSettings.SecondaryLanguageId, masterChoice.name, masterChoice.name2, masterChoice.name3, out res))
                                            tenantChoice.name2 = res;
                                        else if (secondaryTranslations.TryGetValue(tenantChoice.name, out res)) // In case the rows are re-ordered 
                                            tenantChoice.name2 = res;

                                        if (TryGetString(masterSettings, tenantSettings.TernaryLanguageId, masterChoice.name, masterChoice.name2, masterChoice.name3, out res))
                                            tenantChoice.name3 = res;
                                        else if (ternaryTranslations.TryGetValue(tenantChoice.name, out res)) // In case the rows are re-ordered 
                                            tenantChoice.name3 = res;
                                    }
                                }
                                if (ctrlOptions is Api.NavigationControlOptions navOptions)
                                {
                                    try
                                    {
                                        switch (masterGenerateParam.Control)
                                        {
                                            case nameof(Document):
                                                navOptions.definitionId = await GetTenantDocumentId(navOptions.definitionId);
                                                break;
                                            case nameof(Agent):
                                                navOptions.definitionId = await GetTenantAgentId(navOptions.definitionId);
                                                break;
                                            case nameof(Resource):
                                                navOptions.definitionId = await GetTenantResourceId(navOptions.definitionId);
                                                break;
                                            case nameof(Lookup):
                                                navOptions.definitionId = await GetTenantLookupId(navOptions.definitionId);
                                                break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        defErrors.Add(ex.Message);
                                        continue;
                                    }
                                }
                                tenantGenerateParam.ControlOptions = Api.ControlOptionsUtil.Serialize(ctrlOptions);
                            }
                        }

                        // Saving the result back
                        if (defErrors.Count > 0)
                        {
                            WriteLine($"Failure syncing {tenantId}: '{defCode}'", ConsoleColor.Red);
                        }
                        else
                        {
                            try
                            {
                                // Save it
                                await client.Application(tenantId).LineDefinitions.Save(new() { tenantDef });
                                report.SyncedDefs++;
                                WriteLine($"Success syncing {tenantId}: '{defCode}'", ConsoleColor.Green);
                            }
                            catch (AuthorizationException)
                            {
                                defErrors.Add($"Need Update permissions on Line Definitions");
                            }
                            catch (ValidationException ex)
                            {
                                foreach (var errorGroup in ex.Errors)
                                {
                                    foreach (var error in errorGroup.Value)
                                    {
                                        defErrors.Add($"{errorGroup.Key}: {error}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                defErrors.Add($"Error during save: {ex.Message}");
                            }
                        }
                    }
                }
            }));

            #region Report Errors

            WriteLine();
            WriteLine("Syncing complete, generating sync report...", ConsoleColor.Cyan);
            WriteLine();
            WriteLine();

            // Report successes
            foreach (var (tenantId, report) in reports.OrderBy(e => e.Value.IsError).ThenBy(e => e.Key))
            {
                WriteLine($"////////////////// TenantId = {tenantId}");
                if (!report.IsError)
                {
                    if (report.TotalDefs > 0)
                    {
                        WriteLine($"Successfully synced {report.SyncedDefs}/{report.TotalDefs} line definitions.", ConsoleColor.Green);
                    }
                    else
                    {
                        WriteLine($"Nothing to sync.", ConsoleColor.Green);
                    }
                }
                else if (report.Errors.Any())
                {
                    WriteLine($"Failed to sync line definitions:", ConsoleColor.DarkYellow);
                    foreach (var error in report.Errors)
                    {
                        WriteLine($"  - {error}", ConsoleColor.Red);
                    }
                }
                else
                {
                    // Partial success
                    WriteLine($"Synced {report.SyncedDefs} line definitions, the remaining {report.TotalDefs - report.SyncedDefs} line definitions encountered some errors:", ConsoleColor.DarkYellow);
                    foreach (var defErrors in report.DefinitionsErrors)
                    {
                        if (defErrors.Value.Any())
                        {
                            WriteLine($"  - {defErrors.Key}:", ConsoleColor.DarkYellow);
                            foreach (var error in defErrors.Value)
                            {
                                WriteLine($"    - {error}", ConsoleColor.Red);
                            }
                        }
                    }
                }

                WriteLine();
            }

            #endregion

            WriteLine();
            WriteLine($"Operation completed at {DateTime.Now:hh:mm:ss tt}.", ConsoleColor.Cyan);
            WriteLine();
            WriteLine("Press any key to exit...", ConsoleColor.DarkYellow);
            Console.ReadLine();
        }

        static void MatchCount<T>(List<T> list, int count) where T : new()
        {
            while (list.Count > count)
            {
                list.RemoveAt(list.Count - 1);
            }
            while (list.Count < count)
            {
                list.Add(new());
            }
        }

        /// <summary>
        /// Returns true if we should update the target with the result, false otherwise.
        /// </summary>
        static bool TryGetString(
            SettingsForClient sourceSettings,
            string targetLangId,
            string sourceS1,
            string sourceS2,
            string sourceS3,
            out string result)
        {
            if (string.IsNullOrWhiteSpace(targetLangId))
            {
                result = null;
                return false; // Don't update
            }

            if (targetLangId == sourceSettings.PrimaryLanguageId)
            {
                result = sourceS1;
                return true;
            }

            if (targetLangId == sourceSettings.SecondaryLanguageId)
            {
                result = sourceS2;
                return true;
            }

            if (targetLangId == sourceSettings.TernaryLanguageId)
            {
                result = sourceS3;
                return true;
            }

            result = null;
            return false;
        }

        static readonly object _consoleLock = new();
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
        static void Write(string s = "", ConsoleColor color = ConsoleColor.White)
        {
            lock (_consoleLock)
            {
                var original = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Write(s);
                Console.ForegroundColor = original;
            }
        }
    }

    public class TenantReport
    {
        public int TotalDefs { get; set; }
        public int SyncedDefs { get; set; }
        public HashSet<string> Errors { get; set; } = new();
        public Dictionary<string, HashSet<string>> DefinitionsErrors { get; set; } = new();
        public bool IsError => SyncedDefs != TotalDefs;
    }


    public class SynchronizerOptions
    {
        public int? MasterTenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantIds { get; set; }
        public string DefinitionCodes { get; set; }
        public bool SkipConfirmation { get; set; }
    }

    public class SynchronizerException : Exception
    {
        public SynchronizerException(string msg) : base(msg)
        {
        }
    }
}
