CREATE TABLE [dbo].[NotificationCommands]
(
	[Id] INT CONSTRAINT [PK_NotificationCommands] PRIMARY KEY IDENTITY,
	[TemplateId] INT NOT NULL CONSTRAINT [FK_NotificationCommands__NotificationTemplateId] REFERENCES [dbo].[NotificationTemplates] ([Id]),
	[EntityId] INT, -- Manual only
	[Caption] NVARCHAR(1024),
	[ScheduledTime] DATETIMEOFFSET(7),
	[Arguments] NVARCHAR(1024), -- JSON
	[CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT CONSTRAINT [FK_NotificationCommands__CreatedById] REFERENCES [dbo].[Users] ([Id])
)
