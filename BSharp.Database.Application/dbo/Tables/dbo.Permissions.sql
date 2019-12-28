CREATE TABLE [dbo].[Permissions] (
	[Id]			INT					CONSTRAINT [PK_Permissions] PRIMARY KEY IDENTITY,
	[RoleId]		INT					NOT NULL CONSTRAINT [FK_Permissions__Roles] REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
	[View]			NVARCHAR (255)		NOT NULL, -- CONSTRAINT [FK_Permissions__Views] REFERENCES [dbo].[Views] ([Id]) ON DELETE CASCADE,
	[Action]		NVARCHAR (255)		NOT NULL CONSTRAINT [CK_Permissions__Level] CHECK ([Action] IN (N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'All')),
	[Criteria]		NVARCHAR(1024),		-- compiles into LINQ expression to filter the applicability
	[Mask]			NVARCHAR(1024),
	[Memo]			NVARCHAR (255),
	--[SavedAt]			AS [ValidFrom] AT TIME ZONE 'UTC',
	[SavedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Permissions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[PermissionsHistory]));
GO
CREATE INDEX [IX_Permissions__RoleId] ON [dbo].[Roles]([Id]);
GO