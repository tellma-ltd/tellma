CREATE TABLE [dbo].[Emails]
(
	[Id]						INT					CONSTRAINT [PK_Emails] PRIMARY KEY IDENTITY,
	[ToEmail]					NVARCHAR (256),
	[Subject]					NVARCHAR (1024),
	[Body]						NVARCHAR (MAX),

	/*
		0 = Scheduled
		1 = In Progress
		2 = Dispatched (To SendGrid)
		3 = Delivered (To Recipient)
		4 = Opened email
		5 = Clicked email link
		-1 = Validation Failed (Locally)
		-2 = Dispatch Failed (To SendGrid)
		-3 = Delivery Failed (Bounced from Recipient server)
		-4 = Reported Spam
	*/
	[State]						SMALLINT			NOT NULL DEFAULT 0 CONSTRAINT [CK_Email__State] CHECK ([State] BETWEEN -4 AND +5), 
	[ErrorMessage]				NVARCHAR (2048),
	[StateSince]				DATETIMEOFFSET		NOT NULL,
	[DeliveredAt]				DATETIMEOFFSET		NULL,
	[OpenedAt]					DATETIMEOFFSET		NULL,
);
GO

CREATE INDEX [IX_Emails__State] ON [dbo].[Emails]([State]);
GO