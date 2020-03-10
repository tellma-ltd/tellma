CREATE PROCEDURE [api].[Documents__Cancel]
	@DefinitionId NVARCHAR(50),
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Cancel]
		@DefinitionId = @DefinitionId,
		@Ids = @IndexedIds;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	DECLARE @Ids [dbo].[IdList];
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Documents_PostingState__Update] @Ids = @Ids, @PostingState = -1;

	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL;
