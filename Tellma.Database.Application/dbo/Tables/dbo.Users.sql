CREATE TABLE [dbo].[Users] (
	[Id]					INT					CONSTRAINT [PK_Users] PRIMARY KEY IDENTITY,
	[ExternalId]			NVARCHAR (450),
	[InvitedAt]				DATETIMEOFFSET(7),
	[State]					AS CAST(IIF([ExternalId] IS NOT NULL, 2, IIF([InvitedAt] IS NOT NULL, 1, 0)) AS TINYINT) PERSISTED, -- 2 = Member, 1 = Invited, 0 = New
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Email]					NVARCHAR (255)		NOT NULL CONSTRAINT [IX_Users__Email] UNIQUE, -- Required
	[ImageId]				NVARCHAR (50),
	[PreferredLanguage]		NCHAR (2),
	[PreferredCalendar]		NCHAR (2),

	-- Notifications stuff
	[ContactEmail]			NVARCHAR (255),
	[ContactMobile]			NVARCHAR (50),
	[NormalizedContactMobile]	NVARCHAR (50),
	[PushEndpoint]			NVARCHAR (1024),
	[PushP256dh]			NVARCHAR (1024),
	[PushAuth]				NVARCHAR (1024),
	[PreferredChannel]		NVARCHAR (10)		NOT NULL DEFAULT N'Email' CONSTRAINT [CK_Users__PreferredChannel] CHECK ([PreferredChannel] IN (N'Email', N'Sms', N'Push')),
	[EmailNewInboxItem]		BIT					NOT NULL DEFAULT 0,
	[SmsNewInboxItem]		BIT					NOT NULL DEFAULT 0,
	[PushNewInboxItem]		BIT					NOT NULL DEFAULT 0,

	[LastAccess]			DATETIMEOFFSET(7),
	[PermissionsVersion]	UNIQUEIDENTIFIER	NOT NULL DEFAULT NEWID(),
	[UserSettingsVersion]	UNIQUEIDENTIFIER	NOT NULL DEFAULT NEWID(),
	
	[LastInboxCheck]		DATETIMEOFFSET(7)	NULL,
	[LastNotificationsCheck] DATETIMEOFFSET(7)	NULL,


	[SortKey]				DECIMAL (9,4),

	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL CONSTRAINT [FK_Users__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL CONSTRAINT [FK_Users__ModifiedById] REFERENCES [dbo].[Users] ([Id])
	-- ??	
);
GO
--CREATE CLUSTERED INDEX [IX_LocalUsers__SortKey]
--  ON [dbo].[Users]([SortKey]);
--GO