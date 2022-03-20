CREATE TABLE [dbo].[EmailTemplateSubscribers]
(
	[Id] INT CONSTRAINT [PK_EmailTemplateSubscribers] PRIMARY KEY IDENTITY,
	[EmailTemplateId] INT NOT NULL CONSTRAINT [FK_EmailTemplateSubscribers__EmailTemplateId] REFERENCES [dbo].[EmailTemplates] ([Id]) ON DELETE CASCADE,
	[UserId] INT CONSTRAINT [FK_EmailTemplateSubscribers__UserId] REFERENCES [dbo].[Users] ([Id]),
);
GO
