CREATE TABLE [dbo].[ResourceUnits]
(
	[Id]						INT					CONSTRAINT [PK_ResourceUnits] PRIMARY KEY IDENTITY,
	[ResourceId]				INT					NOT NULL CONSTRAINT [FK_ResourceUnits__ResourceId] REFERENCES [dbo].[Resources] ([Id]) ON DELETE CASCADE,
	[UnitId]					INT					NOT NULL CONSTRAINT [FK_ResourceUnits__UnitId] REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[Multiplier]				FLOAT (53)			NOT NULL CONSTRAINT [CK_ResourceUnits__Multiplier] CHECK ([Multiplier] >= 1),
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceUnits__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceUnits__ModifiedById] REFERENCES [dbo].[Users] ([Id])
)
