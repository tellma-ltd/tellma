CREATE FUNCTION [dal].[ft_NotedDate_Center_Agent__Balance] (
	@AccountTypeConcept NVARCHAR (255),
	@NotedDate DATE,
	@ParentCenterId INT,
	@AgentId INT
)
RETURNS @returntable TABLE
(
	[CenterId]		INT,
	[CurrencyId]	NCHAR (3),
	[Balance]		DECIMAL (19,4)
)
AS
BEGIN
	DECLARE @CenterNode HIERARCHYID = (SELECT [Node] FROM dbo.Centers WHERE [Id] = @ParentCenterId);
	INSERT @returntable
	SELECT  E.[CenterId], E.[CurrencyId], SUM([Direction] * [MonetaryValue]) AS Balance
	FROM dbo.Entries E
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	WHERE L.[State] = 4
	AND AC.[Concept] = @AccountTypeConcept
	AND (@CenterNode IS NULL OR C.[Node].IsDescendantOf(@CenterNode) = 1)
	AND (E.[NotedDate] <= @NotedDate)
	AND (E.[AgentId] = @AgentId)
	GROUP BY E.[CenterId], E.[CurrencyId]
	RETURN
END