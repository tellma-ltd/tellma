CREATE FUNCTION [dal].[ft_Concept_Center_Agent_Resource__Balances]
(-- TODO: to be replaced with the version that excludes the current document
	@ParentConcept NVARCHAR (255),
	@ParentCenterId INT,
	@AgentId INT,
	@ResourceId INT,
	@CurrencyId NCHAR (3),
	@AsOf DATE
)
RETURNS @ResultTable TABLE (
	[Quantity] DECIMAL (19, 4),
	[MonetaryValue] DECIMAL (19, 4),
	[Value] DECIMAL (19, 4)
)
AS BEGIN
	DECLARE @ParentNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept);
	DECLARE @ParentCenterNode HIERARCHYID = dal.fn_Center__Node(@ParentCenterId);

	INSERT INTO @ResultTable([Quantity], [MonetaryValue], [Value])
	SELECT SUM(E.[Direction] * bll.fn_Resource_EntryQuantity_EntryUnit__ResourceQuantity(E.[ResourceId], E.[Quantity], E.[UnitId])),
			SUM(E.[Direction] * E.[MonetaryValue]) , SUM(E.[Direction] * E.[Value]) 
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	WHERE --L.[State] = 4
		(L.[State] = 4 OR L.[State] >= 0 AND E.[Direction] * E.[Quantity] < 0)
	AND (AC.[Node].IsDescendantOf(@ParentNode) = 1)
	AND (@ParentCenterId IS NULL OR C.[Node].IsDescendantOf(@ParentCenterNode) = 1)
	AND (E.[AgentId] = @AgentId)
	AND (E.[ResourceId] = @ResourceId)
	AND (E.[CurrencyId] = @CurrencyId)
	AND (L.[PostingDate] <= @AsOf)

	RETURN
END
GO