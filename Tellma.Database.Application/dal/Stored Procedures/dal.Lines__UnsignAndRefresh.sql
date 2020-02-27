CREATE PROCEDURE [dal].[Lines__UnsignAndRefresh]
	@Ids [dbo].[IdList] READONLY,
	@ReturnIds BIT = 0
AS
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
	WHERE [Id] IN (SELECT [Id] FROM @Ids);

	EXEC dal.Documents_State__Refresh @DocIds;

	IF @Returnids = 1
		SELECT [Id] FROM @DocIds;