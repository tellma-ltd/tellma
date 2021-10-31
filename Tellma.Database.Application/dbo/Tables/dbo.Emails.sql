CREATE TABLE [dbo].[Emails]
(
	[Id]						INT					CONSTRAINT [PK_Emails] PRIMARY KEY IDENTITY,
	[To]						NVARCHAR (2048), -- Can hold multiple email addresses separated by ;
	[Cc]						NVARCHAR (2048), -- Can hold multiple email addresses separated by ;
	[Bcc]						NVARCHAR (2048), -- Can hold multiple email addresses separated by ;
	[Subject]					NVARCHAR (1024),
	[BodyBlobId]				NVARCHAR (50),
	[Body]						NVARCHAR (MAX), -- TODO: Remove after migrating to Blob storage

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
	[State]						SMALLINT				NOT NULL DEFAULT 0 CONSTRAINT [CK_Email__State] CHECK ([State] BETWEEN -4 AND +5), 
	[ErrorMessage]				NVARCHAR (2048),
	[StateSince]				DATETIMEOFFSET(7)		NOT NULL,
	[DeliveredAt]				DATETIMEOFFSET(7)		NULL,
	[OpenedAt]					DATETIMEOFFSET(7)		NULL,
	[CreatedAt]					DATETIMEOFFSET(7)		NOT NULL DEFAULT SYSDATETIMEOFFSET(),
);
GO

CREATE INDEX [IX_Emails__State] ON [dbo].[Emails]([State]);
GO