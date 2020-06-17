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

--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
-- Below we set things to the only value that matches the Definition
-- NOTE: This is WRONG. What if we want to add more currencies after
-- we add the account?
--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

-- Return the result
SELECT * FROM @ProcessedEntities;