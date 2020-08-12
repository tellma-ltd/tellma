CREATE FUNCTION [map].[DocumentDefinitionMarkupTemplates]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DocumentDefinitionMarkupTemplates]
);