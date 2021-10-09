CREATE FUNCTION [dal].[ft_Time1__Center_Agent_Resource_Balance] (
	@AccountTypeConcept NVARCHAR (255),
	@AgentDefinitionCode NVARCHAR(255),
	@ResourceDefinitionCode NVARCHAR(255),
	@AsOfDate DATE,
	@ParentCenterId INT
)
RETURNS @returntable TABLE
(
	[CenterId]		INT,
	[AgentId]		INT,
	[ResourceId]	INT,
	[AsOf]			DATE,
	[Quantity]		DECIMAL (19, 4),
	[MonetaryValue]	DECIMAL (19, 4),
	[Value]			DECIMAL (19, 4)
)
AS
BEGIN
	DECLARE @ParentCenterNode HIERARCHYID = (
		SELECT [Node] FROM dbo.Centers WHERE [Id] = @ParentCenterId
	);
	DECLARE @AgentDefinitionId INT = (
		SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = @AgentDefinitionCode
	);
	DECLARE @ResourceDefinitionId INT = (
		SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = @ResourceDefinitionCode
	);

	INSERT @returntable
	SELECT E.[CenterId], E.[AgentId], E.[ResourceId], MAX(E.[Time1]) AS AsOf, 
		SUM(E.[Direction] * E.[BaseQuantity]) AS [Quantity],
		SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
		SUM(E.[Direction] * E.[Value]) AS [Value]
	FROM map.DetailsEntries() E
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	WHERE AC.[Concept] = @AccountTypeConcept
	AND (@AsOfDate IS NULL OR E.[Time1] <= @AsOfDate)
	AND (@ParentCenterId IS NULL OR C.[Node].IsDescendantOf(@ParentCenterNode) = 1)
	AND (@AgentDefinitionCode IS NULL OR AG.[DefinitionId] = @AgentDefinitionId)
	AND (@ResourceDefinitionCode IS NULL OR R.[DefinitionId] = @ResourceDefinitionId)
	AND L.[State] = 2 -- or 4?
	GROUP BY E.[CenterId], E.[AgentId], E.[ResourceId]
	HAVING SUM(E.[Direction] * E.[BaseQuantity]) <> 0;
	RETURN
END