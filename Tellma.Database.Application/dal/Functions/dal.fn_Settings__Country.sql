CREATE FUNCTION [dal].[fn_Settings__Country]()
RETURNS NCHAR(2)
AS
BEGIN -- this is a hack. Better use field from table settings instead
	RETURN [dbo].[fn_DB_Name__Country]()
END
GO