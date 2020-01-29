CREATE TABLE [dbo].[Attachments]
(
	[Id]						INT					CONSTRAINT [PK_Attachments] PRIMARY KEY IDENTITY,
	[DocumentId]				INT					NOT NULL CONSTRAINT [FK_Attachments__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[FileName]					NVARCHAR (255)		NOT NULL,
	[FileId]					NVARCHAR (50)		NOT NULL, -- Ref to blob storage
	[Size]						BIGINT				NOT NULL,

	-- for auditing
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET() CONSTRAINT [FK_Attachments__CreatedById]	FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	[CreatedById]				INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Attachments__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),

)
