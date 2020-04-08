CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]							INT				PRIMARY KEY,
	[Id]							INT				NOT NULL DEFAULT 0,
	[CenterId]						INT,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50),
	--[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	[AccountTypeId]					INT				NOT NULL,
	[CustomClassificationId]		INT,
-- Major properties: NULL means it is not defined.
	[IsSmart]						BIT				NOT NULL DEFAULT 0,
	[IsRelated]						BIT				NOT NULL DEFAULT 0,
	[AgentId]						INT,
	[ResourceId]					INT,
	[CurrencyId]					NCHAR (3),
	[Identifier]					NVARCHAR (10),
	[EntryTypeId]					INT
);