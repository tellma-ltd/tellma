CREATE TABLE [dbo].[LineDefinitionLineKeys]
(
	[Id]				INT CONSTRAINT [PK_LineDefinitionLineKeyss] PRIMARY KEY IDENTITY,
	[LineDefinitionId]	INT NOT NULL,
	[EntryIndex]		INT NOT NULL,
	[CenterId]			INT,
	[CurrencyId]		NCHAR (3),
	[AgentId]			INT,
	[ResourceId]		INT,
	[NotedAgentId]		INT,
	[NotedResourceId]	INT, 
    [Decimal1]			DECIMAL (19,6) NOT NULL
);
GO
CREATE UNIQUE INDEX IX_LineDefinitionsAgentsResourcesCurrencies ON
	dbo.LineDefinitionLineKeys ([LineDefinitionId], [EntryIndex], [CenterId], [CurrencyId], [AgentId], [ResourceId], [NotedAgentId], [NotedResourceId], [Decimal1]);
GO