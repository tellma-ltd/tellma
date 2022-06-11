CREATE FUNCTION [dal].[fn_NullResource] ()
RETURNS INT
AS
BEGIN
	RETURN dal.fn_ResourceDefinition_Code__Id(N'NULL', N'NULL')
END
