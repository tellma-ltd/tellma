CREATE TABLE [dbo].[Notifications] ( -- TODO: Is it needed?
	[Id]				INT PRIMARY KEY,
	[RecipientId]		INT	NOT NULL, -- An agent ... Even those without AVATAR can be notified.
	[ContactChannel]	NVARCHAR (255)		NOT NULL,
	[ContactAddress]	NVARCHAR (255)		NOT NULL,
	[Message]			NVARCHAR (1024),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	CONSTRAINT [CK_Notifications_Channel] CHECK ([ContactChannel] IN (N'Sms', N'Email', N'Messenger', N'WhatsApp')),
	CONSTRAINT [FK_Notifications_RecipientId] FOREIGN KEY ([RecipientId]) REFERENCES [dbo].[Agents] ([Id])
);
GO
CREATE INDEX [IX_Notifications__RecipientId] ON [dbo].[Notifications]([RecipientId]);
GO