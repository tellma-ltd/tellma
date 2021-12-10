CREATE FUNCTION [dal].[ft_NotedDate__Center_Agent_Balance] (
	@AccountTypeConcept NVARCHAR (255),
	@ParentCenterId INT,
	@NotedDate DATE
)
RETURNS @returntable TABLE
(
	[NotedDate]		DATE,
	[CenterId]		INT,
	[AgentId]		INT,
	[CurrencyId]	NCHAR (3),
	[Balance]		DECIMAL (19,4)
)
AS
BEGIN
	DECLARE @CenterNode HIERARCHYID = (SELECT [Node] FROM dbo.Centers WHERE [Id] = @ParentCenterId);
	INSERT @returntable
	SELECT E.[NotedDate], E.[CenterId], E.[AgentId], E.[CurrencyId], SUM([Direction] * [MonetaryValue]) AS Balance
	FROM dbo.Entries E
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	WHERE L.[State] = 4
	AND AC.[Concept] = @AccountTypeConcept
	AND (@CenterNode IS NULL OR C.[Node].IsDescendantOf(@CenterNode) = 1)
	AND (@NotedDate IS NULL OR E.[NotedDate] = @NotedDate)
	GROUP BY E.[CenterId], E.[AgentId], E.[CurrencyId], E.[NotedDate]
	RETURN
END