CREATE TABLE [dbo].[AgentRates] (
	[Id]			INT					CONSTRAINT [PK_AgentRates] PRIMARY KEY IDENTITY,
	[AgentId]		INT					CONSTRAINT [FK_AgentRates__AgentId] REFERENCES dbo.Agents([Id]),
	[ResourceId]	INT					CONSTRAINT [FK_AgentRates__ResourceId] REFERENCES dbo.Resources([Id]),
	[UnitId]		INT					CONSTRAINT [FK_AgentRates__UnitId] REFERENCES dbo.MeasurementUnits([Id]),
	[Rate]			DECIMAL (19,4)		CONSTRAINT [FK_AgentRates__Rate] CHECK ([Rate] >= 0),
	[CurrencyId]	NCHAR (3)			CONSTRAINT [FK_AgentRates__CurrencyId] REFERENCES dbo.Currencies([Id])
);