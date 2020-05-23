CREATE PROCEDURE [api].[AccountClassifications__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @Ids dbo.IdList;
	-- Add here Code that is handled by C#

	EXEC [bll].[AccountClassifications_Validate__Activate]
		@Ids = @IndexedIds,
		@IsActive = @IsActive,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[AccountClassifications__Activate]
		@Ids = @Ids,
		@IsActive = @IsActive;
END;