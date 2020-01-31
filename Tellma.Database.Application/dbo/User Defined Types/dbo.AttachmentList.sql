CREATE TYPE [dbo].[AttachmentList] AS TABLE (
	[Id]						INT				NOT NULL DEFAULT 0,
	[FileName]					NVARCHAR (255)	NOT NULL,
	[FileExtension]				NVARCHAR (50),
	[Size]						BIGINT,
	[FileId]					NVARCHAR (50),
	[DocumentIndex]				INT				NOT NULL
);