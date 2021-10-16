CREATE TABLE [dbo].[NotificationTemplateSubscribers]
(
	[Id] INT CONSTRAINT [PK_NotificationTemplateSubscribers] PRIMARY KEY IDENTITY,
	[NotificationTemplateId] INT NOT NULL CONSTRAINT [FK_NotificationTemplateSubscribers__NotificationTemplateId] REFERENCES [dbo].[NotificationTemplates] ([Id]) ON DELETE CASCADE,
	[AddressType] NVARCHAR (50) NOT NULL CONSTRAINT [CK_NotificationTemplateSubscribers__AddressType] CHECK ([AddressType] IN (N'User', N'Text')),
	[UserId] INT CONSTRAINT [FK_NotificationTemplateSubscribers__UserId] REFERENCES [dbo].[Users] ([Id]),
	[Email] NVARCHAR (1024), -- Template
	[Phone] NVARCHAR (1024), -- Template
	[LastNotificationCount] INT NOT NULL DEFAULT 1,
	[LastNotificationHash] NVARCHAR (255)
);
GO
