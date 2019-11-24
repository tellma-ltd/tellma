CREATE PROCEDURE [api].[Documents__File]
-- TODO: Call it [api].[Documents__Close]
-- If all states are negative, it goes to VOID
-- While in process, the document is in state Active/InProcess/Pending
-- if at least one state is REVIEWED, it goes to Posted/Archived/Filed/Adjourned/WoundUp/Finished/Locked

-- Upon signing lines, the system can also change the document state automatically. 
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__File]
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
	EXEC [dal].[Documents_State__Update]
		@Ids = @Ids,
		@ToState = N'Filed'
		;

	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL
		;
