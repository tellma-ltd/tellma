CREATE FUNCTION [dbo].[fn_FunctionalCurrencyId]()
RETURNS NCHAR (3)
AS
-- TODO: Should we change the schema to dal?
BEGIN
	DECLARE @Result  NCHAR (3);

	SELECT @Result = [FunctionalCurrencyId]
	FROM [dbo].[Settings]
	
	RETURN @Result;
END;