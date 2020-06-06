CREATE TABLE [dbo].[ResourceUnitRates]
(
	[Id]						INT					CONSTRAINT [PK_ResourceUnitRates] PRIMARY KEY IDENTITY,
	[ResourceId]				INT					NOT NULL CONSTRAINT [FK_ResourceUnitRates__ResourceId] REFERENCES [dbo].[Resources] ([Id]) ON DELETE CASCADE,
	[UnitId]					INT					NOT NULL CONSTRAINT [FK_ResourceUnitRates__UnitId] REFERENCES [dbo].[Units] ([Id]),
);