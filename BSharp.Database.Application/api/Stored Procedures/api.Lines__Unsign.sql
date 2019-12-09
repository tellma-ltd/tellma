CREATE PROCEDURE [api].[Lines__Unsign]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];
	-- if all documents are already unsigned, return
	IF NOT EXISTS(
		SELECT * FROM [dbo].[LineSignatures]
		WHERE [LineId] IN (SELECT [Id] FROM @IndexedIds)
		AND [RevokedById] IS NULL
	)
		RETURN;

	-- Validate, checking available signatures for transaction type
	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__Unsign]
		@Ids = @IndexedIds;;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Lines__Unsign] @Ids = @Ids;
	
	-- get the lines whose state will change
	DECLARE @TransitionedIds [dbo].[IdWithStateList];
	/*
	INSERT INTO @TransitionedIds([Id])
	EXEC [dbo].[bll_Documents_State__Select]
	*/
	IF EXISTS(SELECT * FROM @TransitionedIds)
		EXEC [dal].[Lines_State__Update] @Ids = @TransitionedIds

	DECLARE @DocIds dbo.IdList;
	INSERT INTO @DocIds([Id])
	SELECT DISTINCT DocumentId FROM dbo.Lines
	WHERE [Id] IN (SELECT [Id] FROM @IndexedIds);

	EXEC dal.Documents_State__Refresh @DocIds;
END;