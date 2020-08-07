CREATE TABLE [dbo].[ResourceUnits]
(
	[Id]						INT					CONSTRAINT [PK_ResourceUnits] PRIMARY KEY IDENTITY,
	[ResourceId]				INT					NOT NULL CONSTRAINT [FK_ResourceUnits__ResourceId] REFERENCES [dbo].[Resources] ([Id]) ON DELETE CASCADE,

	[EntryTypeId]				INT,				-- Added
	[UnitId]					INT					NOT NULL CONSTRAINT [FK_ResourceUnits__UnitId] REFERENCES [dbo].[Units] ([Id]),

	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceUnits__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceUnits__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);