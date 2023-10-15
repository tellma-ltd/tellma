CREATE FUNCTION [map].[ResourceAttachments]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[ResourceId],
		[CategoryId],
		[FileName],
		[FileExtension],
		[FileId],
		[Size],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM [dbo].[ResourceAttachments]
);