CREATE TYPE [dbo].[ResourceAttachmentList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[CategoryId]				INT,
	[FileName]					NVARCHAR (255),
	[FileExtension]				NVARCHAR (50),
	[FileId]					NVARCHAR (50),
	[Size]						BIGINT
);