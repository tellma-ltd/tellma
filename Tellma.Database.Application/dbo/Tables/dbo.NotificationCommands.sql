CREATE TABLE [dbo].[NotificationCommands]
(
	[Id] INT CONSTRAINT [PK_NotificationCommands] PRIMARY KEY IDENTITY,
	[NotificationTemplateId] INT NOT NULL CONSTRAINT [FK_NotificationCommands__NotificationTemplateId] REFERENCES [dbo].[NotificationTemplates] ([Id]),
	[Caption] NVARCHAR(1024),
	[Parameters] NVARCHAR(1024), -- JSON
	[Collection] NVARCHAR (50), -- Manual only
	[DefinitionId] INT, -- Manual only
	[EntityId] INT, -- Manual only
	[CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT CONSTRAINT [FK_NotificationCommands__CreatedById] REFERENCES [dbo].[Users] ([Id])
)
