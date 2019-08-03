CREATE TABLE [dbo].[RoleMemberships] (
	[Id]				INT	PRIMARY KEY,
	[Userid]			INT	NOT NULL,
	[RoleId]			INT	NOT NULL,
	[Memo]				NVARCHAR (255),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	CONSTRAINT [FK_RoleMemberships__UserId] FOREIGN KEY ([Userid]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_RoleMemberships__RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]),
	CONSTRAINT [FK_RoleMemberships__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_RoleMemberships__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO