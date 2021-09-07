CREATE FUNCTION [map].[Attachments]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[DocumentId],
		[FileName],
		[FileExtension],
		[FileId],
		[Size],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM [dbo].[Attachments]
);
