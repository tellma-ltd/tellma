CREATE PROCEDURE [api].[Roles__Activate]
	@IndexedIds  [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Roles__Activate] @Ids = @Ids, @IsActive = @IsActive;