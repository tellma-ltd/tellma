CREATE FUNCTION [map].[EmailTemplateAttachments] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[EmailTemplateAttachments]
);
