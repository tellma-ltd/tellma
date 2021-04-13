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
/*
UPDATE A
SET
	A.[CurrencyId] = R.[CurrencyId],
	A.[CenterId] = COALESCE(
		IIF(AC.[IsBusinessUnit] = 1, R.[CenterId], R.[CostCenterId]),
		A.[CenterId])
FROM @ProcessedEntities A JOIN dbo.[Resources] R ON A.[ResourceId] = R.Id
JOIN map.AccountTypes() AC ON A.[AccountTypeId] = AC.[Id];
*/
-- Account Type/Relation Definition = null => Account/Relation is null
UPDATE A
SET RelationId = NULL, RelationDefinitionId = NULL 
FROM  @ProcessedEntities A
LEFT JOIN dbo.AccountTypeRelationDefinitions ATRD ON A.AccountTypeId = ATRD.AccountTypeId AND A.RelationDefinitionId = ATRD.RelationDefinitionId
WHERE A.RelationDefinitionId IS NOT NULL AND ATRD.RelationDefinitionId IS NULL

UPDATE A
SET ResourceId = NULL, ResourceDefinitionId = NULL 
FROM  @ProcessedEntities A
LEFT JOIN dbo.AccountTypeResourceDefinitions ATRD ON A.AccountTypeId = ATRD.AccountTypeId AND A.ResourceDefinitionId = ATRD.ResourceDefinitionId
WHERE A.ResourceDefinitionId IS NOT NULL AND ATRD.ResourceDefinitionId IS NULL

UPDATE A
SET NotedRelationId = NULL, NotedRelationDefinitionId = NULL 
FROM  @ProcessedEntities A
LEFT JOIN dbo.AccountTypeNotedRelationDefinitions ATRD ON A.AccountTypeId = ATRD.AccountTypeId AND A.NotedRelationDefinitionId = ATRD.NotedRelationDefinitionId
WHERE A.NotedRelationDefinitionId IS NOT NULL AND ATRD.NotedRelationDefinitionId IS NULL

UPDATE A
SET ResourceId = NULL
FROM  @ProcessedEntities A
WHERE ResourceDefinitionId IS NULL 

-- Return the result
SELECT * FROM @ProcessedEntities;