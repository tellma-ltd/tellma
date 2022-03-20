CREATE FUNCTION [map].[EmailTemplates] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[EmailTemplates]
);
