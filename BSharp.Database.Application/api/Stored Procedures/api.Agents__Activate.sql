CREATE PROCEDURE [api].[Agents__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dal].[Agents__Activate] @Ids = @Ids, @IsActive = @IsActive;