CREATE PROCEDURE [api].[Lookups__Delete]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];

	--INSERT INTO @ValidationErrors

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Lookups__Delete] @Ids = @Ids;