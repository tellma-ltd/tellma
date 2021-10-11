CREATE FUNCTION [map].[PrintingTemplates] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[PrintingTemplates]
);
