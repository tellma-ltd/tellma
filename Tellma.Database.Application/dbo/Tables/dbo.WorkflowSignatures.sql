CREATE TABLE [dbo].[WorkflowSignatures] (
	[Id]					INT					CONSTRAINT [PK_WorkflowSignatures] PRIMARY KEY IDENTITY,
	[WorkflowId]			INT					NOT NULL CONSTRAINT [FK_WorkflowSignatories__WorkflowId] REFERENCES [dbo].[Workflows] ([Id]) ON DELETE CASCADE,
	[RuleType]				NVARCHAR (50)		NOT NULL DEFAULT N'ByRole' REFERENCES dbo.[RuleTypes] ([RuleType]),
	[RuleTypeEntryIndex]	INT					CONSTRAINT [FK_WorkflowSignatures__RuleTypeIndex] CHECK([RuleTypeEntryIndex] >= 0),
	-- All roles are needed to get to next positive state, one is enough to get to negative state
	[RoleId]				INT					CONSTRAINT [FK_WorkflowSignatures__RoleId] REFERENCES [dbo].[Roles] ([Id]),
	CONSTRAINT [CK_WorkflowSignatures__RuleType_RoleId] CHECK ([RuleType] <> N'ByRole' OR [RoleId] IS NOT NULL),
	[UserId]				INT					CONSTRAINT [FK_WorkflowSignatures__UserId] REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [CK_WorkflowSignatures__RuleType_UserId] CHECK ([RuleType] <> N'ByUser' OR [UserId] IS NOT NULL),
	CONSTRAINT [CK_WorkflowSignatures__RuleType_RuleTypeIndex] CHECK ([RuleType] <> N'ByAgent' OR [RuleTypeEntryIndex] IS NOT NULL),
	[PredicateType]			NVARCHAR(50)		CONSTRAINT [FK_WorkflowSignatures__PredicateType] REFERENCES dbo.PredicateTypes([PredicateType]),
	[PredicateTypeEntryIndex]	INT				CONSTRAINT [FK_WorkflowSignatures__PredicateTypeIndex_Value] CHECK([PredicateTypeEntryIndex] >= 0),
	[Value]					DECIMAL (19,4),
	CONSTRAINT [CK_WorkflowSignatures__PredicateType_PredicateTypeIndex] CHECK ([PredicateType] IS NULL OR [PredicateType] <> N'ValueGreaterOrEqual' OR [PredicateTypeEntryIndex] IS NOT NULL AND [Value] IS NOT NULL),
	[ProxyRoleId]			INT,			-- If a transition has a proxy role, an agent with that proxy role can sign on behalf.
	--[SavedAt]			AS [ValidFrom] AT TIME ZONE 'UTC',
	[SavedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_WorkflowSignatures__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[WorkflowSignaturesHistory]));
GO