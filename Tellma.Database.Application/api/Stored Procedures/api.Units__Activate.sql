CREATE PROCEDURE [api].[Units__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @Ids dbo.IdList;
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;

	DECLARE @ValidationErrors ValidationErrorList;
	--INSERT INTO @ValidationErrors
;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);


	EXEC [dal].[Units__Activate] @Ids = @Ids, @IsActive = @IsActive;