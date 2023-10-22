CREATE FUNCTION [dal].[fn_NullCenter] ()
RETURNS INT
AS
BEGIN
	RETURN dal.fn_CenterCode__Id(N'NULL')
END
