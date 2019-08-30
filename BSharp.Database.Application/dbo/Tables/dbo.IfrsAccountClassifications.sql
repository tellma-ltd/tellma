CREATE TABLE [dbo].[IfrsAccountClassifications] ( -- managed by Banan IT
	[Id]						NVARCHAR (255) CONSTRAINT [PK_IfrsClassifications] PRIMARY KEY NONCLUSTERED CONSTRAINT [FK_IfrsClassifications__Id] FOREIGN KEY ([Id]) REFERENCES [dbo].[IfrsConcepts] ([Id]) ON DELETE CASCADE,
	[Node]						HIERARCHYID CONSTRAINT [CK_IfrsClassifications__Node] UNIQUE INDEX [IX_IfrsClassifications__Node] CLUSTERED,
	[ParentNode]				AS [Node].GetAncestor(1),
	[IsLeaf]					BIT					NOT NULL DEFAULT 1, -- update to 0 those who do appear as ancestors
	-- classifications of childen of same parent can all be aggregated to the parent,
	-- or can some be combined into catchall "other", like Other Inventories, Other property plant and equipment, etc.
	[StatementClassificationId]			NVARCHAR (255)		NULL, -- financial position, comprehensive income
	-- sum the debit movement, and map it to the debit cash flow classification. Repeat for credit movement.
	[DebitCashFlowClassificationId]		NVARCHAR (255)		NULL,
	[CreditCashFlowClassificationId]	NVARCHAR (255)		NULL,
	-- Inactive means, the subtree - whose root is this - does not appear to the user when classifying an account
	[IsActive]					BIT					NOT NULL DEFAULT 1, -- when off, the customer does not use this classification
	[Label]						NVARCHAR (1024)		NOT NULL,
	[Label2]					NVARCHAR (1024),
	[Label3]					NVARCHAR (1024),
	[Documentation]				NVARCHAR (MAX),
	[Documentation2]			NVARCHAR (MAX),
	[Documentation3]			NVARCHAR (MAX),
	[EffectiveDate]				DATETIME2(7)		NOT NULL DEFAULT('0001-01-01 00:00:00'),
	[ExpiryDate]				DATETIME2(7)		NOT NULL DEFAULT('9999-12-31 23:59:59'),

--	The settings below apply to any account whose type is this Ifrs
	[IfrsNoteSetting]			NVARCHAR (255)			NOT NULL DEFAULT 'N/A', -- N/A, Optional, Required
/*
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
	*/
);
GO
