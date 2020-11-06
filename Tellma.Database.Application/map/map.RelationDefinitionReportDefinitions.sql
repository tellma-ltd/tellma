CREATE FUNCTION [map].[RelationDefinitionReportDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[RelationDefinitionReportDefinitions]
);
