CREATE FUNCTION [map].[AgentAttachments]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[AgentId],
		[CategoryId],
		[FileName],
		[FileExtension],
		[FileId],
		[Size],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM [dbo].[AgentAttachments]
);