CREATE PROCEDURE [api].[Views__Activate]
	@Ids [dbo].[ViewList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	--INSERT INTO @ValidationErrors
	--EXEC [bll].[Views_Validate__Activate]
	--	@Ids = @Ids,
	--	@IsActive = @IsActive

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Views__Activate] @Ids = @Ids, @IsActive = @IsActive;