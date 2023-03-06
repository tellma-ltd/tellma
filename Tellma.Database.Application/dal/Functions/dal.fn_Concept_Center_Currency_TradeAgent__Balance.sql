CREATE FUNCTION [dal].[fn_Concept_Center_Currency_TradeAgent__Balance]
(
	@ParentConcept NVARCHAR (255),
	@ParentCenterId INT,
	@CurrencyId NCHAR (3),
	@AgentId INT,
	@NotedDate DATE
)
RETURNS DECIMAL (19,4)
AS BEGIN
DECLARE @Result  DECIMAL (19,4), @ParentNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept);
	SELECT @Result = SUM([Balance])
	FROM [dal].[ft_Concept_Center__TradeAgents_Balances](@ParentConcept, @ParentCenterId) T
	JOIN dbo.Agents AG ON AG.[Id] = T.[AgentId]
	WHERE T.[CurrencyId] = @CurrencyId
	AND T.[AgentId] = @AgentId
	AND (@NotedDate IS NULL			AND [NotedDate] IS NULL			OR AG.[ToDate] <= @NotedDate)
	RETURN ISNULL(@Result, 0)
END
GO
