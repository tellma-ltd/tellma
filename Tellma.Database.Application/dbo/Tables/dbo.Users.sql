CREATE TABLE [dbo].[Users] (
	[Id]					INT					CONSTRAINT [PK_Users] PRIMARY KEY IDENTITY,--CONSTRAINT [FK_Users__Id] REFERENCES [dbo].[Agents] ([Id]),
	[ExternalId]			NVARCHAR (450),
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Email]					NVARCHAR (255)		NOT NULL CONSTRAINT [IX_Users__Email] UNIQUE, -- Required
	[ImageId]				NVARCHAR (50),
	[PreferredLanguage]		NCHAR (2),
	[LastAccess]			DATETIMEOFFSET(7),
	[PermissionsVersion]	UNIQUEIDENTIFIER	NOT NULL DEFAULT NEWID(),
	[UserSettingsVersion]	UNIQUEIDENTIFIER	NOT NULL DEFAULT NEWID(),

	-- Delete
	-- End Delete

	[SortKey]				DECIMAL (9,4),

	-- ??
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Users__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Users__ModifiedById] REFERENCES [dbo].[Users] ([Id])
	-- ??	
);
GO
--CREATE CLUSTERED INDEX [IX_LocalUsers__SortKey]
--  ON [dbo].[Users]([SortKey]);
--GO