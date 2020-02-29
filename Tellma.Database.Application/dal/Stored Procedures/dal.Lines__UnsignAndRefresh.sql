CREATE PROCEDURE [dal].[Lines__UnsignAndRefresh]
	@Ids [dbo].[IdList] READONLY,
	@ReturnIds BIT = 0
AS
	EXEC [dal].[Lines__Unsign] @Ids = @Ids;

	DECLARE @DocIds dbo.IdList;
	INSERT INTO @DocIds([Id])
	SELECT DISTINCT DocumentId FROM dbo.Lines
	WHERE [Id] IN (SELECT [Id] FROM @Ids);

	EXEC dal.Documents_State__Refresh @DocIds;

	IF @ReturnIds = 1
		SELECT [Id] FROM @DocIds;