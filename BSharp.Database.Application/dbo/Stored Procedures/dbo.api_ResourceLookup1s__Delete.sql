CREATE PROCEDURE [dbo].[api_ResourceLookup1s__Delete]
	@Ids [dbo].[IdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dbo].[dal_ResourceLookup1s__Delete] @Ids = @Ids;