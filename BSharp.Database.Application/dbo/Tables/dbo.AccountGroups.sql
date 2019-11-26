CREATE TABLE [dbo].[AccountGroups] (
	[Id]							NVARCHAR (50) PRIMARY KEY, --CONSTRAINT [PK_AccountGroups] PRIMARY KEY,
	[AccountTypeId]					NVARCHAR (50) CONSTRAINT [FK_AccountGroups__AccountTypeId] REFERENCES dbo.AccountTypes([Id]),
	[AgentRelationDefinitionId]		NVARCHAR (50),
	[ResourceTypeId]				NVARCHAR (50),
	[EntryTypeId]					NVARCHAR (255) CONSTRAINT [FK_AccountGroups__EntryTypeId] REFERENCES dbo.EntryTypes([Id]),

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

	[HasRelatedAgent]				BIT,
	-- If HasRelatedAgent = 0 and DebitRelatedAgentLabel is not null, then the user is supposed to fill the related agent name, instead of related agent Id
	[DebitRelatedAgentLabel]		NVARCHAR (50), -- NULL means it is invisible
	[DebitRelatedAgentLabel2]		NVARCHAR (50),
	[DebitRelatedAgentLabel3]		NVARCHAR (50),

	[CreditRelatedAgentLabel]		NVARCHAR (50), -- NULL means it is invisible
	[CreditRelatedAgentLabel2]		NVARCHAR (50),
	[CreditRelatedAgentLabel3]		NVARCHAR (50),

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
CREATE UNIQUE INDEX [IX_AccountGroups__AccountTypeId_AgentRelationDefinitionId_ResourceTypeId_EntryTypeId] ON
	[dbo].[AccountGroups]([AccountTypeId], [AgentRelationDefinitionId], [ResourceTypeId], [EntryTypeId]);
GO