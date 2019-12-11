CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]							INT				PRIMARY KEY,
	[Id]							INT				NOT NULL DEFAULT 0,
	[Name]							NVARCHAR (255)	NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255),
	[IsSmart]						BIT				NOT NULL DEFAULT 0,	
	[AccountTypeId]					NVARCHAR (50)	NOT NULL,
	[AccountClassificationId]		INT,
	
	-- Not used right now
	[CurrencyId]					NCHAR (3),
	[ResponsibilityCenterId]		INT,
	[ContractType]					NVARCHAR (50),--	REFERENCES dbo.[ContractTypes]([Id]),
	[AgentDefinitionId]				NVARCHAR (50),
	[ResourceClassificationId]		INT,
	[IsCurrent]						BIT,
-- Minor properties: range of values is restricted by defining a major property. For example, if AccountTypeId = N'Payable', then responsibility center
-- must be an operating segment. 
-- NULL means two things:
--	a) If the type itself is null, then it is not defined
--	b) if the type itself is not null, then it is to be defined in entries.
	[AgentId]						INT,
	[ResourceId]					INT,
	[Identifier]					NVARCHAR (10),
--
	[EntryClassificationId]			INT,
	CHECK (
		([IsSmart] = 0 AND [ResourceId] = CONVERT(INT, SESSION_CONTEXT(N'FunctionalResourceId'))) OR 
		([ResourceClassificationId] IS NOT NULL AND [ContractType] IS NOT NULL)
	)
);