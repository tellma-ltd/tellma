CREATE PROCEDURE [api].[Views__Activate]
	@Ids [dbo].[ViewList] READONLY,
	@IsActive BIT
AS
SET NOCOUNT ON;
	EXEC [dal].[Views__Activate] @Ids = @Ids, @IsActive = @IsActive;