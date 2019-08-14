CREATE TABLE [dbo].[Users] (
	[Id]					INT	PRIMARY KEY,
	[ExternalId]			NVARCHAR (450),
	[Email]					NVARCHAR (255)		NOT NULL UNIQUE, -- Required

	[LastAccess]			DATETIMEOFFSET(7),
	[PermissionsVersion]	UNIQUEIDENTIFIER	NOT NULL DEFAULT ('aafc6590-cadf-45fe-8c4a-045f4d6f73b1'),
	[UserSettingsVersion]	UNIQUEIDENTIFIER	NOT NULL DEFAULT ('aafc6590-cadf-45fe-8c4a-045f4d6f73b2'),
	[ImageId]				NVARCHAR (50),
	[PreferredLanguage]		NCHAR (2)			NOT NULL DEFAULT (N'en'), 
	[IsActive]				BIT					NOT NULL DEFAULT (1),
	
	[SortKey]				DECIMAL (9,4),
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	CONSTRAINT [FK_Users__AgentId] FOREIGN KEY ([Id]) REFERENCES [dbo].[Agents] ([Id]),
	CONSTRAINT [FK_Users__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_Users__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
--CREATE CLUSTERED INDEX [IX_LocalUsers__SortKey]
--  ON [dbo].[Users]([SortKey]);
--GO