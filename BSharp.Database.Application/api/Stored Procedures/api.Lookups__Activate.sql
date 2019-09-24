CREATE PROCEDURE [api].[Lookups__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dal].[Lookups__Activate] @Ids = @Ids, @IsActive = @IsActive;