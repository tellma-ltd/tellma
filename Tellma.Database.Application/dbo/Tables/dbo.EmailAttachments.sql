CREATE TABLE [dbo].[EmailAttachments]
(
	[Id] INT CONSTRAINT [PK_EmailAttachments] PRIMARY KEY IDENTITY,
	[Index] INT NOT NULL,
	[EmailId] INT NOT NULL CONSTRAINT [FK_EmailAttachments__EmailId] REFERENCES [dbo].[Emails] ([Id]) ON DELETE CASCADE,
	[Name] NVARCHAR(1024) NOT NULL, -- File Name
	[ContentBlobId] NVARCHAR(50) -- File key in the blob storage
)
