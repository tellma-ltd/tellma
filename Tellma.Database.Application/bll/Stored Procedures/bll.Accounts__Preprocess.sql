CREATE PROCEDURE [bll].[Accounts__Preprocess]
	@Entities [dbo].[AccountList] READONLY
AS
SET NOCOUNT ON;
	--=-=-=-=-=-=- [C# Preprocess - Before SQL]
	/* 
	
	 [✓] IsRelated and IsSmart are set to 0 by default
	 [✓] IF IsSmart = 0 THEN these are set to NULL: ResourceId, ContractId, Identifier, EntryTypeId

	*/

DECLARE @ProcessedEntities [dbo].[AccountList];
INSERT INTO @ProcessedEntities SELECT * FROM @Entities;

-- If Resource has a CurrencyId, copy it to Account
UPDATE A
SET A.[CurrencyId] = COALESCE(R.[CurrencyId], A.[CurrencyId])
FROM @ProcessedEntities A JOIN dbo.[Resources] R ON A.[ResourceId] = R.Id;

-- If Contract has a CurrencyId, copy it to Account
UPDATE A
SET A.[CurrencyId] = COALESCE(C.[CurrencyId], A.[CurrencyId])
FROM @ProcessedEntities A JOIN dbo.[Contracts] C ON A.[ContractId] = C.Id;

-- If Center has expense entry type, and A is an expense account, copy it to Account
WITH ExpenseIfrsTypes AS (
	SELECT [Id] FROM dbo.AccountTypes
	WHERE [Node].IsDescendantOf((
		SELECT [Node] FROM dbo.AccountTypes
		WHERE [Code] = N'ExpenseByNature'
	)) = 1
)
UPDATE A
SET A.[EntryTypeId] = COALESCE(C.[ExpenseEntryTypeId], A.[EntryTypeId])
FROM @ProcessedEntities A
JOIN dbo.[Centers] C ON A.[CenterId] = C.[Id]
WHERE A.IfrsTypeId IN (SELECT [Id] FROM ExpenseIfrsTypes);

--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
-- Below we set things to the only value that matches the Definition
-- NOTE: This is WRONG. What if we want to add more currencies after
-- we add the account
--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

-- Center: If only one center matches the AD filter, use it
--UPDATE A
--SET A.[CenterId] = ADF.[CenterId]
--FROM @ProcessedEntities A
--JOIN (
--	SELECT ADCT.[AccountDefinitionId], MIN(C.[Id]) AS [CenterId]
--	FROM dbo.[AccountDefinitionCenterTypes] ADCT
--	JOIN dbo.Centers C ON ADCT.CenterType = C.CenterType
--	WHERE C.[IsActive] = 1 AND C.[IsLeaf] = 1
--	GROUP BY  ADCT.[AccountDefinitionId]
--	HAVING COUNT(*) = 1
--) ADF ON A.[DefinitionId] = ADF.[AccountDefinitionId]

---- Currency
--UPDATE A
--SET A.[CurrencyId] = ADF.[CurrencyId]
--FROM @ProcessedEntities A
--JOIN (
--	SELECT ADC.[AccountDefinitionId], MIN(ADC.[CurrencyId]) AS [CurrencyId]
--	FROM dbo.[AccountDefinitionCurrencies] ADC
--	GROUP BY ADC.[AccountDefinitionId]
--	HAVING COUNT(*) = 1
--) ADF ON A.[DefinitionId] = ADF.[AccountDefinitionId]

---- Resource
--UPDATE A
--SET A.[ResourceId] = ADF.[ResourceId]
--FROM @ProcessedEntities A
--JOIN (
--	SELECT ADC.[AccountDefinitionId], MIN(R.[Id]) AS [ResourceId]
--	FROM dbo.[AccountDefinitionResourceDefinitions] ADC
--	JOIN dbo.Resources R ON R.[DefinitionId] = ADC.[ResourceDefinitionId]
--	WHERE R.[IsActive] = 1
--	GROUP BY ADC.[AccountDefinitionId]
--	HAVING COUNT(*) = 1
--) ADF ON A.[DefinitionId] = ADF.[AccountDefinitionId]

---- Contract
--UPDATE A
--SET A.[ContractId] = ADF.[ContractId]
--FROM @ProcessedEntities A
--JOIN (
--	SELECT ADC.[AccountDefinitionId], MIN(R.[Id]) AS [ContractId]
--	FROM dbo.[AccountDefinitionContractDefinitions] ADC
--	JOIN dbo.[Contracts] R ON R.[DefinitionId] = ADC.[ContractDefinitionId]
--	WHERE R.[IsActive] = 1
--	GROUP BY ADC.[AccountDefinitionId]
--	HAVING COUNT(*) = 1
--) ADF ON A.[DefinitionId] = ADF.[AccountDefinitionId];

---- EntryType
--UPDATE A
--SET A.[EntryTypeId] = ADF.[EntryTypeId]
--FROM @ProcessedEntities A
--JOIN (
--	SELECT AD.[Id] AS [AccountDefinitionId], MIN(CET.[Id]) AS [EntryTypeId]
--	FROM dbo.[AccountDefinitions] AD
--	JOIN dbo.[EntryTypes] PET ON AD.[EntryTypeParentId] = PET.Id
--	JOIN dbo.[EntryTypes] CET ON CET.[Node].IsDescendantOf(PET.[Node]) = 1
--	WHERE CET.[IsActive] = 1 AND CET.[IsAssignable] = 1
--	GROUP BY AD.[Id]
--	HAVING COUNT(*) = 1
--) ADF ON A.[DefinitionId] = ADF.[AccountDefinitionId]

-- Return the result
SELECT * FROM @ProcessedEntities;
