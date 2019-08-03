CREATE PROCEDURE [dbo].[api_Documents__Unsign]
	@Documents [dbo].[UuidList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	-- if all documents are already unsigned, return
	IF NOT EXISTS(
		SELECT * FROM [dbo].[DocumentSignatures]
		WHERE [DocumentId] IN (SELECT [Id] FROM @Documents)
		AND [RevokedById] IS NULL
	)
		RETURN;

	-- Validate, checking available signatures for transaction type
	EXEC [dbo].[bll_Documents_Validate__Unsign]
		@Entities = @Documents,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_Documents__Unsign] @Documents = @Documents;
	
	-- get the documents whose state will change
	DECLARE @TransitionedIds [dbo].[UiidWithStateList];
	/*
	INSERT INTO @TransitionedIds([Id])
	EXEC [dbo].[bll_Documents_State__Select]
	*/
	IF EXISTS(SELECT * FROM @TransitionedIds)
		EXEC dal_Documents_State__Update @Entities = @TransitionedIds
END;