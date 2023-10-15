CREATE TABLE [dbo].[ResourceAttachments]
(
	[Id]						INT					CONSTRAINT [PK_ResourceAttachments] PRIMARY KEY IDENTITY,
	[ResourceId]				INT					NOT NULL CONSTRAINT [FK_ResourceAttachments__ResourceId] REFERENCES [dbo].[Resources] ([Id]) ON DELETE CASCADE,
	[CategoryId]				INT					NULL CONSTRAINT [FK_ResourceAttachments__CategoryId] REFERENCES [dbo].[Lookups] ([Id]),
	[FileName]					NVARCHAR (255)		NOT NULL,
	[FileExtension]				NVARCHAR (50)		NULL,
	[FileId]					NVARCHAR (50)		NOT NULL, -- Ref to blob storage
	[Size]						BIGINT				NOT NULL,

	-- for auditing
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT	NOT NULL CONSTRAINT [FK_ResourceAttachments__CreatedById]	FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT	NOT NULL CONSTRAINT [FK_ResourceAttachments__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
)
