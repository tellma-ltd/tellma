CREATE PROCEDURE [dbo].[api_Roles__Activate]
	@Ids [dbo].[UuidList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dbo].[dal_Roles__Activate] @Ids = @Ids, @IsActive = @IsActive;