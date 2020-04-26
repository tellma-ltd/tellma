CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]					INT				PRIMARY KEY,
	[Id]					INT				NOT NULL DEFAULT 0,
	[DefinitionId]			INT,
	[CenterId]				INT,
	[Name]					NVARCHAR (255)	NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Code]					NVARCHAR (50),
	[IfrsTypeId]			INT				NOT NULL,
	[ClassificationId]		INT,
-- Major properties: NULL means it is not defined.
	[RelationId]			INT,
	[ContractId]			INT,
	[ResourceId]			INT,
	[CurrencyId]			NCHAR (3),
	[EntryTypeId]			INT
);