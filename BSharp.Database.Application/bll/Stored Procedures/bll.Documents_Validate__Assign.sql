CREATE PROCEDURE [bll].[Documents_Validate__Assign]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Can only assign while in (Active) state
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsIn0State',
		BE.[State]
	FROM @Ids FE
	JOIN [dbo].[Documents] BE ON FE.[Id] = BE.[Id]
	JOIN [dbo].[DocumentDefinitions] DT ON BE.[DefinitionId] = DT.Id
	WHERE (BE.[State] <> N'Active');

	SELECT TOP (@Top) * FROM @ValidationErrors;