CREATE TABLE [dbo].[Emails]
(
	[Id]						INT					CONSTRAINT [PK_Emails] PRIMARY KEY IDENTITY,
	[ToEmail]					NVARCHAR (256),
	[Subject]					NVARCHAR (1024),
	[Body]						NVARCHAR (MAX),
	[State]						SMALLINT			NOT NULL DEFAULT 0 CONSTRAINT [CK_Email__State] CHECK ([State] BETWEEN -3 AND +3), --   0 = Scheduled, 1 = In Progress, 2 = Dispatched, 3 = Delivered, -1 = Validation Failed, -2 = Dispatch Failed, -3 = Delivery Failed
	[ErrorMessage]				NVARCHAR (2048),
	[EngagementState]			SMALLINT			NOT NULL DEFAULT 0 CONSTRAINT [CK_Email__EngagementState] CHECK ([State] BETWEEN -1 AND +2), --   0 = None, 1 = Opened email, 2 = Clicked link inside email, -1 = Reported email as spam
	[StateSince]				DATETIMEOFFSET		NOT NULL,
	[EngagementStateSince]		DATETIMEOFFSET		NOT NULL,
);
GO

CREATE INDEX [IX_Emails__State] ON [dbo].[Emails]([State]);
GO