CREATE FUNCTION [dal].[fn_Settings__Calendar]()
RETURNS NCHAR(2)
AS
BEGIN -- this is a hack. Better use field from table settings instead
	IF [dbo].[fn_DB_Name__Country]() = N'ET' AND RIGHT(DB_NAME(), 3) NOT IN ('204')
		RETURN 'ET'
	RETURN 'GC'
END
GO