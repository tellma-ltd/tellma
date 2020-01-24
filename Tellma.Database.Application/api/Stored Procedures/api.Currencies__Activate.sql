CREATE PROCEDURE [api].[Currencies__Activate]
	@IndexedIds [dbo].[IndexedStringList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @Ids dbo.StringList;
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Currencies__Activate] @Ids = @Ids, @IsActive = @IsActive;