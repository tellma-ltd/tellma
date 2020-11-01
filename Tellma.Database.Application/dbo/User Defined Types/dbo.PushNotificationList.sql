CREATE TYPE [dbo].[PushNotificationList] AS TABLE (
	[Index]						INT					PRIMARY KEY DEFAULT 0,
	[Endpoint]					NVARCHAR (15),
	[P256dh]					NVARCHAR (15),
	[Auth]						NVARCHAR (15),
	[Title]						NVARCHAR (2048),
	[Body]						NVARCHAR (2048),
	[Content]					NVARCHAR (MAX), -- JSON
	[State]						SMALLINT,
	[ErrorMessage]				NVARCHAR (2048)
);