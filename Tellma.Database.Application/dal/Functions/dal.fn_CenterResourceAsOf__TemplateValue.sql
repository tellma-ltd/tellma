CREATE FUNCTION [dal].[fn_HRMCenterResourceAsOf__TemplateValue]
(
	@CenterId INT,
	@ResourceId INT,
	@Time10	DATE
)
RETURNS DECIMAL (19,4)
AS
BEGIN
	DECLARE @CenterNode HIERARCHYID = (SELECT [Node] FROM dbo.Centers WHERE [Id] = @CenterId);
	DECLARE @Result DECIMAL (19,4);
	SELECT @Result = SUM(E.[Direction] * E.[MonetaryValue]) 
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	WHERE AC.[Concept] = N'HRMExtension'
	AND L.[State] = 2
	AND @CenterNode.IsDescendantOf(C.[Node]) = 1
	AND E.[AgentId] IS NULL
	AND E.[ResourceId] = @ResourceId
	AND E.[Time1] <= @Time10
	RETURN @Result
END