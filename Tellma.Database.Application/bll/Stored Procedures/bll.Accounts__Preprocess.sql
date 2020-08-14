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

-- If Custody has a Currency or Center copy it to Account
UPDATE A
SET A.[CurrencyId] = COALESCE(C.[CurrencyId], A.[CurrencyId]),
	A.[CenterId] = COALESCE(C.[CenterId], A.[CenterId])
FROM @ProcessedEntities A JOIN dbo.[Custodies] C ON A.[CustodyId] = C.Id;

-- If Resource has a CurrencyId Or Center, copy it to Account
UPDATE A
SET
	A.[CurrencyId] = COALESCE(R.[CurrencyId], A.[CurrencyId]),
	A.[CenterId] = COALESCE(R.[CenterId], A.[CenterId])
FROM @ProcessedEntities A JOIN dbo.[Resources] R ON A.[ResourceId] = R.Id;

UPDATE A
SET CustodyDefinitionId = NULL 
FROM  @ProcessedEntities A
LEFT JOIN dbo.AccountTypeCustodyDefinitions ATCD ON A.AccountTypeId = ATCD.AccountTypeId AND A.CustodyDefinitionId = ATCD.CustodyDefinitionId
WHERE A.CustodyDefinitionId IS NOT NULL

UPDATE A
SET ResourceDefinitionId = NULL 
FROM  @ProcessedEntities A
LEFT JOIN dbo.AccountTypeResourceDefinitions ATRD ON A.AccountTypeId = ATRD.AccountTypeId AND A.ResourceDefinitionId = ATRD.ResourceDefinitionId
WHERE A.ResourceDefinitionId IS NOT NULL

UPDATE A
SET NotedRelationDefinitionId = NULL 
FROM  @ProcessedEntities A
LEFT JOIN dbo.AccountTypeNotedRelationDefinitions ATNRD ON A.AccountTypeId = ATNRD.AccountTypeId AND A.NotedRelationDefinitionId = ATNRD.NotedRelationDefinitionId
WHERE A.NotedRelationDefinitionId IS NOT NULL
--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
-- Below we set things to the only value that matches the Definition
-- NOTE: This is WRONG. What if we want to add more currencies after
-- we add the account?
--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

-- Return the result
SELECT * FROM @ProcessedEntities;