CREATE FUNCTION [dal].[fn_TaxAgentId] (
	@TaxAgentCode NVARCHAR (50)
)
RETURNS INT
AS
BEGIN
DECLARE @TaxDepartmentAD INT = (SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = N'TaxDepartment');
	RETURN (
		SELECT [Id] FROM dbo.Agents
		WHERE [DefinitionId] = @TaxDepartmentAD
		AND [Code] = @TaxAgentCode
	)
END;