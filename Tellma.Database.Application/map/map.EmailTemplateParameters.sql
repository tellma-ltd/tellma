CREATE FUNCTION [map].[EmailTemplateParameters] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[EmailTemplateParameters]
);
