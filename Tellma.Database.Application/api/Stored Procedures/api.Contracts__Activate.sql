CREATE PROCEDURE [api].[Contracts__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @Ids [dbo].[IdList];
	-- Add here Code that is handled by C#

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Contracts__Activate] @Ids = @Ids, @IsActive = @IsActive;