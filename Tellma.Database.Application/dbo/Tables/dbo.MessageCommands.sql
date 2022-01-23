CREATE TABLE [dbo].[MessageCommands]
(
	[Id] INT CONSTRAINT [PK_MessageCommands] PRIMARY KEY IDENTITY,
	[TemplateId] INT NOT NULL CONSTRAINT [FK_MessageCommands__MessageTemplateId] REFERENCES [dbo].[MessageTemplates] ([Id]),
	[EntityId] INT, -- Manual only
	[Caption] NVARCHAR(1024),
	[ScheduledTime] DATETIMEOFFSET(7),
	[Arguments] NVARCHAR(1024), -- JSON
	[CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT CONSTRAINT [FK_MessageCommands__CreatedById] REFERENCES [dbo].[Users] ([Id])
)
