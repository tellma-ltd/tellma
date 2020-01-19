CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]							INT				PRIMARY KEY,
	[Id]							INT				NOT NULL DEFAULT 0,
	[ResponsibilityCenterId]		INT,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50),
	--[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	[AccountTypeId]					INT				NOT NULL,
	[IsCurrent]						BIT,
--
	[LegacyClassificationId]		INT,
	[LegacyType]					NVARCHAR (50),
-- Major properties: NULL means it is not defined.
	[AgentDefinitionId]				NVARCHAR (50),
	[HasResource]					BIT				NOT NULL DEFAULT 0,
	[HasAgent]						BIT				NOT NULL DEFAULT 0,
	[IsRelated]						BIT				NOT NULL DEFAULT 0,
	[HasReferenceAgent]				BIT				NOT NULL DEFAULT 0,	
	[HasReferenceText]				BIT				NOT NULL DEFAULT 0,
	[HasReferenceAmount]			BIT				NOT NULL DEFAULT 0, 
	--[HasReferenceResource]		BIT				NOT NULL DEFAULT 0,
	[AgentId]						INT,
	[ResourceId]					INT,
	[CurrencyId]					NCHAR (3)			NOT NULL,
	[Identifier]					NVARCHAR (10),
-- Entry Property
	[EntryTypeId]					INT
);