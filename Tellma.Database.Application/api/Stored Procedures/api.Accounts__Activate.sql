CREATE PROCEDURE [api].[Accounts__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @Ids dbo.IdList;
	-- Add here Code that is handled by C#

	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Accounts_Validate__Activate]
		@Ids = @IndexedIds,
		@IsActive = @IsActive;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Accounts__Activate]
		@Ids = @Ids,
		@IsActive = @IsActive;
END;