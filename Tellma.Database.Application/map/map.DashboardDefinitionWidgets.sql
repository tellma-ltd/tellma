CREATE FUNCTION [map].[DashboardDefinitionWidgets]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DashboardDefinitionWidgets]
);
