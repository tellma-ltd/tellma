CREATE TABLE [dbo].[WorkflowSignatures] (
	[Id]			INT					CONSTRAINT [PK_WorkflowSignatures] PRIMARY KEY IDENTITY,
	[WorkflowId]	INT					NOT NULL CONSTRAINT [FK_WorkflowSignatories__WorkflowId] REFERENCES [dbo].[Workflows] ([Id]) ON DELETE CASCADE,
	[RuleType]		NVARCHAR (50)		NOT NULL DEFAULT N'ByRole' CHECK ([RuleType] IN (N'ByRole', N'Public')),
	-- All roles are needed to get to next positive state, one is enough to get to negative state
	[RoleId]		INT					CONSTRAINT [FK_WorkflowSignatures__RoleId] REFERENCES [dbo].[Roles] ([Id]),
	CONSTRAINT [CK_WorkflowSignatures__RuleType_RoleId] CHECK([RuleType] <> N'ByRole' OR [RoleId] IS NOT NULL),
	[Criteria]		NVARCHAR(1024), -- when evaluated to true, the role signature becomes required
	[ProxyRoleId]	INT,			-- If a transition has a proxy role, an agent with that proxy role can sign on behalf.
	--[SavedAt]			AS [ValidFrom] AT TIME ZONE 'UTC',
	[SavedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_WorkflowSignatures__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]		DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]		DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[WorkflowSignaturesHistory]));
GO