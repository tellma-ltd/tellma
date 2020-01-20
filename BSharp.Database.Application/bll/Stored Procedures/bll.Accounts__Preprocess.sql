CREATE PROCEDURE [bll].[Accounts__Preprocess]
	@Entities [dbo].[AccountList] READONLY
AS
SET NOCOUNT ON;

DECLARE @ProcessedEntities [dbo].[AccountList];
INSERT INTO @ProcessedEntities SELECT * FROM @Entities;

-- If AgentId is set, then AgentDefinitionId is auto determined
UPDATE A
SET A.[AgentDefinitionId] = AG.[DefinitionId]
FROM @ProcessedEntities A JOIN [dbo].[Agents] AG ON A.[AgentId] = AG.[Id]
WHERE A.[AgentId] IS NOT NULL;

-- If there is only ONE active responsibility center set the account to it
IF (SELECT COUNT(*) FROM dbo.ResponsibilityCenters WHERE [IsActive] = 1) = 1
UPDATE @ProcessedEntities
SET [ResponsibilityCenterId] = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE [IsActive] = 1);

-- From Account Type, determine IsCurrent and HasResource, etc...
UPDATE A
SET
	A.[IsCurrent]	= COALESCE([AT].[IsCurrent], A.[IsCurrent]),
	A.[HasResource] = (CASE WHEN [AT].IsReal = 0 THEN 0 ELSE A.[HasResource] END),
	A.[HasAgent]	= (CASE WHEN [AT].[IsPersonal] = 0 THEN 0 ELSE A.[HasAgent] END),
	A.[EntryTypeId] = (CASE WHEN [AT].[EntryTypeParentId] IS NULL THEN NULL ELSE A.[EntryTypeId] END)
FROM @ProcessedEntities A JOIN [dbo].[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]

-- Return the result
SELECT * FROM @ProcessedEntities;
