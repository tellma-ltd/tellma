CREATE PROCEDURE [bll].[Accounts__Preprocess]
	@Entities [dbo].[AccountList] READONLY
AS
SET NOCOUNT ON;

DECLARE @ProcessedEntities [dbo].[AccountList];
INSERT INTO @ProcessedEntities SELECT * FROM @Entities;

-- If there is only ONE activecenter set the account to it
IF (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [IsActive] = 1 AND [IsLeaf] = 1) = 1
UPDATE @ProcessedEntities
SET [CenterId] = (SELECT [Id] FROM [dbo].[Centers] WHERE [IsActive] = 1 AND [IsLeaf] = 1);

-- If AgentDefinitionId IS NULL, Set AgentId to Null  (This depends on the previous step)
UPDATE PE
SET PE.[AgentId] = NULL
FROM @ProcessedEntities PE
JOIN AccountTypes AC ON PE.AccountTypeId = AC.[Id]
WHERE AC.[AgentAssignment] <> N'A'

-- If Resource has a CurrencyId, copy it to Account
UPDATE A
SET A.[CurrencyId] = R.[CurrencyId]
FROM @ProcessedEntities A JOIN [Resources] R ON A.[ResourceId] = R.Id
WHERE R.[CurrencyId] IS NOT NULL;

-- Return the result
SELECT * FROM @ProcessedEntities;
