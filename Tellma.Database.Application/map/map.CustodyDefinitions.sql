CREATE FUNCTION [map].[CustodyDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[CustodyDefinitions]
);
