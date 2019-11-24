CREATE PROCEDURE [api].[Documents__Void]
-- TODO: merge it with api_Documents__Close --> 
-- all final states are negative
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
-- Must move all lines to final negative states.
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];
	--INSERT INTO @ValidationErrors
	--EXEC [bll].[Documents_Validate__Void]
	--	@Ids = @IndexedIds;

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
		@ToState = N'Void'
		;

	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL
		;
