CREATE TABLE [dbo].[Notifications] ( -- TODO: Is it needed?
	[Id]				INT					CONSTRAINT [PK_Notifications] PRIMARY KEY,
	 -- An agent ... Even those without AVATAR can be notified.
	[RecipientId]		INT					NOT NULL CONSTRAINT [FK_Notifications_RecipientId] REFERENCES [dbo].[Relations] ([Id]),
	[ContactChannel]	NVARCHAR (255)		NOT NULL CONSTRAINT [CK_Notifications_Channel] CHECK ([ContactChannel] IN (N'Sms', N'Email', N'Messenger', N'WhatsApp')),
	[ContactAddress]	NVARCHAR (255)		NOT NULL,
	[Message]			NVARCHAR (1024),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId'))	
);
GO
CREATE INDEX [IX_Notifications__RecipientId] ON [dbo].[Notifications]([RecipientId]);
GO