CREATE PROCEDURE [api].[Relations__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @Ids [dbo].[IdList];
	-- Add here Code that is handled by C#
	DECLARE @ValidationErrors ValidationErrorList;
	--INSERT INTO @ValidationErrors

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Relations__Activate] @Ids = @Ids, @IsActive = @IsActive;