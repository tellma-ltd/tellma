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
	EXEC [dal].[Lines__UnsignAndRefresh] @Ids = @Ids;
END;