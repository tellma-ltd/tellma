CREATE FUNCTION [rpt].[Agents] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Agents]
);
