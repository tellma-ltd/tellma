CREATE FUNCTION [map].[MessageTemplates] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[MessageTemplates]
);
