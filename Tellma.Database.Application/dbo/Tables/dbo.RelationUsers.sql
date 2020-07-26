CREATE TABLE [dbo].[RelationUsers] (
	[Id]				INT					CONSTRAINT [PK_RelationUsers] PRIMARY KEY IDENTITY,
	[RelationId]		INT					NOT NULL CONSTRAINT [FK_RelationUsers__RelationId] REFERENCES dbo.[Relations]([Id]) ON DELETE CASCADE,
	[UserId]			INT					NOT NULL CONSTRAINT [FK_RelationUsers__UserId] REFERENCES dbo.Users([Id]),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_RelationUsers__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_RelationUsers__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);