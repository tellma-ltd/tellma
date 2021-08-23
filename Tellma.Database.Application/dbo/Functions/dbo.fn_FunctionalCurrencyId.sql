CREATE FUNCTION [dbo].[fn_FunctionalCurrencyId]()
RETURNS NCHAR (3)
AS
BEGIN
	DECLARE @Result  NCHAR (3);

	SELECT @Result = [FunctionalCurrencyId]
	FROM [dbo].[Settings]
	
	RETURN @Result;
END;