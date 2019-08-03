CREATE PROCEDURE [dbo].[api_ResourceLookup1s__Activate]
	@Ids [dbo].[UuidList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dbo].[dal_ResourceLookup1s__Activate] @Ids = @Ids, @IsActive = @IsActive;