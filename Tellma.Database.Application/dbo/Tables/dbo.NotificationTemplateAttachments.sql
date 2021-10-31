CREATE TABLE [dbo].[NotificationTemplateAttachments]
(
	[Id] INT CONSTRAINT [PK_NotificationTemplateAttachments] PRIMARY KEY NONCLUSTERED IDENTITY,	
	[Index] INT NOT NULL,
	[NotificationTemplateId] INT NOT NULL CONSTRAINT [FK_NotificationTemplateAttachments__NotificationTemplateId] REFERENCES [dbo].[NotificationTemplates] ([Id]) ON DELETE CASCADE,
	[ContextOverride] NVARCHAR (1024),
	[DownloadNameOverride] NVARCHAR (1024),
	[PrintingTemplateId] INT CONSTRAINT [FK_NotificationTemplateAttachments__PrintingTemplateId] REFERENCES [dbo].[PrintingTemplates] ([Id]),
)
