CREATE FUNCTION [dal].[fn_Concept_Center_Currency_Agent__Balance]
(
	@ParentConcept NVARCHAR (255),
	@CenterId INT,
	@CurrencyId NCHAR (3),
	@AgentId INT,
	@ResourceId INT,
	@InternalReference NVARCHAR (50),
	@ExternalReference NVARCHAR (50),
	@NotedAgentId INT,
	@NotedResourceId INT,
	@NotedDate DATE
)
RETURNS DECIMAL (19,4)
AS BEGIN
DECLARE @Result  DECIMAL (19,4), @ParentNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept);
	SELECT @Result = [Balance] 
	FROM [dal].[ft_Concept_Center__Agents_Balances](@ParentConcept, @CenterId) 
	WHERE [CurrencyId] = @CurrencyId
	AND [AgentId] = @AgentId
	AND (@ResourceId IS NULL		AND [ResourceId] IS NULL		OR [ResourceId] = @ResourceId)
	AND (@NotedAgentId IS NULL		AND [NotedAgentId] IS NULL		OR [NotedAgentId] = @NotedAgentId)
	AND (@NotedResourceId IS NULL	AND [NotedResourceId] IS NULL	OR [NotedResourceId] = @NotedResourceId)
	AND (@NotedDate IS NULL			AND [NotedDate] IS NULL			OR [NotedDate] = @NotedDate)
	AND (@InternalReference IS NULL AND [InternalReference] IS NULL	OR [InternalReference] = @InternalReference)
	AND (@ExternalReference IS NULL AND [ExternalReference] IS NULL	OR [ExternalReference] = @ExternalReference)
	RETURN ISNULL(@Result, 0)
END