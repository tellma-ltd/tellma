CREATE PROCEDURE [api].[Lookups__Delete]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @Ids [dbo].[IdList];
	DECLARE @ValidationErrors ValidationErrorList;
--	INSERT INTO @ValidationErrors
;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Lookups__Delete] @Ids = @Ids;