CREATE TABLE [dbo].[RoleMemberships] (
	[Id]				INT					CONSTRAINT [PK_RoleMemberships] PRIMARY KEY IDENTITY,
	[AgentId]			INT	NOT NULL		CONSTRAINT [FK_RoleMemberships__AgentId] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agents] ([Id]) ON DELETE CASCADE,	
	[RoleId]			INT	NOT NULL		CONSTRAINT [FK_RoleMemberships__RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
	[Memo]				NVARCHAR (255),
	-- Computed columns require a workaround for Temporal tables:
	--[SavedAt]			AS [ValidFrom] AT TIME ZONE 'UTC',
	[SavedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_RoleMemberships__SavedById] FOREIGN KEY ([SavedById]) REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[RoleMembershipsHistory]));
GO
-- I don't think this is needed
-- CREATE UNIQUE INDEX [IX_RoleMemberships__UserId_RoleId] ON [dbo].[RoleMemberships]([AgentId], RoleId)