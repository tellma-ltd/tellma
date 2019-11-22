CREATE TABLE [dbo].[AccountTypes]
(
	[Id]							NVARCHAR (50) PRIMARY KEY,
	[AgentRelationDefinitionList]	NVARCHAR (1024),
	[HasRelatedAgent]				BIT				NOT NULL DEFAULT 0,
	[HasResource]					BIT				NOT NULL DEFAULT 0,
	[ResourceTypeList]				NVARCHAR (1024),
	[EntryTypeId]					NVARCHAR (255)
)
	