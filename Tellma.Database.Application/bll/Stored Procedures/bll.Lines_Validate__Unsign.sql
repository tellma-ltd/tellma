CREATE PROCEDURE [bll].[Lines_Validate__Unsign]
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
		CAST(BE.[State] AS NVARCHAR(50))
	FROM @Ids FE
	JOIN [dbo].[Lines] DL ON FE.[Id] = DL.[Id]
	JOIN [dbo].[Documents] BE ON DL.[DocumentId] = BE.[Id]
	--WHERE (BE.[State] <> N'Active');
	WHERE (BE.[State] = 5);

	SELECT TOP (@Top) * FROM @ValidationErrors;