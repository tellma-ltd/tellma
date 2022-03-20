CREATE TABLE [dbo].[EmailTemplateAttachments]
(
	[Id] INT CONSTRAINT [PK_EmailTemplateAttachments] PRIMARY KEY NONCLUSTERED IDENTITY,	
	[Index] INT NOT NULL,
	[EmailTemplateId] INT NOT NULL CONSTRAINT [FK_EmailTemplateAttachments__EmailTemplateId] REFERENCES [dbo].[EmailTemplates] ([Id]) ON DELETE CASCADE,
	[ContextOverride] NVARCHAR (1024),
	[DownloadNameOverride] NVARCHAR (1024),
	[PrintingTemplateId] INT CONSTRAINT [FK_EmailTemplateAttachments__PrintingTemplateId] REFERENCES [dbo].[PrintingTemplates] ([Id]),
)
