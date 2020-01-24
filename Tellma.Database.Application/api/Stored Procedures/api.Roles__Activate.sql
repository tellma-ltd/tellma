CREATE PROCEDURE [api].[Roles__Activate]
	@IndexedIds  [dbo].[IndexedIdList] READONLY,
	@IsActive BIT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Roles__Activate] @Ids = @Ids, @IsActive = @IsActive;