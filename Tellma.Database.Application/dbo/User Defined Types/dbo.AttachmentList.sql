CREATE TYPE [dbo].[AttachmentList] AS TABLE (
	[DocumentIndex]				INT,
	[Id]						INT				NOT NULL DEFAULT 0,
	[FileName]					NVARCHAR (255),
	[FileExtension]				NVARCHAR (50),
	[FileId]					NVARCHAR (50),
	[Size]						BIGINT
);