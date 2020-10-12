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
	A.[CurrencyId] = R.[CurrencyId],
	A.[CenterId] = COALESCE(
		IIF(AC.[IsBusinessUnit] = 1, R.[CenterId], R.[CostCenterId]),
		A.[CenterId])
FROM @ProcessedEntities A JOIN dbo.[Resources] R ON A.[ResourceId] = R.Id
JOIN map.AccountTypes() AC ON A.[AccountTypeId] = AC.[Id];
-- In account type, Custodian def is null iff all custody definitions have custodian def is null


-- Exists Custody Definitions in AT => (Custody Def is optional) 
-- If custody is defined, then /Custodian = Custody/Custodian


-- Account/AccountType/CustodianDefinition = null => Account/Custodian = null
UPDATE A
SET	CustodianId = NULL 
FROM  @ProcessedEntities A
JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.Id
WHERE AC.CustodianDefinitionId IS NULL

-- No Account/AccountType/CustodyDefinitions => Account/Custody = null, Account/CustodyDefinition = null
UPDATE A
SET CustodyId = NULL, CustodyDefinitionId = NULL 
FROM  @ProcessedEntities A
LEFT JOIN dbo.AccountTypeCustodyDefinitions ATCD ON A.AccountTypeId = ATCD.AccountTypeId AND A.CustodyDefinitionId = ATCD.CustodyDefinitionId
WHERE A.CustodyDefinitionId IS NOT NULL AND ATCD.CustodyDefinitionId IS NULL

-- Account/CustodyDefinition = null => Account/Custody is null
UPDATE A
SET CustodyId = NULL
FROM  @ProcessedEntities A
WHERE CustodyDefinitionId IS NULL 

UPDATE A
SET ParticipantId = NULL 
FROM  @ProcessedEntities A
JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.Id
WHERE AC.ParticipantDefinitionId IS NULL

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