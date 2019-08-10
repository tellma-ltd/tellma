CREATE PROCEDURE [dbo].[api_Roles__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT
AS
SET NOCOUNT ON;
	EXEC [dbo].[dal_Roles__Activate] @Ids = @Ids, @IsActive = @IsActive;