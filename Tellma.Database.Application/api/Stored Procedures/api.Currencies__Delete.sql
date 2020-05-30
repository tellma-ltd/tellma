CREATE PROCEDURE [api].[Currencies__Delete]
	@IndexedIds [dbo].[IndexedStringList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @Ids [dbo].[StringList];
	-- Add here Code that is handled by C#

	EXEC [bll].[Currencies_Validate__Delete]
		@Ids = @IndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Currencies__Delete] @Ids = @Ids;