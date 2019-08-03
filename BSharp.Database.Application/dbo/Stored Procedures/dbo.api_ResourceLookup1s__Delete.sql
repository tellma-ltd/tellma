CREATE PROCEDURE [dbo].[api_ResourceLookup1s__Delete]
	@Ids [dbo].[UuidList] READONLY,
	@IsDeleted BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dbo].[dal_ResourceLookup1s__Delete] @Ids = @Ids, @IsDeleted = @IsDeleted;