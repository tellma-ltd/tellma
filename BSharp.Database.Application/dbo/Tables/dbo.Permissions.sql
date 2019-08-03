CREATE TABLE [dbo].[Permissions] (
	[Id]			INT PRIMARY KEY,
	[RoleId]		INT	NOT NULL,
	[ViewId]		NVARCHAR (255)		NOT NULL,
	[Level]			NVARCHAR (255)		NOT NULL,
	[Criteria]		NVARCHAR(1024), -- compiles into LINQ expression to filter the applicability
	[Memo]			NVARCHAR (255),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	CONSTRAINT [CK_Permissions__Level] CHECK ([Level] IN (N'Read', N'Create', N'ReadCreate', N'Update')),--, N'Sign')),
	CONSTRAINT [FK_Permissions__Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_Permissions__Views] FOREIGN KEY ([ViewId]) REFERENCES [dbo].[Views] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_Permissions__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_Permissions__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE INDEX [IX_Permissions__RoleId] ON [dbo].[Roles]([Id]);
GO