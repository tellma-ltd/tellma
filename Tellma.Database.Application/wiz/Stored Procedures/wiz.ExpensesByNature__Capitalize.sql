CREATE PROCEDURE [wiz].[ExpensesByNature__Capitalize]
	@DocumentIndex	INT = 0,
	@CenterType		NVARCHAR (255),
	@CenterId		INT = NULL
AS
	DECLARE @ExpenseByNatureNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'ExpenseByNature');
	DECLARE @LineDefinitionId INT =
		CASE
			WHEN @CenterType = N'ConstructionInProgressExpendituresControl' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CIPFromConstructionExpense')
			WHEN @CenterType = N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IPUCDFromDevelopmentExpense')
			WHEN @CenterType = N'WorkInProgressExpendituresControl' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'WIPFromProductionExpense')
			WHEN @CenterType = N'CurrentInventoriesInTransitExpendituresControl' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IITFromTransitExpense')
		END;

	DECLARE @WideLines WideLineList;

	WITH ExpenseByNatureAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Node].IsDescendantOf(@ExpenseByNatureNode) = 1
		)
	),
	UnCapitalizedExpenses AS (
		SELECT MIN(E.[Id]) AS [Id], E.[AccountId], E.[CenterId], E.[ResourceId], E.[ParticipantId],
				E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue], SUM(E.[Direction] * E.[Value])  AS [Value]
		FROM Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		WHERE DD.DocumentType = 2 -- event
		AND L.[State] = 4
		AND C.[CenterType] = @CenterType
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		AND (@CenterId IS NULL OR E.[CenterId] = @CenterId)
		GROUP BY E.[AccountId], E.[CenterId], E.[ResourceId], E.[ParticipantId],
				E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	)
	INSERT INTO @WideLines([Index], [DefinitionId],
			[DocumentIndex],
			[AccountId1], [CenterId1], [ResourceId1], [ParticipantId1], [CurrencyId1],
			[MonetaryValue1], [Value1])
	SELECT	ROW_NUMBER() OVER(ORDER BY [Id]) - 1, @LineDefinitionId,
			@DocumentIndex,
			[AccountId], [CenterId], [ResourceId], [ParticipantId], [CurrencyId],
			[MonetaryValue], [Value]
	FROM UnCapitalizedExpenses;

	SELECT * FROM @WideLines;