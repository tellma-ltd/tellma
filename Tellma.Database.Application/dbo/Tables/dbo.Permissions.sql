﻿CREATE TABLE [dbo].[Permissions] (
	[Id]			INT					CONSTRAINT [PK_Permissions] PRIMARY KEY IDENTITY,
	[RoleId]		INT					NOT NULL CONSTRAINT [FK_Permissions__Roles] REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
	[View]			NVARCHAR (255)		NOT NULL,
	[Action]		NVARCHAR (255)		NOT NULL,
	[Criteria]		NVARCHAR(1024),		-- compiles into LINQ expression to filter the applicability
	[Mask]			NVARCHAR(1024),
	[Memo]			NVARCHAR (255),
	--[SavedAt]			AS [ValidFrom] AT TIME ZONE 'UTC',
	[SavedById]			INT					NOT NULL CONSTRAINT [FK_Permissions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[PermissionsHistory]));
GO
CREATE INDEX [IX_Permissions__RoleId] ON [dbo].[Permissions]([RoleId]);
GO