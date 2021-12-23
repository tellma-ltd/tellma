CREATE TABLE [dbo].[MessageTemplateSubscribers]
(
	[Id] INT CONSTRAINT [PK_MessageTemplateSubscribers] PRIMARY KEY IDENTITY,
	[MessageTemplateId] INT NOT NULL CONSTRAINT [FK_MessageTemplateSubscribers__MessageTemplateId] REFERENCES [dbo].[MessageTemplates] ([Id]) ON DELETE CASCADE,
	[UserId] INT CONSTRAINT [FK_MessageTemplateSubscribers__UserId] REFERENCES [dbo].[Users] ([Id]),
);
GO
