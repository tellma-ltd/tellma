CREATE PROCEDURE [bll].[Lines_Validate__Unsign]
-- TODO: Will pass signature ids instead of Line Ids
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot unsign the lines unless the document state is ACTIVE
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		CASE
			WHEN D.[State] = 1 THEN N'Error_TheDocumentIsInPostedState'
			WHEN D.[State] = -1 THEN N'Error_TheDocumentIsInCancelledState'
		END
	FROM @Ids FE
	JOIN [dbo].[Lines] L ON FE.[Id] = L.[Id]
	JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
	WHERE (D.[State] <> 0);

	-- TODO: cannot unsign unless it was part of the last transition

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	SELECT TOP (@Top) * FROM @ValidationErrors;