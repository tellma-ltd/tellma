CREATE TYPE [dbo].[UserList] AS TABLE
(
	[Index]					INT				PRIMARY KEY DEFAULT 0,
	[Id]					INT				NOT NULL DEFAULT 0,
	[Name]					NVARCHAR (255)	NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Email]					NVARCHAR (255)	NOT NULL,
	[PreferredLanguage]		NCHAR (2),
	[PreferredCalendar]		NCHAR (2),
	[ContactEmail]			NVARCHAR (255),
	[ContactMobile]			NVARCHAR (50),
	[NormalizedContactMobile] NVARCHAR (50),
	[PreferredChannel]		NVARCHAR (255) DEFAULT N'Email',
	[EmailNewInboxItem]		BIT DEFAULT 0,
	[SmsNewInboxItem]		BIT DEFAULT 0,
	[PushNewInboxItem]		BIT DEFAULT 0,

	-- Extra Columns not in User.cs
	[ImageId]					NVARCHAR (50)
)