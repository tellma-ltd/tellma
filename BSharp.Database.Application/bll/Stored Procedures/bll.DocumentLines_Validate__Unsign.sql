CREATE PROCEDURE [bll].[DocumentLines_Validate__Unsign]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot unsign the lines unless the document state is ACTIVE
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsIn0State',
		BE.[State]
	FROM @Ids FE
	JOIN [dbo].[DocumentLines] DL ON FE.[Id] = DL.[Id]
	JOIN [dbo].[Documents] BE ON DL.[DocumentId] = BE.[Id]
	WHERE (BE.[State] <> N'Active');

	SELECT TOP (@Top) * FROM @ValidationErrors;