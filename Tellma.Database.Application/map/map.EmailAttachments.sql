CREATE FUNCTION [map].[EmailAttachments]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[Index],
		[EmailId],
		[Name],
		[ContentBlobId]
	FROM [dbo].[EmailAttachments]
);