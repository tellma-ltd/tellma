CREATE TYPE [dbo].[AttachmentList] AS TABLE (
	[Id]						INT				NOT NULL DEFAULT 0,
	[FileName]					NVARCHAR (255)	NOT NULL,
	[Size]						BIGINT,
	[FileId]					NVARCHAR (50),
	[DocumentIndex]				INT				NOT NULL
);