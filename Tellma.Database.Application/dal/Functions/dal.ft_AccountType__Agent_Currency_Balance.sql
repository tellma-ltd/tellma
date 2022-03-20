CREATE FUNCTION [dal].[ft_AccountType__Agent_Currency_Balance] (
	@AccountTypeConcept NVARCHAR (255),
	@AgentDefinitionCode NVARCHAR(255),
	@CurrencyId NVARCHAR(3),
	@AsOfDate DATE,
	@ParentCenterId INT
)
RETURNS @returntable TABLE
(
	[CenterId]		INT,
	[AgentId]		INT,
	[CurrencyId]	NCHAR (3),
	[MonetaryValue]	DECIMAL (19, 4),
	[Value]			DECIMAL (19, 4)
)
AS
BEGIN
	DECLARE @AccountTypeNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@AccountTypeConcept);
	DECLARE @ParentCenterNode HIERARCHYID = (
		SELECT [Node] FROM dbo.Centers WHERE [Id] = @ParentCenterId
	);
	DECLARE @AgentDefinitionId INT = (
		SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = @AgentDefinitionCode
	);
	WITH MyAccounts AS (
		SELECT A.[Id] 
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE AC.[Node].IsDescendantOf(@AccountTypeNode) = 1

	)
	INSERT @returntable
	SELECT E.[CenterId], E.[AgentId], E.[CurrencyId],
		SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
		SUM(E.[Direction] * E.[Value]) AS [Value]
	FROM map.DetailsEntries() E
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN MyAccounts A ON E.[AccountId] = A.[Id]
	WHERE (@AgentDefinitionCode IS NULL OR AG.[DefinitionId] = @AgentDefinitionId)
	AND (@CurrencyId IS NULL OR E.[CurrencyId] = @CurrencyId)
	AND (@AsOfDate IS NULL OR L.PostingDate <= @AsOfDate)
	AND (L.[State] >= 3)
	GROUP BY E.[CenterId], E.[AgentId], E.[CurrencyId]
	--HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0;
	RETURN
END;