CREATE FUNCTION [map].[DashboardDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DashboardDefinitions]
);
