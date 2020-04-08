CREATE FUNCTION [map].[Attachments]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Attachments]
);
