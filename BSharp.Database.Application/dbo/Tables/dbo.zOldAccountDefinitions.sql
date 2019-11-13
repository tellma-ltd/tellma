CREATE TABLE [dbo].[OldAccountDefinitions] (
	[Id]							NVARCHAR (50) CONSTRAINT [PK_AccountDefinitions] PRIMARY KEY,
	[Description]					NVARCHAR (255),
	[Description2]					NVARCHAR (255),
	[Description3]					NVARCHAR (255),
	[TitleSingular]					NVARCHAR (255) NOT NULL,
	[TitleSingular2]				NVARCHAR (255),
	[TitleSingular3]				NVARCHAR (255),
	[TitlePlural]					NVARCHAR (255) NOT NULL,
	[TitlePlural2]					NVARCHAR (255),
	[TitlePlural3]					NVARCHAR (255),

	[AgentRelationDefinitionList]	NVARCHAR (1024),
	[HasResource]					BIT				NOT NULL DEFAULT 0,
	[ResourceTypeList]				NVARCHAR (1024),
	[HasRelatedAgent]				BIT				NOT NULL DEFAULT 0,
		
	[EntryTypeId]					NVARCHAR (255) CONSTRAINT [FK_AccountDefinitions__EntryTypeId] REFERENCES dbo.EntryTypes([Id]),

	[DebitPartyNameLabel]			NVARCHAR (50), -- NULL means it is invisible
	[DebitPartyNameLabel2]			NVARCHAR (50),
	[DebitPartyNameLabel3]			NVARCHAR (50),

	[CreditPartyNameLabel]			NVARCHAR (50), -- NULL means it is invisible
	[CreditPartyNameLabel2]			NVARCHAR (50),
	[CreditPartyNameLabel3]			NVARCHAR (50),

	[DueDateLabel]					NVARCHAR (50), -- NULL means it is invisible
	[DueDateLabel2]					NVARCHAR (50),
	[DueDateLabel3]					NVARCHAR (50),

	[RelatedAmountLabel]			NVARCHAR (50), -- NULL means it is invisible
	[RelatedAmountLabel2]			NVARCHAR (50),
	[RelatedAmountLabel3]			NVARCHAR (50)
);