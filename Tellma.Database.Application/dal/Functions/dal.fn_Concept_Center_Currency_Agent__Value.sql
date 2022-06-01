CREATE FUNCTION [dal].[fn_Concept_Center_Currency_Agent__Value]
(
	@ParentConcept NVARCHAR (255),
	@CenterId INT,
	@AgentId INT,
	@ResourceId INT,
	@NotedDate DATE
)
RETURNS DECIMAL (19,4)
AS BEGIN
DECLARE @Result  DECIMAL (19,4), @ParentNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept);
	SELECT @Result = SUM(E.[Direction] * E.[Value]) 
	FROM dbo.Entries E
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	WHERE L.[State] = 4
	AND AC.[Node].IsDescendantOf(@ParentNode) = 1
	AND (E.[CenterId] = @CenterId)
	AND (E.[AgentId] = @AgentId)
	AND (@ResourceId IS NULL		AND E.[ResourceId] IS NULL		OR E.[ResourceId] = @ResourceId)
	AND (@NotedDate IS NULL			AND [NotedDate] IS NULL			OR [NotedDate] = @NotedDate)
	RETURN ISNULL(@Result, 0)
END