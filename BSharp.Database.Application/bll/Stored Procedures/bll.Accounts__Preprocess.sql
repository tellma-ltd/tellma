CREATE PROCEDURE [bll].[Accounts__Preprocess]
	@Entities [dbo].[AccountList] READONLY
AS
SET NOCOUNT ON;

DECLARE @ProcessedEntities [dbo].[AccountList];
INSERT INTO @ProcessedEntities SELECT * FROM @Entities;

-- If AgentId is set, then AgentDefinitionId is auto determined
UPDATE [Q]
SET [Q].[AgentDefinitionId] = [A].[DefinitionId]
FROM @ProcessedEntities [Q] JOIN [dbo].[Agents] [A] ON [Q].[AgentId] = [A].[Id]
WHERE [Q].[AgentId] IS NOT NULL;

-- If ResourceId is set, then CurrencyId is auto determined
UPDATE [Q]
SET [Q].[CurrencyId] = [R].[CurrencyId]
FROM @ProcessedEntities [Q] JOIN [dbo].[Resources] [R] ON [Q].[ResourceId] = [R].[Id]
WHERE [Q].[IsSmart] = 1 AND [Q].[ResourceId] IS NOT NULL;

-- If ResourceId is set, then ResourceClassificationId is auto determined
UPDATE [Q]
SET [Q].[ResourceClassificationId] = [R].[ResourceClassificationId]
FROM @ProcessedEntities [Q] JOIN [dbo].[Resources] [R] ON [Q].[ResourceId] = [R].[Id]
WHERE [Q].[ResourceId] IS NOT NULL;

-- Return the result
SELECT * FROM @ProcessedEntities;
