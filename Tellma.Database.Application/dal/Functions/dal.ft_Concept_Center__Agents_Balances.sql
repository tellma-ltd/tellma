CREATE FUNCTION [dal].[ft_Concept_Center__Agents_Balances]
(
	@ParentConcept NVARCHAR (255),
	@CenterId INT
)
RETURNS @ResultTable TABLE (
	[CenterId] INT,
	[AccountId] INT,
	[CurrencyId] NCHAR (3),
	[AgentId] INT,
	[ResourceId] INT,
	[InternalReference]  NVARCHAR (50),
	[ExternalReference] NVARCHAR (50),
	[NotedAgentId] INT,
	[NotedResourceId] INT,
	[NotedDate] DATE,
	[Balance] DECIMAL (19, 4),
	[Value] DECIMAL (19, 4),
	[NotedAmount] DECIMAL (19, 4)
)
AS BEGIN
	DECLARE @ParentNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept);

	INSERT INTO @ResultTable([CenterId], [AccountId], [CurrencyId], [AgentId], [ResourceId], [InternalReference], [ExternalReference], [NotedAgentId], [NotedResourceId], [NotedDate],
		[Balance], [Value], [NotedAmount])
	SELECT E.[CenterId], E.[AccountId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[InternalReference], E.[ExternalReference], E.[NotedAgentId], E.[NotedResourceId], E.[NotedDate],
		SUM(E.[Direction] * E.[MonetaryValue]), SUM(E.[Direction] * E.[Value]), SUM(E.[Direction] * E.[NotedAmount])
	FROM dbo.Entries E
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	WHERE L.[State] = 4
	AND AC.[Node].IsDescendantOf(@ParentNode) = 1
	AND (@CenterId IS NULL OR E.[CenterId] = @CenterId)
	GROUP BY E.[CenterId], E.[AccountId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[InternalReference], E.[ExternalReference], E.[NotedAgentId], E.[NotedResourceId], E.[NotedDate]

	RETURN
END