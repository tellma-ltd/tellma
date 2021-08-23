CREATE PROCEDURE [bll].[Currencies_Validate__Activate]
	@Ids [dbo].[StringList] READONLY,
	@IsActive BIT,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Validation goes here
	-- C# validates that functional currency remains active

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;