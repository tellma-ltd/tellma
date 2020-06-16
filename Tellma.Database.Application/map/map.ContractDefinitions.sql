CREATE FUNCTION [map].[ContractDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ContractDefinitions]
);
