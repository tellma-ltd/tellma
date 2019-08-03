CREATE PROCEDURE [dbo].[api_Views__Activate]
	@Ids [dbo].[ViewList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dbo].[dal_Views__Activate] @Ids = @Ids, @IsActive = @IsActive;