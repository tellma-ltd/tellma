CREATE FUNCTION [map].[MarkupTemplates] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[MarkupTemplates]
);
