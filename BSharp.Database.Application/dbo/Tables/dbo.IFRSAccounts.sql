CREATE TABLE [dbo].[IfrsAccounts] (
	[Id]						NVARCHAR (255) CONSTRAINT [PK_IfrsAccounts] PRIMARY KEY NONCLUSTERED CONSTRAINT [FK_IfrsAccounts__Id] FOREIGN KEY ([Id]) REFERENCES [dbo].[IfrsConcepts] ([Id]) ON DELETE CASCADE,
	[Node]						HIERARCHYID CONSTRAINT [CK_IfrsNotes__Node] UNIQUE INDEX [IX_IfrsAccounts__Node] CLUSTERED,
	[ParentNode]				AS [Node].GetAncestor(1),

	-- Aggregate means, it does not take direct entries, but rather used for aggregation only
	-- IsAggergate = True If and only if isLeaf = False. We used IsAggregate instead since
	-- a leaf is used in computer science to mean a node with no children. So, as we build the tree
	-- leaves are converted into internal nodes. Hence it is a computed property, unlike IsAggregate
	[IsAggregate]				BIT						NOT NULL DEFAULT 1,

--	The settings below apply to the Account with this Ifrs, as well to the JE.LI endowed with this Ifrs
	[IfrsNoteSetting]			NVARCHAR (255)			NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required

	[AgentAccountSetting]		NVARCHAR (255)			NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
	[AgentRelationTypeList]		NVARCHAR (1024),	-- e.g., OtherCurrentReceivables applies to ALL except supplier & customer
--	[AgentAccountFilter]		NVARCHAR (1024),	
	[AgentAccountLabel]			NVARCHAR (255),
	[AgentAccountLabel2]		NVARCHAR (255),
	[AgentAccountLabel3]		NVARCHAR (255),

	[ResourceSetting]			NVARCHAR (255)			NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
	[ResourceTypeList]			NVARCHAR (1024),	
--	[ResourceFilter]			NVARCHAR (1024),
	[ResourceLabel]				NVARCHAR (255),
	[ResourceLabel2]			NVARCHAR (255),
	[ResourceLabel3]			NVARCHAR (255),

	[DebitExternalReferenceSetting]		NVARCHAR (255)			NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
	[DebitReferenceLabel]		NVARCHAR (255),
	[DebitReferenceLabel2]		NVARCHAR (255),
	[DebitReferenceLabel3]		NVARCHAR (255),

	[CreditExternalReferenceSetting]	NVARCHAR (255)			NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
	[CreditReferenceLabel]		NVARCHAR (255),
	[CreditReferenceLabel2]		NVARCHAR (255),
	[CreditReferenceLabel3]		NVARCHAR (255),

	[DebitAdditionalReferenceSetting]		NVARCHAR (255)	NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
	[DebitRelatedReferenceLabel]		NVARCHAR (255),
	[DebitRelatedReferenceLabel2]		NVARCHAR (255),
	[DebitRelatedReferenceLabel3]		NVARCHAR (255),

	[CreditAdditionalReferenceSetting]	NVARCHAR (255)		NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
	[CreditRelatedReferenceLabel]		NVARCHAR (255),
	[CreditRelatedReferenceLabel2]		NVARCHAR (255),
	[CreditRelatedReferenceLabel3]		NVARCHAR (255),

	[RelatedResourceSetting]	NVARCHAR (255)			NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
	[RelatedResourceTypeList]	NVARCHAR (1024),	
--	[RelatedResourceFilter]		NVARCHAR (1024),
	[RelatedResourceLabel]		NVARCHAR (255),
	[RelatedResourceLabel2]		NVARCHAR (255),
	[RelatedResourceLabel3]		NVARCHAR (255),
	
	[RelatedAgentAccountSetting]NVARCHAR (255)			NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
--	[RelatedAgentAccountFilter]	NVARCHAR (1024),
	[RelatedAgentRelationTypeList]NVARCHAR (1024),
	[RelatedAgentAccountLabel]	NVARCHAR (255),
	[RelatedAgentAccountLabel2]	NVARCHAR (255),
	[RelatedAgentAccountLabel3]	NVARCHAR (255)
);
GO
