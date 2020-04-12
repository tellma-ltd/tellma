CREATE TABLE [dbo].[AccountTypes] (
	[Id]						INT					CONSTRAINT [PK_AccountTypes]  PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]					INT					CONSTRAINT [FK_AccountTypes__ParentId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[Code]						NVARCHAR (255)		NOT NULL CONSTRAINT [IX_AccountTypes__Code] UNIQUE NONCLUSTERED,
	[IfrsConceptId]				INT					CONSTRAINT [FK_AccountTypes__IfrsConceptId] REFERENCES dbo.[IfrsConcepts]([Id]),				
	[Name]						NVARCHAR (255)		NOT NULL,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[Node]						HIERARCHYID			NOT NULL CONSTRAINT [UX_AccountTypes__Node] UNIQUE CLUSTERED,
	[IsAssignable]				BIT					NOT NULL DEFAULT 1,
	[CurrencyAssignment]		NCHAR (1)			NOT NULL DEFAULT N'A' CONSTRAINT [CK_AccountTypes__CurrencyAssignment] CHECK([CurrencyAssignment] IN (N'A',N'E')),
	[AgentAssignment]			NCHAR (1)			NOT NULL DEFAULT N'N' CONSTRAINT [CK_AccountTypes__AgentAssignment] CHECK([AgentAssignment] IN (N'N',N'A',N'E')),
	[AgentDefinitionId]			NVARCHAR (50)		CONSTRAINT [FK_AccountTypes__AgentDefinitionId] REFERENCES dbo.AgentDefinitions([Id]),

	[ResourceAssignment]		NCHAR (1)			NOT NULL DEFAULT N'N' CONSTRAINT [CK_AccountTypes__ResourceAssignment] CHECK([ResourceAssignment] IN (N'N',N'A',N'E')),
	[ResourceDefinitionId]		NVARCHAR (50)		CONSTRAINT [FK_AccountTypes__ResourceDefinitionId] REFERENCES dbo.ResourceDefinitions([Id]),

	[IsResourceClassification]	AS CAST(
										IIF([ResourceAssignment] IN (N'A',N'E')
										AND [Code] NOT IN (N'CurrentInventoriesInTransit', N'WorkInProgress'), 1, 0)
									AS BIT) PERSISTED,
	[CenterAssignment]			NCHAR (1)			NOT NULL DEFAULT N'A' CONSTRAINT [CK_AccountTypes__CenterAssignment] CHECK([CenterAssignment] IN (N'A',N'E')),
	[EntryTypeAssignment]		NCHAR (1)			NOT NULL DEFAULT N'N' CONSTRAINT [CK_AccountTypes__EntryTypeAssignment] CHECK([EntryTypeAssignment] IN (N'N',N'A',N'E')),
	[EntryTypeParentId]			INT					CONSTRAINT [FK_AccountTypes__EntryTypeParentId] REFERENCES [dbo].[EntryTypes] ([Id]),	
	CONSTRAINT [CK_AccountTypes__EntryTypeAssignment_EntryTypeParentId] CHECK(
		[EntryTypeAssignment] = N'N' AND [EntryTypeParentId] IS NULL OR
		[EntryTypeAssignment] <> N'N' AND [EntryTypeParentId] IS NOT NULL
		),
	[IdentifierAssignment]		NCHAR (1)			NOT NULL DEFAULT N'N' CONSTRAINT [CK_AccountTypes__IdentifierAssignment] CHECK([IdentifierAssignment] IN (N'N',N'A',N'E')),
	[IdentifierLabel]			NVARCHAR (50),
	[IdentifierLabel2]			NVARCHAR (50),
	[IdentifierLabel3]			NVARCHAR (50),
	[NotedAgentAssignment]		NCHAR (1)			NOT NULL DEFAULT N'N' CONSTRAINT [CK_AccountTypes__NotedAgentAssignment] CHECK([NotedAgentAssignment] IN (N'N',N'E')),
	[NotedAgentDefinitionId]	NVARCHAR (50)		CONSTRAINT [FK_AccountTypes__NotedAgentDefinitionId] REFERENCES dbo.AgentDefinitions([Id]),

	[DueDateLabel]				NVARCHAR (50),
	[DueDateLabel2]				NVARCHAR (50),
	[DueDateLabel3]				NVARCHAR (50),
	[Time1Label]				NVARCHAR (50),
	[Time1Label2]				NVARCHAR (50),
	[Time1Label3]				NVARCHAR (50),
	[Time2Label]				NVARCHAR (50),
	[Time2Label2]				NVARCHAR (50),
	[Time2Label3]				NVARCHAR (50),
	[ExternalReferenceLabel]	NVARCHAR (50),
	[ExternalReferenceLabel2]	NVARCHAR (50),
	[ExternalReferenceLabel3]	NVARCHAR (50),
	[AdditionalReferenceLabel]	NVARCHAR (50),
	[AdditionalReferenceLabel2]	NVARCHAR (50),
	[AdditionalReferenceLabel3]	NVARCHAR (50),
	[NotedAgentNameLabel]		NVARCHAR (50),
	[NotedAgentNameLabel2]		NVARCHAR (50),
	[NotedAgentNameLabel3]		NVARCHAR (50),
	[NotedAmountLabel]			NVARCHAR (50),
	[NotedAmountLabel2]			NVARCHAR (50),
	[NotedAmountLabel3]			NVARCHAR (50),
	[NotedDateLabel]			NVARCHAR (50),
	[NotedDateLabel2]			NVARCHAR (50),
	[NotedDateLabel3]			NVARCHAR (50),
	-- Additional properties, Is Active at the end
	[IsActive]					BIT					NOT NULL DEFAULT 1,
	[IsSystem]					BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	-- Pure SQL properties and computed properties
	[ParentNode]				AS [Node].GetAncestor(1)
);
GO