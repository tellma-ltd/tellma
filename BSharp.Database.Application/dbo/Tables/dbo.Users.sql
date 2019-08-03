CREATE TABLE [dbo].[Users] (
	[Id]				INT	PRIMARY KEY IDENTITY (1, 1),
	[Name]				NVARCHAR (255)		NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[ExternalId]		NVARCHAR (450),
	[Email]				NVARCHAR (255)		NOT NULL UNIQUE, -- Required

	[LastAccess]		DATETIMEOFFSET(7),
	[PermissionsVersion] UNIQUEIDENTIFIER	NOT NULL DEFAULT ('aafc6590-cadf-45fe-8c4a-045f4d6f73b1'),
	[UserSettingsVersion] UNIQUEIDENTIFIER	NOT NULL DEFAULT ('aafc6590-cadf-45fe-8c4a-045f4d6f73b2'),
	[ImageId]			NVARCHAR (50),
	[PreferredLanguage] NCHAR (2)			NOT NULL DEFAULT (N'en'), 
	[AgentId]			INT,
	[IsActive]			BIT					NOT NULL DEFAULT (1),
	
	[SortKey]			DECIMAL (9,4),
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT	NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT	NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	CONSTRAINT [FK_Users__AgentId] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agents] ([Id]),
	CONSTRAINT [FK_Users__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_Users__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
--CREATE CLUSTERED INDEX [IX_LocalUsers__SortKey]
--  ON [dbo].[Users]([SortKey]);
--GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users__Name]
  ON [dbo].[Users]([Name]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users__Name2]
  ON [dbo].[Users]([Name2]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users__Name3]
  ON [dbo].[Users]([Name3]) WHERE [Name3] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users__AgentId]
  ON [dbo].[Users]([AgentId]) WHERE [AgentId] IS NOT NULL;
GO