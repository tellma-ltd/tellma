CREATE PROCEDURE [bll].[LegacyClassifications_Validate__Deprecate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsDeprecated BIT,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- None of the mapped accounts can have non zero balance
	--WITH
	--ActiveAccounts([Index], [AccountDefinitionId], [Value], [MonetaryValue], [Mass], [Resource], [Agent])
	--AS (
	--	SELECT I.[Index], DLE.AccountId, 
	--		SUM(DLE.[Direction] * DLE.[Value]) AS [Value],
	--		SUM(DLE.[Direction] * DLE.[MonetaryValue]),
	--		SUM(DLE.[Direction] * DLE.[Mass]),
	--		ISNULL(R.[Name], N''), ISNULL(AG.[Name], N'')
	--	-- TODO: Add the remaining units
	--	FROM dbo.Entries DLE
	--	JOIN dbo.Lines DL ON DLE.[LineId] = DL.[Id]
	--	JOIN dbo.Documents D ON DL.[DocumentId] = D.[Id]
	--	JOIN dbo.Accounts A ON DLE.AccountId = A.Id
	--	JOIN @Ids I ON I.[Id] = A.[AccountClassificationId]
	--	LEFT JOIN dbo.Resources R ON R.[Id] = A.[ResourceId]
	--	LEFT JOIN dbo.Agents AG ON AG.[Id] = A.[AgentId]
	--	WHERE D.[State] = 5 -- N'Closed'
	--	GROUP BY I.[Index], DLE.AccountId, R.[Name], AG.[Name], RC.[Name]
	--	HAVING
	--		SUM(DLE.[Direction] * DLE.[Value]) <> 0
	--	OR	SUM(DLE.[Direction] * DLE.[MonetaryValue]) <> 0
	--	OR	SUM(DLE.[Direction] * DLE.[Mass]) <> 0
	--)
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	--	SELECT TOP (@Top)
	--	'[' + CAST([Index] AS NVARCHAR (255)) + ']',
	--	N'Error_TheAccountClassificationHasBalance0InResource1Agent2',
	--	[Value] AS Argument0, [Resource] AS [Argument1], [Agent] AS [Argument2]
	--FROM ActiveAccounts

	SELECT TOP(@Top) * FROM @ValidationErrors;
RETURN 0
