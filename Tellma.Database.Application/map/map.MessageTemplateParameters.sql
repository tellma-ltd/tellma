CREATE FUNCTION [map].[MessageTemplateParameters] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[MessageTemplateParameters]
);
