CREATE FUNCTION [dal].[fn_Agent__Citizenship](
	@AgentId INT
)
-- Note: Citizenship is ubiquitous, and is better to have its own separate column, since labor law logic is based on it
RETURNS NCHAR (3)
AS BEGIN
	DECLARE @MyResult NCHAR (3);
	SELECT @MyResult = dal.fn_Lookup__Code([Lookup2Id]) FROM dbo.Agents WHERE [Id] = @AgentId
	RETURN @MyResult
END
GO