CREATE PROCEDURE [dbo].[api_DocumentLines__Unsign]
	@Documents [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	-- if all documents are already unsigned, return
	IF NOT EXISTS(
		SELECT * FROM [dbo].[DocumentLineSignatures]
		WHERE [DocumentLineId] IN (SELECT [Id] FROM @Documents)
		AND [RevokedById] IS NULL
	)
		RETURN;

	-- Validate, checking available signatures for transaction type
	INSERT INTO @ValidationErrors
	EXEC [bll].[DocumentLines_Validate__Unsign]
		@Entities = @Documents;;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[DocumentLines__Unsign] @Documents = @Documents;
	
	-- get the documents whose state will change
	DECLARE @TransitionedIds [dbo].[IdWithStateList];
	/*
	INSERT INTO @TransitionedIds([Id])
	EXEC [dbo].[bll_Documents_State__Select]
	*/
	IF EXISTS(SELECT * FROM @TransitionedIds)
		EXEC [dal].[DocumentLines_State__Update] @Ids = @TransitionedIds
END;