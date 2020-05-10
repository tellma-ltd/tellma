CREATE PROCEDURE [api].[Documents__Open]
	@DefinitionId INT,
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Open]
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
	EXEC [dal].[Documents_State__Update] @Ids = @Ids, @State = 0;

	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = @UserId
		;
