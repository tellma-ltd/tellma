CREATE PROCEDURE [dbo].[api_ResourceLookup1s__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	--INSERT INTO @ValidationErrors

	EXEC [dbo].[dal_ResourceLookup1s__Delete] @Ids = @Ids;