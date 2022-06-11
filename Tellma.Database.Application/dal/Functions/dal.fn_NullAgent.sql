CREATE FUNCTION [dal].[fn_NullAgent] ()
RETURNS INT
AS
BEGIN
	RETURN dal.fn_AgentDefinition_Code__Id(N'NULL', N'NULL')
END
