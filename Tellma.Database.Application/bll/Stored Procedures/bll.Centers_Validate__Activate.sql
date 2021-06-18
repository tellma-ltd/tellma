CREATE PROCEDURE [bll].[Centers_Validate__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Validation goes here

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;