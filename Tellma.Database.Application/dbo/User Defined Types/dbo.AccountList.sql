CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]						INT				PRIMARY KEY,
	[Id]						INT				NOT NULL DEFAULT 0,
	[AccountTypeId]				INT,
	[CenterId]					INT,
	[Name]						NVARCHAR (255),
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Code]						NVARCHAR (50),
	[ClassificationId]			INT,
	[RelationDefinitionId]		INT,
	[RelationId]				INT,
	[ResourceDefinitionId]		INT,
	[ResourceId]				INT,
	[NotedRelationDefinitionId]	INT,
	[NotedRelationId]			INT,
	[CurrencyId]				NCHAR (3),
	[EntryTypeId]				INT
);