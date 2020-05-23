CREATE PROCEDURE [api].[Lookups__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @Ids [dbo].[IdList];

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds
	EXEC [dal].[Lookups__Activate] @Ids = @Ids, @IsActive = @IsActive;