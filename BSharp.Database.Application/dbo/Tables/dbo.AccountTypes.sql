CREATE TABLE [dbo].[AccountTypes]
(
	[Id]							NVARCHAR (50) CONSTRAINT [PK_AccountTypes] PRIMARY KEY,
	[AgentRelationDefinitionList]	NVARCHAR (1024),
	[ResourceClassificationList]	NVARCHAR (1024),
	[HasLiquidity]					BIT
)
	