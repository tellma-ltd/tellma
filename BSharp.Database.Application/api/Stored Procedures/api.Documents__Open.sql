CREATE PROCEDURE [api].[Documents__Open]
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Open]
		@Ids = @IndexedIds;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Documents_State__Refresh]
		@Ids = @Ids;

	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = @UserId
		;
