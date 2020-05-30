CREATE PROCEDURE [bll].[LineSignatures_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot delete a signature unless the document state is ACTIVE
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		CASE
			WHEN D.[State] = 1 THEN N'Error_TheDocumentIsInPostedState'
			WHEN D.[State] = -1 THEN N'Error_TheDocumentIsInCancelledState'
		END
	FROM @Ids FE
	JOIN dbo.[LineSignatures] LS ON FE.[Id] = LS.[Id]
	JOIN [dbo].[Lines] L ON LS.[LineId] = L.[Id]
	JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
	WHERE (D.[State] <> 0);

	-- cannot unsign if there are subsequent signatures
		INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_SubsequentSignaturesMustBeDeletedFirst'
	FROM @Ids FE
	JOIN dbo.[LineSignatures] LS1 ON FE.[Id] = LS1.[Id]
	JOIN dbo.[LineSignatures] LS2 ON LS1.[LineId] = LS2.[LineId] AND LS2.[SignedAt] > LS1.[SignedAt]
	WHERE LS2.RevokedById IS NULL

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	SELECT TOP (@Top) * FROM @ValidationErrors;