CREATE TABLE [dbo].[AgentRates] (
	[Id]			INT					CONSTRAINT [PK_AgentRates] PRIMARY KEY IDENTITY,
	[AgentId]		INT					NOT NULL CONSTRAINT [FK_AgentRates__AgentId] REFERENCES dbo.Agents([Id]) ON DELETE CASCADE,
	[ResourceId]	INT					NOT NULL CONSTRAINT [FK_AgentRates__ResourceId] REFERENCES dbo.Resources([Id]),
	[UnitId]		INT					NOT NULL CONSTRAINT [FK_AgentRates__UnitId] REFERENCES dbo.[Units]([Id]),
	[Rate]			DECIMAL (19,4)		NOT NULL CONSTRAINT [FK_AgentRates__Rate] CHECK ([Rate] >= 0),
	[CurrencyId]	NCHAR (3)			NOT NULL CONSTRAINT [FK_AgentRates__CurrencyId] REFERENCES dbo.Currencies([Id])
);