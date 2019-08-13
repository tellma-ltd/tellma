CREATE TABLE [dbo].[Permissions] (
	[Id]			INT					CONSTRAINT [PK_Permissions] PRIMARY KEY IDENTITY,
	[RoleId]		INT					NOT NULL CONSTRAINT [FK_Permissions__Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
	[ViewId]		NVARCHAR (255)		NOT NULL CONSTRAINT [FK_Permissions__Views] FOREIGN KEY ([ViewId]) REFERENCES [dbo].[Views] ([Id]) ON DELETE CASCADE,
	[Action]		NVARCHAR (255)		NOT NULL CONSTRAINT [CK_Permissions__Level] CHECK ([Action] IN (N'Read', N'Update', N'IsActive', N'ResendInvitationEmail', N'All')),
	[Criteria]		NVARCHAR(1024),		-- compiles into LINQ expression to filter the applicability
	[Mask]			NVARCHAR(1024),
	[Memo]			NVARCHAR (255),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Permissions__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]	INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Permissions__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])	
);
GO
CREATE INDEX [IX_Permissions__RoleId] ON [dbo].[Roles]([Id]);
GO