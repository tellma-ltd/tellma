CREATE PROCEDURE [bll].[Units_Validate__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;