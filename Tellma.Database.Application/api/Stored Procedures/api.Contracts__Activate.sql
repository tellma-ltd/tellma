CREATE PROCEDURE [api].[Contracts__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Contracts__Activate] @Ids = @Ids, @IsActive = @IsActive;