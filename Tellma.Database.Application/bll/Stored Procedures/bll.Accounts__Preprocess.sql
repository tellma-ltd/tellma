CREATE PROCEDURE [bll].[Accounts__Preprocess]
	@Entities [dbo].[AccountList] READONLY
AS
SET NOCOUNT ON;
	--=-=-=-=-=-=- [C# Preprocess - Before SQL]
	/* 
	
	 [✓] IsRelated and IsSmart are set to 0 by default
	 [✓] IF IsSmart = 0 THEN these are set to NULL: ResourceId, AgentId, Identifier, EntryTypeId

	*/

DECLARE @ProcessedEntities [dbo].[AccountList];
INSERT INTO @ProcessedEntities SELECT * FROM @Entities;

-- IF there is only ONE activecenter set it in the account
-- ONLY if the account is dumb and AccountType.CenterAssignment = N'A'
IF (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [IsActive] = 1 AND [IsLeaf] = 1) = 1
UPDATE A 
SET A.[CenterId] = (SELECT [Id] FROM [dbo].[Centers] WHERE [IsActive] = 1 AND [IsLeaf] = 1)
FROM @ProcessedEntities A JOIN [dbo].[AccountTypes] T ON A.[AccountTypeId] = T.[Id]
WHERE A.IsSmart = 0 OR T.[CenterAssignment] = N'A';

-- If Resource has a CurrencyId, copy it to Account
-- ONLY if the account is dumb and AccountType.CurrencyAssignment = N'A'
UPDATE A
SET A.[CurrencyId] = R.[CurrencyId]
FROM @ProcessedEntities A JOIN [Resources] R ON A.[ResourceId] = R.Id
JOIN [dbo].[AccountTypes] T ON A.[AccountTypeId] = T.[Id]
WHERE R.[CurrencyId] IS NOT NULL AND (A.IsSmart = 0 OR T.[CenterAssignment] = N'A');

--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
-- Below we set things to NULL where Assignment <> N'A'
--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

-- Center
UPDATE A
SET A.[CenterId] = NULL
FROM @ProcessedEntities A JOIN [dbo].[AccountTypes] T ON A.[AccountTypeId] = T.[Id]
WHERE A.[IsSmart] = 1 AND T.[CenterAssignment] <> N'A';

-- Currency
UPDATE A
SET A.[CurrencyId] = NULL
FROM @ProcessedEntities A JOIN [dbo].[AccountTypes] T ON A.[AccountTypeId] = T.[Id]
WHERE A.[IsSmart] = 1 AND T.[CurrencyAssignment] <> N'A';

-- Resource
UPDATE A
SET A.[ResourceId] = NULL
FROM @ProcessedEntities A JOIN [dbo].[AccountTypes] T ON A.[AccountTypeId] = T.[Id]
WHERE T.[ResourceAssignment] <> N'A';

-- Agent
UPDATE A
SET A.[AgentId] = NULL
FROM @ProcessedEntities A JOIN [dbo].[AccountTypes] T ON A.[AccountTypeId] = T.[Id]
WHERE T.[AgentAssignment] <> N'A';

-- Identifier
UPDATE A
SET A.[Identifier] = NULL
FROM @ProcessedEntities A JOIN [dbo].[AccountTypes] T ON A.[AccountTypeId] = T.[Id]
WHERE T.[IdentifierAssignment] <> N'A';

-- EntryType
UPDATE A
SET A.[EntryTypeId] = NULL
FROM @ProcessedEntities A JOIN [dbo].[AccountTypes] T ON A.[AccountTypeId] = T.[Id]
WHERE T.[EntryTypeAssignment] <> N'A';


-- Return the result
SELECT * FROM @ProcessedEntities;
