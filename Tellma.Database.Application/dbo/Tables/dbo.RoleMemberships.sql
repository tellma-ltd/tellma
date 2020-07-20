CREATE TABLE [dbo].[RoleMemberships] (
	[Id]				INT				CONSTRAINT [PK_RoleMemberships] PRIMARY KEY IDENTITY,
	[UserId]			INT				NOT NULL CONSTRAINT [FK_RoleMemberships__UserId] REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,	
	[RoleId]			INT				NOT NULL CONSTRAINT [FK_RoleMemberships__RoleId] REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
	[Memo]				NVARCHAR (255),
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_RoleMemberships__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[RoleMembershipsHistory]));
GO
