CREATE TABLE [dbo].[Messages]
(
	[Id]						INT					CONSTRAINT [PK_Messages] PRIMARY KEY IDENTITY,
	[CommandId]					INT CONSTRAINT [FK_Messages__CommandId] REFERENCES [dbo].[MessageCommands] ([Id]),
	[PhoneNumber]				NVARCHAR (15),
	[Content]					NVARCHAR (1600),
	[State]						SMALLINT			NOT NULL DEFAULT 0 CONSTRAINT [CK_SmsMessage__State] CHECK ([State] BETWEEN -4 AND +4),
	[StateSince]				DATETIMEOFFSET		NOT NULL,
	[ErrorMessage]				NVARCHAR (2048),
	[CreatedAt]					DATETIMEOFFSET(7)		NOT NULL DEFAULT SYSDATETIMEOFFSET(),
);
GO

CREATE INDEX [IX_SmsMessages__State] ON [dbo].[Messages]([State]);
GO