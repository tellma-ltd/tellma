CREATE PROCEDURE [bll].[AgentDefinitions_Validate__Save]
	@Entities [AgentDefinitionList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Id must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheCode0IsDuplicated',
		[Id]
	FROM @Entities
	WHERE [Id] IN (
		SELECT [Id] FROM @Entities
		GROUP BY [Id]
		HAVING COUNT(*) > 1
	);

	SELECT TOP (@Top) * FROM @ValidationErrors;