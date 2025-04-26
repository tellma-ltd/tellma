CREATE FUNCTION [dal].[fn_Settings__Calendar]()
RETURNS NCHAR(2)
AS
BEGIN -- this is a hack. Better use field from table settings instead
-- MA: Commented 2025-04-25, as 204 moved to ET calendar
--	IF [dbo].[fn_DB_Name__Country]() = N'ET' AND RIGHT(DB_NAME(), 3) NOT IN ('200','204')
	IF [dbo].[fn_DB_Name__Country]() = N'ET' AND RIGHT(DB_NAME(), 3) NOT IN ('200') 
		RETURN 'ET'
	RETURN 'GC'
END
GO