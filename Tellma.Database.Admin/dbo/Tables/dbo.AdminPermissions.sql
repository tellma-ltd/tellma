CREATE TABLE [dbo].[AdminPermissions] (
	[Id]				INT					CONSTRAINT [PK_Permissions] PRIMARY KEY IDENTITY,
	[AdminUserId]		INT					NOT NULL CONSTRAINT [FK_AdminPermissions__AdminUsers] REFERENCES [dbo].[AdminUsers] ([Id]) ON DELETE CASCADE,
	[View]				NVARCHAR (255)		NOT NULL,
	[Action]			NVARCHAR (255)		NOT NULL CONSTRAINT [CK_AdminPermissions__Level] CHECK ([Action] IN (N'Read', N'Update', N'Delete', N'IsActive', N'SendInvitationEmail', N'All')),
	[Criteria]			NVARCHAR(1024),		-- compiles into LINQ expression to filter the applicability
	[Memo]				NVARCHAR (255),
	[CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById] INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')), 
    CONSTRAINT [FK_AdminUsers_AdminPermissions_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AdminUsers]([Id]),
    CONSTRAINT [FK_AdminUsers_AdminPermissions_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [AdminUsers]([Id])
)
GO
CREATE INDEX [IX_AdminPermissions__AdminUserId] ON [dbo].[AdminPermissions]([AdminUserId]);
GO