CREATE FUNCTION [dbo].[fn_FunctionalCurrency]()
RETURNS NCHAR (3)
AS
BEGIN
	DECLARE @Result  NCHAR (3);

	SELECT @Result = FunctionalCurrency
	FROM dbo.Settings
	
	RETURN @Result;
END;