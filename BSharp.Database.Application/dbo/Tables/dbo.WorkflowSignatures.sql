CREATE TABLE [dbo].[WorkflowSignatures] (
	[Id]			INT					PRIMARY KEY IDENTITY,
	[WorkflowId]	INT					NOT NULL CONSTRAINT [FK_WorkflowSignatories__WorkflowId] FOREIGN KEY ([WorkflowId]) REFERENCES [dbo].[Workflows] ([Id]) ON DELETE CASCADE,
	-- All roles are needed to get to next positive state, one is enough to get to negative state
	[RoleId]		INT					NOT NULL CONSTRAINT [FK_WorkflowSignatories__RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]),
	[Criteria]		NVARCHAR(1024)		NULL, -- when evaluated to true, the role signature becomes required
	--[SavedAt]			AS [ValidFrom] AT TIME ZONE 'UTC',
	[SavedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_WorkflowSignatures__SavedById] FOREIGN KEY ([SavedById]) REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]		DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]		DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[WorkflowSignaturesHistory]));
GO