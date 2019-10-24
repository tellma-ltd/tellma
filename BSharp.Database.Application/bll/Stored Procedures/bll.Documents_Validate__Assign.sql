CREATE PROCEDURE [bll].[Documents_Validate__Assign]
	@Entities [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot assign while in final state
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsIn0State',
		BE.[State]
	FROM @Entities FE
	JOIN [dbo].[Documents] BE ON FE.[Id] = BE.[Id]
	JOIN [dbo].[DocumentDefinitions] DT ON BE.[DocumentDefinitionId] = DT.Id
	WHERE (BE.[State] IN (N'Void', N'Posted'));

	SELECT TOP (@Top) * FROM @ValidationErrors;