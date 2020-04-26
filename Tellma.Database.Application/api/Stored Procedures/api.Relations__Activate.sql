CREATE PROCEDURE [api].[Relations__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Relations__Activate] @Ids = @Ids, @IsActive = @IsActive;