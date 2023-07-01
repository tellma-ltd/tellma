CREATE FUNCTION [dal].[fn_Settings__GetCalendar]()
RETURNS NCHAR(2)
AS
BEGIN -- this is a hack. Better use field from table settings instead
	RETURN 'GC'
END
