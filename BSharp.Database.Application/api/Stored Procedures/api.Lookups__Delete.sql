CREATE PROCEDURE [api].[Lookups__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	--INSERT INTO @ValidationErrors

	EXEC [dal].[Lookups__Delete] @Ids = @Ids;