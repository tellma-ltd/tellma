CREATE TABLE [dbo].[ExternalEntries] (
	[Id]						INT				CONSTRAINT [PK_ExternalEntries] PRIMARY KEY IDENTITY,
	[PostingDate]				DATE			CONSTRAINT [CK_ExternalEntries__PostingDate] CHECK ([PostingDate] < DATEADD(DAY, 1, GETDATE())),
	[Direction]					SMALLINT		NOT NULL CONSTRAINT [CK_ExternalEntries__Direction]	CHECK ([Direction] IN (-1, 1)),
	[AccountId]					INT				CONSTRAINT [FK_ExternalEntries__AccountId] REFERENCES [dbo].[Accounts] ([Id]),
	[RelationId]				INT				CONSTRAINT [FK_ExternalEntries__RelationId] REFERENCES dbo.Relations([Id]),
	[MonetaryValue]				DECIMAL (19,4),
	[ExternalReference]			NVARCHAR (255),
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ExternalEntries__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ExternalEntries__ModifiedById] REFERENCES [dbo].[Users] ([Id]),	
);
GO
CREATE INDEX [IX_ExternalEntries__AccountId] ON [dbo].[ExternalEntries]([AccountId]);
GO
CREATE INDEX [IX_ExternalEntries__RelationId] ON [dbo].[ExternalEntries]([RelationId]);
GO