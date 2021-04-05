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

-- If Relation has a Currency or Center copy it to Account
UPDATE A
SET A.[CurrencyId] = COALESCE(R.[CurrencyId], A.[CurrencyId]),
	A.[CenterId] = COALESCE(R.[CenterId], A.[CenterId])
FROM @ProcessedEntities A JOIN dbo.[Relations] R ON A.[RelationId] = R.Id;

-- If Resource has a CurrencyId Or Center, copy it to Account
UPDATE A
SET
	A.[CurrencyId] = R.[CurrencyId],
	A.[CenterId] = COALESCE(
		IIF(AC.[IsBusinessUnit] = 1, R.[CenterId], R.[CostCenterId]),
		A.[CenterId])
FROM @ProcessedEntities A JOIN dbo.[Resources] R ON A.[ResourceId] = R.Id
JOIN map.AccountTypes() AC ON A.[AccountTypeId] = AC.[Id];

-- Account Type/Relation Definition = null => Account/Relation is null
UPDATE A
SET A.[RelationId] = NULL
FROM  @ProcessedEntities A
JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
WHERE AC.RelationDefinitionId IS NULL 

UPDATE A
SET A.[NotedRelationId] = NULL 
FROM  @ProcessedEntities A
JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.Id
WHERE AC.[NotedRelationDefinitionId] IS NULL

UPDATE A
SET ResourceId = NULL, ResourceDefinitionId = NULL 
FROM  @ProcessedEntities A
LEFT JOIN dbo.AccountTypeResourceDefinitions ATRD ON A.AccountTypeId = ATRD.AccountTypeId AND A.ResourceDefinitionId = ATRD.ResourceDefinitionId
WHERE A.ResourceDefinitionId IS NOT NULL AND ATRD.ResourceDefinitionId IS NULL

UPDATE A
SET ResourceId = NULL
FROM  @ProcessedEntities A
WHERE ResourceDefinitionId IS NULL 

-- Return the result
SELECT * FROM @ProcessedEntities;