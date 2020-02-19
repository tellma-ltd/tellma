CREATE TABLE [dbo].[Registers]
(
	[Id]							INT					CONSTRAINT [PK_Registers] PRIMARY KEY IDENTITY,
	[DocumentId]					INT					CONSTRAINT [FK_Registers__DocumentId] REFERENCES dbo.Documents([Id]),
	[AccountId]						INT					NOT NULL CONSTRAINT [FK_Registers__AccountId] REFERENCES dbo.Accounts([Id]),
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Registers__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Registers__ModifiedById] REFERENCES [dbo].[Users] ([Id])
)
