CREATE FUNCTION [map].[ContractUsers] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ContractUsers]
);