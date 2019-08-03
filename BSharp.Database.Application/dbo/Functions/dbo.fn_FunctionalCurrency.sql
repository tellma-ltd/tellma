CREATE FUNCTION [dbo].[fn_FunctionalCurrency]()
RETURNS INT
AS
BEGIN
	DECLARE @Result INT;

	SELECT @Result = FunctionalCurrencyId
	FROM dbo.Settings
	
	RETURN @Result;
END;