CREATE TABLE [dbo].[LineDefinitionsAgentsResourcesCurrencies]
(
	[Id]				INT CONSTRAINT [PK_LineDefinitionsAgentsResourcesCurrencies] PRIMARY KEY IDENTITY,
	[LineDefinitionId]	INT NOT NULL,
	[AgentId]			INT NOT NULL,
	[ResourceId]		INT NOT NULL,
	[CurrencyId]		NCHAR (3) NOT NULL
);
GO
CREATE UNIQUE INDEX IX_LineDefinitionsAgentsResourcesCurrencies ON
	dbo.LineDefinitionsAgentsResourcesCurrencies ([LineDefinitionId], [AgentId], [ResourceId], [CurrencyId]);
GO