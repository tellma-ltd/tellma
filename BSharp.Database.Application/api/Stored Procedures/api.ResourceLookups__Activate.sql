CREATE PROCEDURE [api].[ResourceLookups__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dal].[ResourceLookups__Activate] @Ids = @Ids, @IsActive = @IsActive;