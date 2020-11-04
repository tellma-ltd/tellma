CREATE TABLE [dbo].[SmsMessages]
(
	[Id]						INT					CONSTRAINT [PK_Messages] PRIMARY KEY IDENTITY,
	[ToPhoneNumber]				NVARCHAR (15),
	[Message]					NVARCHAR (1600),
	[State]						SMALLINT			NOT NULL DEFAULT 0 CONSTRAINT [CK_SmsMessage__State] CHECK ([State] BETWEEN -4 AND +4),
	[StateSince]				DATETIMEOFFSET		NOT NULL,
	[ErrorMessage]				NVARCHAR (2048)
);
GO

CREATE INDEX [IX_SmsMessages__State] ON [dbo].[SmsMessages]([State]);
GO