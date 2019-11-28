CREATE TABLE [dbo].[AccountTypes]
(
	[Id]							NVARCHAR (50) CONSTRAINT [PK_AccountTypess] PRIMARY KEY,
	[AgentRelationDefinitionList]	NVARCHAR (1024),
	[ResourceTypeList]				NVARCHAR (1024),
	[HasLiquidity]					BIT
)
	