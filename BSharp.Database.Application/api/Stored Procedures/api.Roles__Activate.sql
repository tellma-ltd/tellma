CREATE PROCEDURE [api].[Roles__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT
AS
SET NOCOUNT ON;
	EXEC [dal].[Roles__Activate] @Ids = @Ids, @IsActive = @IsActive;