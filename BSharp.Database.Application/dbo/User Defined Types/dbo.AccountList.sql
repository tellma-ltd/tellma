﻿CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]							INT				PRIMARY KEY,
	[Id]							INT				NOT NULL DEFAULT 0,
	[AccountGroupId]				NVARCHAR (50)	NOT NULL,
	[AccountClassificationId]		INT,
	[Name]							NVARCHAR (255)	NOT NULL INDEX IX_Name UNIQUE,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255),
	[PartyReference]				NVARCHAR (255),
	[HasSingleCurrency]				BIT				NOT NULL DEFAULT 1,
	[CurrencyId]					NCHAR (3)		DEFAULT CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId')),
	[HasSpecificLiquidity]			BIT				NOT NULL DEFAULT 1,
	[IsCurrent]						BIT				NOT NULL DEFAULT 1,
	[HasSingleResponsibilityCenterId] BIT			DEFAULT 1,
	[ResponsibilityCenterId]		INT,
	[AgentRelationDefinitionId]		NVARCHAR(50),
	[HasSingleAgent]				BIT,
	[AgentId]						INT,
	[HasSingleResource]				BIT,
	[ResourceId]					INT,
	[HasSingleEntryTypeId]			BIT,
	[EntryTypeId]					NVARCHAR (255)
);