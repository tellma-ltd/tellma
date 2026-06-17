SELECT [P].[Id] AS [Id]
INTO #Docs
{DocScope}
OPTION(RECOMPILE) 

SELECT [P].[Id] AS [Id]
INTO #Lines
FROM [map].[Lines]() As [P]
WHERE [P].[DocumentId] IN (SELECT [Id] FROM #Docs)
OPTION(RECOMPILE) 

SELECT [P1].[AccountTypeId] AS [AccountTypeId], [P1].[ResourceId] AS [AccountResourceId], [P].[ResourceId] AS [EntryResourceId], [P].[NotedResourceId] AS [EntryNotedResourceId]
INTO #Entries
FROM [map].[Entries]() As [P]
LEFT JOIN [map].[Accounts]() As [P1] ON [P].[AccountId] = [P1].[Id]
WHERE [P].[LineId] IN (SELECT [Id] FROM #Lines)
OPTION(RECOMPILE) 

SELECT [P].[Id], [P].[CurrencyId], [P].[CenterId], [P].[AgentId], [P].[ResourceId], [P].[NotedAgentId], [P].[NotedResourceId], [P].[ReferenceSourceId], [P].[UnitId], [P].[DurationUnitId], [P].[CreatedById], [P].[ModifiedById], [P].[AssigneeId], [P].[Lookup1Id], [P].[Lookup2Id], [P].[SerialNumber], [P].[Clearance], [P].[PostingDate], [P].[PostingDateIsCommon], [P].[Memo], [P].[MemoIsCommon], [P].[CurrencyIsCommon], [P].[CenterIsCommon], [P].[AgentIsCommon], [P].[NotedAgentIsCommon], [P].[ResourceIsCommon], [P].[NotedResourceIsCommon], [P].[Quantity], [P].[QuantityIsCommon], [P].[UnitIsCommon], [P].[Time1], [P].[Time1IsCommon], [P].[Duration], [P].[DurationIsCommon], [P].[DurationUnitIsCommon], [P].[Time2], [P].[Time2IsCommon], [P].[NotedDate], [P].[NotedDateIsCommon], [P].[ExternalReference], [P].[ExternalReferenceIsCommon], [P].[ReferenceSourceIsCommon], [P].[InternalReference], [P].[InternalReferenceIsCommon], [P].[DefinitionId], [P].[Code], [P].[State], [P].[StateAt], [P].[Comment], [P].[AssignedAt], [P].[AssignedById], [P].[OpenedAt], [P].[CreatedAt], [P].[ModifiedAt], [P1].[Name], [P1].[Id], [P1].[Name2], [P1].[Name3], [P1].[E], [P2].[Name], [P2].[Id], [P2].[Name2], [P2].[Name3], [P3].[Name], [P3].[Id], [P3].[Name2], [P3].[Name3], [P3].[DefinitionId], [P4].[Name], [P4].[Id], [P4].[Name2], [P4].[Name3], [P4].[DefinitionId], [P5].[Name], [P5].[Id], [P5].[Name2], [P5].[Name3], [P5].[DefinitionId], [P6].[Name], [P6].[Id], [P6].[Name2], [P6].[Name3], [P6].[DefinitionId], [P7].[Name], [P7].[Id], [P7].[Name2], [P7].[Name3], [P7].[DefinitionId], [P8].[Name], [P8].[Id], [P8].[Name2], [P8].[Name3], [P9].[Name], [P9].[Id], [P9].[Name2], [P9].[Name3], [P10].[Name], [P10].[Id], [P10].[Name2], [P10].[Name3], [P10].[ImageId], [P11].[Name], [P11].[Id], [P11].[Name2], [P11].[Name3], [P11].[ImageId], [P12].[Name], [P12].[Id], [P12].[Name2], [P12].[Name3], [P12].[ImageId], [P13].[Name], [P13].[Id], [P13].[Name2], [P13].[Name3], [P13].[DefinitionId], [P14].[Name], [P14].[Id], [P14].[Name2], [P14].[Name3], [P14].[DefinitionId]
{DocScope}
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P].[Id], [P].[CurrencyId], [P].[CenterId], [P].[AgentId], [P].[ResourceId], [P].[NotedAgentId], [P].[NotedResourceId], [P].[ReferenceSourceId], [P].[UnitId], [P].[DurationUnitId], [P].[LineDefinitionId], [P].[EntryIndex], [P].[PostingDate], [P].[PostingDateIsCommon], [P].[Memo], [P].[MemoIsCommon], [P].[CurrencyIsCommon], [P].[CenterIsCommon], [P].[AgentIsCommon], [P].[NotedAgentIsCommon], [P].[ResourceIsCommon], [P].[NotedResourceIsCommon], [P].[Quantity], [P].[QuantityIsCommon], [P].[UnitIsCommon], [P].[Time1], [P].[Time1IsCommon], [P].[Duration], [P].[DurationIsCommon], [P].[DurationUnitIsCommon], [P].[Time2], [P].[Time2IsCommon], [P].[NotedDate], [P].[NotedDateIsCommon], [P].[ExternalReference], [P].[ExternalReferenceIsCommon], [P].[ReferenceSourceIsCommon], [P].[InternalReference], [P].[InternalReferenceIsCommon], [P].[DocumentId], [P].[CreatedAt], [P].[CreatedById], [P].[ModifiedAt], [P].[ModifiedById], [P1].[Name], [P1].[Id], [P1].[Name2], [P1].[Name3], [P1].[E], [P2].[Name], [P2].[Id], [P2].[Name2], [P2].[Name3], [P3].[Name], [P3].[Id], [P3].[Name2], [P3].[Name3], [P3].[DefinitionId], [P4].[Name], [P4].[Id], [P4].[Name2], [P4].[Name3], [P4].[DefinitionId], [P5].[Name], [P5].[Id], [P5].[Name2], [P5].[Name3], [P5].[DefinitionId], [P6].[Name], [P6].[Id], [P6].[Name2], [P6].[Name3], [P6].[DefinitionId], [P7].[Name], [P7].[Id], [P7].[Name2], [P7].[Name3], [P7].[DefinitionId], [P8].[Name], [P8].[Id], [P8].[Name2], [P8].[Name3], [P9].[Name], [P9].[Id], [P9].[Name2], [P9].[Name3]
FROM [map].[DocumentLineDefinitionEntries]() As [P]
LEFT JOIN [map].[Currencies]() As [P1] ON [P].[CurrencyId] = [P1].[Id]
LEFT JOIN [map].[Centers]() As [P2] ON [P].[CenterId] = [P2].[Id]
LEFT JOIN [map].[Agents]() As [P3] ON [P].[AgentId] = [P3].[Id]
LEFT JOIN [map].[Resources]() As [P4] ON [P].[ResourceId] = [P4].[Id]
LEFT JOIN [map].[Agents]() As [P5] ON [P].[NotedAgentId] = [P5].[Id]
LEFT JOIN [map].[Resources]() As [P6] ON [P].[NotedResourceId] = [P6].[Id]
LEFT JOIN [map].[Agents]() As [P7] ON [P].[ReferenceSourceId] = [P7].[Id]
LEFT JOIN [map].[Units]() As [P8] ON [P].[UnitId] = [P8].[Id]
LEFT JOIN [map].[Units]() As [P9] ON [P].[DurationUnitId] = [P9].[Id]
WHERE [P].[DocumentId] IN (SELECT [Id] FROM #Docs)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P].[Id], [P].[DefinitionId], [P].[PostingDate], [P].[Memo], [P].[Boolean1], [P].[Decimal1], [P].[Decimal2], [P].[Text1], [P].[Text2], [P].[DocumentId], [P].[EmployeeId], [P].[CustomerId], [P].[SupplierId], [P].[State], [P].[CreatedAt], [P].[CreatedById], [P].[ModifiedAt], [P].[ModifiedById], [P].[Index]
FROM [map].[Lines]() As [P]
WHERE [P].[DocumentId] IN (SELECT [Id] FROM #Docs)
ORDER BY [P].[Index] ASC, [P].[Id]
OPTION(RECOMPILE) 

SELECT [P].[Id], [P].[AccountId], [P].[CurrencyId], [P].[AgentId], [P].[ResourceId], [P].[NotedAgentId], [P].[NotedResourceId], [P].[ReferenceSourceId], [P].[EntryTypeId], [P].[CenterId], [P].[UnitId], [P].[DurationUnitId], [P].[Direction], [P].[MonetaryValue], [P].[Quantity], [P].[Value], [P].[RValue], [P].[PValue], [P].[Time1], [P].[Duration], [P].[Time2], [P].[ExternalReference], [P].[InternalReference], [P].[NotedAgentName], [P].[NotedAmount], [P].[NotedDate], [P].[Index], [P].[LineId], [P].[CreatedAt], [P].[CreatedById], [P].[ModifiedAt], [P].[ModifiedById], [P1].[Name], [P1].[Id], [P1].[AccountTypeId], [P1].[CenterId], [P1].[EntryTypeId], [P1].[CurrencyId], [P1].[AgentId], [P1].[ResourceId], [P1].[NotedAgentId], [P1].[NotedResourceId], [P1].[Name2], [P1].[Name3], [P1].[Code], [P1].[AgentDefinitionId], [P1].[ResourceDefinitionId], [P1].[NotedAgentDefinitionId], [P1].[NotedResourceDefinitionId], [P2].[Name], [P2].[Id], [P2].[EntryTypeParentId], [P2].[Name2], [P2].[Name3], [P2].[StandardAndPure], [P2].[Time1Label], [P2].[Time1Label2], [P2].[Time1Label3], [P2].[Time2Label], [P2].[Time2Label2], [P2].[Time2Label3], [P2].[ExternalReferenceLabel], [P2].[ExternalReferenceLabel2], [P2].[ExternalReferenceLabel3], [P2].[ReferenceSourceLabel], [P2].[ReferenceSourceLabel2], [P2].[ReferenceSourceLabel3], [P2].[InternalReferenceLabel], [P2].[InternalReferenceLabel2], [P2].[InternalReferenceLabel3], [P2].[NotedAgentNameLabel], [P2].[NotedAgentNameLabel2], [P2].[NotedAgentNameLabel3], [P2].[NotedAmountLabel], [P2].[NotedAmountLabel2], [P2].[NotedAmountLabel3], [P2].[NotedDateLabel], [P2].[NotedDateLabel2], [P2].[NotedDateLabel3], [P3].[IsActive], [P3].[Id], [P4].[Name], [P4].[Id], [P4].[Name2], [P4].[Name3], [P5].[Name], [P5].[Id], [P5].[Name2], [P5].[Name3], [P5].[IsActive], [P6].[Name], [P6].[Id], [P6].[Name2], [P6].[Name3], [P6].[E], [P7].[Name], [P7].[Id], [P7].[Name2], [P7].[Name3], [P7].[DefinitionId], [P8].[Name], [P8].[Id], [P8].[UnitId], [P8].[Name2], [P8].[Name3], [P8].[DefinitionId], [P9].[Name], [P9].[Id], [P9].[Name2], [P9].[Name3], [P10].[Name], [P10].[Id], [P10].[Name2], [P10].[Name3], [P10].[DefinitionId], [P11].[Name], [P11].[Id], [P11].[Name2], [P11].[Name3], [P11].[DefinitionId], [P12].[Name], [P12].[Id], [P12].[Name2], [P12].[Name3], [P12].[E], [P13].[Name], [P13].[Id], [P13].[CurrencyId], [P13].[CenterId], [P13].[Name2], [P13].[Name3], [P13].[DefinitionId], [P14].[Name], [P14].[Id], [P14].[Name2], [P14].[Name3], [P14].[E], [P15].[Name], [P15].[Id], [P15].[Name2], [P15].[Name3], [P16].[Name], [P16].[Id], [P16].[UnitId], [P16].[CurrencyId], [P16].[CenterId], [P16].[Name2], [P16].[Name3], [P16].[DefinitionId], [P17].[Name], [P17].[Id], [P17].[Name2], [P17].[Name3], [P18].[Name], [P18].[Id], [P18].[Name2], [P18].[Name3], [P18].[E], [P19].[Name], [P19].[Id], [P19].[Name2], [P19].[Name3], [P20].[Name], [P20].[Id], [P20].[CurrencyId], [P20].[CenterId], [P20].[Name2], [P20].[Name3], [P20].[DefinitionId], [P21].[Name], [P21].[Id], [P21].[Name2], [P21].[Name3], [P21].[E], [P22].[Name], [P22].[Id], [P22].[Name2], [P22].[Name3], [P23].[Name], [P23].[Id], [P23].[UnitId], [P23].[CurrencyId], [P23].[CenterId], [P23].[Name2], [P23].[Name3], [P23].[DefinitionId], [P24].[Name], [P24].[Id], [P24].[Name2], [P24].[Name3], [P25].[Name], [P25].[Id], [P25].[Name2], [P25].[Name3], [P25].[E], [P26].[Name], [P26].[Id], [P26].[Name2], [P26].[Name3], [P27].[Name], [P27].[Id], [P27].[Name2], [P27].[Name3], [P27].[DefinitionId], [P28].[Name], [P28].[Id], [P28].[Name2], [P28].[Name3], [P28].[IsActive], [P29].[Name], [P29].[Id], [P29].[Name2], [P29].[Name3], [P30].[Name], [P30].[Id], [P30].[Name2], [P30].[Name3], [P31].[Name], [P31].[Id], [P31].[Name2], [P31].[Name3]
FROM [map].[Entries]() As [P]
LEFT JOIN [map].[Accounts]() As [P1] ON [P].[AccountId] = [P1].[Id]
LEFT JOIN [map].[AccountTypes]() As [P2] ON [P1].[AccountTypeId] = [P2].[Id]
LEFT JOIN [map].[EntryTypes]() As [P3] ON [P2].[EntryTypeParentId] = [P3].[Id]
LEFT JOIN [map].[Centers]() As [P4] ON [P1].[CenterId] = [P4].[Id]
LEFT JOIN [map].[EntryTypes]() As [P5] ON [P1].[EntryTypeId] = [P5].[Id]
LEFT JOIN [map].[Currencies]() As [P6] ON [P1].[CurrencyId] = [P6].[Id]
LEFT JOIN [map].[Agents]() As [P7] ON [P1].[AgentId] = [P7].[Id]
LEFT JOIN [map].[Resources]() As [P8] ON [P1].[ResourceId] = [P8].[Id]
LEFT JOIN [map].[Units]() As [P9] ON [P8].[UnitId] = [P9].[Id]
LEFT JOIN [map].[Agents]() As [P10] ON [P1].[NotedAgentId] = [P10].[Id]
LEFT JOIN [map].[Resources]() As [P11] ON [P1].[NotedResourceId] = [P11].[Id]
LEFT JOIN [map].[Currencies]() As [P12] ON [P].[CurrencyId] = [P12].[Id]
LEFT JOIN [map].[Agents]() As [P13] ON [P].[AgentId] = [P13].[Id]
LEFT JOIN [map].[Currencies]() As [P14] ON [P13].[CurrencyId] = [P14].[Id]
LEFT JOIN [map].[Centers]() As [P15] ON [P13].[CenterId] = [P15].[Id]
LEFT JOIN [map].[Resources]() As [P16] ON [P].[ResourceId] = [P16].[Id]
LEFT JOIN [map].[Units]() As [P17] ON [P16].[UnitId] = [P17].[Id]
LEFT JOIN [map].[Currencies]() As [P18] ON [P16].[CurrencyId] = [P18].[Id]
LEFT JOIN [map].[Centers]() As [P19] ON [P16].[CenterId] = [P19].[Id]
LEFT JOIN [map].[Agents]() As [P20] ON [P].[NotedAgentId] = [P20].[Id]
LEFT JOIN [map].[Currencies]() As [P21] ON [P20].[CurrencyId] = [P21].[Id]
LEFT JOIN [map].[Centers]() As [P22] ON [P20].[CenterId] = [P22].[Id]
LEFT JOIN [map].[Resources]() As [P23] ON [P].[NotedResourceId] = [P23].[Id]
LEFT JOIN [map].[Units]() As [P24] ON [P23].[UnitId] = [P24].[Id]
LEFT JOIN [map].[Currencies]() As [P25] ON [P23].[CurrencyId] = [P25].[Id]
LEFT JOIN [map].[Centers]() As [P26] ON [P23].[CenterId] = [P26].[Id]
LEFT JOIN [map].[Agents]() As [P27] ON [P].[ReferenceSourceId] = [P27].[Id]
LEFT JOIN [map].[EntryTypes]() As [P28] ON [P].[EntryTypeId] = [P28].[Id]
LEFT JOIN [map].[Centers]() As [P29] ON [P].[CenterId] = [P29].[Id]
LEFT JOIN [map].[Units]() As [P30] ON [P].[UnitId] = [P30].[Id]
LEFT JOIN [map].[Units]() As [P31] ON [P].[DurationUnitId] = [P31].[Id]
WHERE [P].[LineId] IN (SELECT [Id] FROM #Lines)
ORDER BY [P].[Index] ASC, [P].[Id]
OPTION(RECOMPILE) 

SELECT [P].[AgentDefinitionId], [P].[Id], [P].[AccountTypeId]
FROM [map].[AccountTypeAgentDefinitions]() As [P]
WHERE [P].[AccountTypeId] IN (SELECT [AccountTypeId] FROM #Entries)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P].[ResourceDefinitionId], [P].[Id], [P].[AccountTypeId]
FROM [map].[AccountTypeResourceDefinitions]() As [P]
WHERE [P].[AccountTypeId] IN (SELECT [AccountTypeId] FROM #Entries)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P].[NotedAgentDefinitionId], [P].[Id], [P].[AccountTypeId]
FROM [map].[AccountTypeNotedAgentDefinitions]() As [P]
WHERE [P].[AccountTypeId] IN (SELECT [AccountTypeId] FROM #Entries)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P].[NotedResourceDefinitionId], [P].[Id], [P].[AccountTypeId]
FROM [map].[AccountTypeNotedResourceDefinitions]() As [P]
WHERE [P].[AccountTypeId] IN (SELECT [AccountTypeId] FROM #Entries)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P1].[Name], [P].[Id], [P].[UnitId], [P1].[Id], [P1].[Name2], [P1].[Name3], [P].[ResourceId]
FROM [map].[ResourceUnits]() As [P]
LEFT JOIN [map].[Units]() As [P1] ON [P].[UnitId] = [P1].[Id]
WHERE [P].[ResourceId] IN (SELECT [AccountResourceId] FROM #Entries)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P1].[Name], [P].[Id], [P].[UnitId], [P1].[Id], [P1].[Name2], [P1].[Name3], [P].[ResourceId]
FROM [map].[ResourceUnits]() As [P]
LEFT JOIN [map].[Units]() As [P1] ON [P].[UnitId] = [P1].[Id]
WHERE [P].[ResourceId] IN (SELECT [EntryResourceId] FROM #Entries)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P1].[Name], [P].[Id], [P].[UnitId], [P1].[Id], [P1].[Name2], [P1].[Name3], [P].[ResourceId]
FROM [map].[ResourceUnits]() As [P]
LEFT JOIN [map].[Units]() As [P1] ON [P].[UnitId] = [P1].[Id]
WHERE [P].[ResourceId] IN (SELECT [EntryNotedResourceId] FROM #Entries)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P].[Id], [P].[CreatedById], [P].[ModifiedById], [P].[FileName], [P].[FileExtension], [P].[DocumentId], [P].[FileId], [P].[Size], [P].[CreatedAt], [P].[ModifiedAt], [P1].[Name], [P1].[Id], [P1].[Name2], [P1].[Name3], [P1].[ImageId], [P2].[Name], [P2].[Id], [P2].[Name2], [P2].[Name3], [P2].[ImageId]
FROM [map].[Attachments]() As [P]
LEFT JOIN [map].[Users]() As [P1] ON [P].[CreatedById] = [P1].[Id]
LEFT JOIN [map].[Users]() As [P2] ON [P].[ModifiedById] = [P2].[Id]
WHERE [P].[DocumentId] IN (SELECT [Id] FROM #Docs)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P].[Id], [P].[ModifiedById], [P].[DocumentId], [P].[FromState], [P].[ToState], [P].[ModifiedAt], [P1].[Name], [P1].[Id], [P1].[Name2], [P1].[Name3], [P1].[ImageId]
FROM [map].[DocumentStatesHistory]() As [P]
LEFT JOIN [map].[Users]() As [P1] ON [P].[ModifiedById] = [P1].[Id]
WHERE [P].[DocumentId] IN (SELECT [Id] FROM #Docs)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 

SELECT [P].[Id], [P].[CreatedById], [P].[AssigneeId], [P].[DocumentId], [P].[Comment], [P].[CreatedAt], [P].[ModifiedAt], [P].[OpenedAt], [P1].[Name], [P1].[Id], [P1].[Name2], [P1].[Name3], [P1].[ImageId], [P2].[Name], [P2].[Id], [P2].[Name2], [P2].[Name3], [P2].[ImageId]
FROM [map].[DocumentAssignmentsHistory]() As [P]
LEFT JOIN [map].[Users]() As [P1] ON [P].[CreatedById] = [P1].[Id]
LEFT JOIN [map].[Users]() As [P2] ON [P].[AssigneeId] = [P2].[Id]
WHERE [P].[DocumentId] IN (SELECT [Id] FROM #Docs)
ORDER BY [P].[Id] ASC
OPTION(RECOMPILE) 
