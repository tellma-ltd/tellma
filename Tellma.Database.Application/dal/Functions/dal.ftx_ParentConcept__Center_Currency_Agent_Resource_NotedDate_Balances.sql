CREATE FUNCTION [dal].[ft_ParentConcept__Center_Currency_Agent_Resource_NotedDate_Balances]
(-- A null parameter means: bring me all possible values
	@ParentConcept NVARCHAR (255),
	@ParentCenterId INT,
	@CurrencyId NCHAR (3),
	@AgentId INT,
	@ResourceId INT,
	@NotedDate DATE,
	@AsOf DATE
)
RETURNS @ResultTable TABLE (
	[AccountId] INT,
	[CenterId] INT,
	[CurrencyId] NCHAR (3),
	[AgentId] INT,
	[ResourceId] INT,
	[NotedDate] DATE,
	[Quantity] DECIMAL (19, 4),
	[MonetaryValue] DECIMAL (19, 4),
	[Value] DECIMAL (19, 4),
	[NotedAmount] DECIMAL (19, 4)
)
AS BEGIN
	DECLARE @ParentAccountTypeNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept);
	DECLARE @ParentCenterNode HIERARCHYID = dal.fn_Center__Node(@ParentCenterId);

	INSERT INTO @ResultTable(
		[AccountId], [CenterId], [CurrencyId], [AgentId], [ResourceId], [NotedDate],
		[Quantity], [MonetaryValue], [Value], [NotedAmount]
		)
	SELECT
		E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedDate],
		SUM(E.[Direction] * E.[Quantity]) AS [Quantity],
		SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
		SUM(E.[Direction] * E.[Value]) AS [Value],
		SUM(E.[Direction] * E.[NotedAmount]) AS [NotedAmount]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	WHERE L.[State] = 4
	AND AC.[Node].IsDescendantOf(@ParentAccountTypeNode) = 1
	AND (@ParentCenterId IS NULL OR C.[Node].IsDescendantOf(@ParentCenterNode) = 1)
	AND (@CurrencyId IS NULL OR E.[CurrencyId] = @CurrencyId)
	AND (@AgentId IS NULL OR E.[AgentId] = @AgentId)
	AND (@ResourceId IS NULL OR E.[ResourceId] = @ResourceId)
	AND (@NotedDate IS NULL OR E.[NotedDate] = @NotedDate)	
	AND (@AsOf IS NULL OR L.[PostingDate] <= @AsOf)
	GROUP BY E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedDate]

	RETURN
END