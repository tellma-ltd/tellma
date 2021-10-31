CREATE TYPE [dbo].[EmailAttachmentList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Name]						NVARCHAR (1024),
	[ContentBlobId]				NVARCHAR (50)
);