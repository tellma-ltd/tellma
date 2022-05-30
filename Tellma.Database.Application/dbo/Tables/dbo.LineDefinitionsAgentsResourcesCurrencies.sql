CREATE TABLE [dbo].[LineDefinitionLineKeys]
(
	[Id]				INT CONSTRAINT [PK_LineDefinitionLineKeyss] PRIMARY KEY IDENTITY,
	[LineDefinitionId]	INT NOT NULL,
	[EntryIndex]		INT NOT NULL,
	[CenterId]			INT NOT NULL,
	[CurrencyId]		NCHAR (3) NOT NULL,
	[AgentId]			INT,
	[ResourceId]		INT,
	[NotedAgentId]		INT,
	[NotedResourceId]	INT,
);
GO
CREATE UNIQUE INDEX IX_LineDefinitionsAgentsResourcesCurrencies ON
	dbo.LineDefinitionLineKeys ([LineDefinitionId], [EntryIndex], [CenterId], [CurrencyId], [AgentId], [ResourceId], [NotedAgentId], [NotedResourceId]);
GO