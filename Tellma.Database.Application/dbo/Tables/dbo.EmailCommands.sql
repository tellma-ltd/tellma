CREATE TABLE [dbo].[EmailCommands]
(
	[Id] INT CONSTRAINT [PK_EmailCommands] PRIMARY KEY IDENTITY,
	[TemplateId] INT NOT NULL CONSTRAINT [FK_EmailCommands__TemplateId] REFERENCES [dbo].[EmailTemplates] ([Id]),
	[EntityId] INT, -- Manual only
	[Caption] NVARCHAR(1024),
	[ScheduledTime] DATETIMEOFFSET(7),
	[Arguments] NVARCHAR(1024), -- JSON
	[CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT CONSTRAINT [FK_EmailCommands__CreatedById] REFERENCES [dbo].[Users] ([Id])
)
