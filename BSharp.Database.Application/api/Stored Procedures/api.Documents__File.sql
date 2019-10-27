CREATE PROCEDURE [api].[Documents__File]
	@Ids dbo.[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
	SET NOCOUNT ON;
	DECLARE @ValidationErrors dbo.[ValidationErrorList];
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__File]
		@Ids = @Ids;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents_State__Update]
		@Ids = @Ids,
		@ToState = N'Filed'
		;

	EXEC [dal].[Documents__Assign]
		@Documents = @Ids,
		@AssigneeId = NULL
		;
