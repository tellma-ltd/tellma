CREATE PROCEDURE [bll].[Lines_Validate__Unsign]
-- TODO: Will pass signature ids instead of Line Ids
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot unsign the lines unless the document state is ACTIVE
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		CASE
			WHEN D.[PostingState] = 1 THEN N'Error_TheDocumentIsInPostedState'
			WHEN D.[PostingState] = -1 THEN N'Error_TheDocumentIsInCancelledState'
		END
	FROM @Ids FE
	JOIN [dbo].[Lines] L ON FE.[Id] = L.[Id]
	JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
	WHERE (D.[PostingState] <> 0);

	-- TODO: cannot unsign unless it was part of the last transition


	SELECT TOP (@Top) * FROM @ValidationErrors;