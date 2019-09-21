CREATE PROCEDURE [api].[ResourceLookups__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	--INSERT INTO @ValidationErrors

	EXEC [dal].[ResourceLookups__Delete] @Ids = @Ids;