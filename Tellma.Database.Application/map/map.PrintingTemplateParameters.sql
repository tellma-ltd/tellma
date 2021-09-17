CREATE FUNCTION [map].[PrintingTemplateParameters]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[PrintingTemplateParameters]
);
