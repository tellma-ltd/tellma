CREATE TABLE [dbo].[AccountDefinitions] (
	[Id]							NVARCHAR (50) PRIMARY KEY, --CONSTRAINT [PK_AccountDefinitions] PRIMARY KEY,
	[AccountTypeId]					NVARCHAR (50) REFERENCEs dbo.AccountTypes([Id]),
	[AgentRelationDefinitionId]		NVARCHAR (50),
	[ResourceTypeId]				NVARCHAR (50),
	[EntryTypeId]					NVARCHAR (255) CONSTRAINT [FK_AccountTypes__EntryTypeId] REFERENCES dbo.EntryTypes([Id]),

	[Description]					NVARCHAR (255),
	[Description2]					NVARCHAR (255),
	[Description3]					NVARCHAR (255),

	[TitleSingular]					NVARCHAR (255) NOT NULL,
	[TitleSingular2]				NVARCHAR (255),
	[TitleSingular3]				NVARCHAR (255),
	[TitlePlural]					NVARCHAR (255) NOT NULL,
	[TitlePlural2]					NVARCHAR (255),
	[TitlePlural3]					NVARCHAR (255),

	[MonetaryAmountLabel]			NVARCHAR (50),
	[MonetaryAmountLabel2]			NVARCHAR (50),
	[MonetaryAmountLabel3]			NVARCHAR (50),

	[DebitPartyNameLabel]			NVARCHAR (50), -- NULL means it is invisible
	[DebitPartyNameLabel2]			NVARCHAR (50),
	[DebitPartyNameLabel3]			NVARCHAR (50),

	[CreditPartyNameLabel]			NVARCHAR (50), -- NULL means it is invisible
	[CreditPartyNameLabel2]			NVARCHAR (50),
	[CreditPartyNameLabel3]			NVARCHAR (50),

	[DueDateLabel]					NVARCHAR (50), -- NULL means it is invisible
	[DueDateLabel2]					NVARCHAR (50),
	[DueDateLabel3]					NVARCHAR (50),

	[ExternalReferenceLabel]		NVARCHAR (50), -- NULL means it is invisible
	[ExternalReferenceLabel2]		NVARCHAR (50), -- NULL means it is invisible
	[ExternalReferenceLabel3]		NVARCHAR (50), -- NULL means it is invisible

	[AdditionalReferenceLabel]		NVARCHAR (50), -- NULL means it is invisible
	[AdditionalReferenceLabel2]		NVARCHAR (50), -- NULL means it is invisible
	[AdditionalReferenceLabel3]		NVARCHAR (50), -- NULL means it is invisible

	[RelatedAmountLabel]			NVARCHAR (50), -- NULL means it is invisible
	[RelatedAmountLabel2]			NVARCHAR (50),
	[RelatedAmountLabel3]			NVARCHAR (50)
);
GO
CREATE UNIQUE INDEX [IX_AccountDefinitions__AccountTypeId_AgentRelationDefinitionId_ResourceTypeId_EntryTypeId] ON
	[dbo].[AccountDefinitions]([AccountTypeId], [AgentRelationDefinitionId], [ResourceTypeId], [EntryTypeId]);
GO