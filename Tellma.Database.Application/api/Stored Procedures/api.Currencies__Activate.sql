CREATE PROCEDURE [api].[Currencies__Activate]
	@IndexedIds [dbo].[IndexedStringList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors ValidationErrorList;
--	INSERT INTO @ValidationErrors
;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	DECLARE @Ids dbo.StringList;
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Currencies__Activate] @Ids = @Ids, @IsActive = @IsActive;