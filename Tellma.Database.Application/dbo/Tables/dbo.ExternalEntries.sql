CREATE TABLE [dbo].[ExternalEntries] (
--	These are for transactions only. If there are ExternalEntries from requests or inquiries, etc=> other tables
	[Id]						INT				CONSTRAINT [PK_ExternalEntries] PRIMARY KEY IDENTITY,
	[PostingDate]				DATE			CONSTRAINT [CK_ExternalEntries__PostingDate] CHECK ([PostingDate] < DATEADD(DAY, 1, GETDATE())),
	--[Memo]						NVARCHAR (255), -- a textual description for statements and reports
	[Direction]					SMALLINT		NOT NULL CONSTRAINT [CK_ExternalEntries__Direction]	CHECK ([Direction] IN (-1, 1)),
	[AccountId]					INT				CONSTRAINT [FK_ExternalEntries__AccountId] REFERENCES [dbo].[Accounts] ([Id]),
	--[CurrencyId]				NCHAR (3)		CONSTRAINT [FK_ExternalEntries__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	--[CustodianId]				INT				CONSTRAINT [FK_ExternalEntries_CustodianId] REFERENCES dbo.[Relations] ([Id]),
	[CustodyId]					INT				CONSTRAINT [FK_ExternalEntries__CustodyId] REFERENCES dbo.[Custodies]([Id]),
	--[ParticipantId]				INT				CONSTRAINT [FK_ExternalEntries__PerticipantId] REFERENCES dbo.[Relations] ([Id]),
	--[ResourceId]				INT				CONSTRAINT [FK_ExternalEntries__ResourceId] REFERENCES dbo.[Resources]([Id]),
	--[CenterId]					INT				CONSTRAINT [FK_ExternalEntries__CentertId] REFERENCES dbo.[Centers]([Id]),
	[MonetaryValue]				DECIMAL (19,4),
	--[Quantity]					DECIMAL (19,4),
	--[UnitId]					INT				CONSTRAINT [FK_ExternalEntries__UnitId] REFERENCES [dbo].[Units] ([Id]),
	[ExternalReference]			NVARCHAR (50),
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ExternalEntries__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ExternalEntries__ModifiedById] REFERENCES [dbo].[Users] ([Id]),	
);
GO
CREATE INDEX [IX_ExternalEntries__AccountId] ON [dbo].[ExternalEntries]([AccountId]);
GO
--CREATE INDEX [IX_ExternalEntries__CurrencyId] ON [dbo].[ExternalEntries]([CurrencyId]);
--GO
--CREATE INDEX [IX_ExternalEntries__CenterId] ON [dbo].[ExternalEntries]([CenterId]);
--GO
--CREATE INDEX [IX_ExternalEntries__ResourceId] ON [dbo].[ExternalEntries]([ResourceId]);
--GO
--CREATE INDEX [IX_ExternalEntries__UnitId] ON [dbo].[ExternalEntries]([UnitId]);
--GO
CREATE INDEX [IX_ExternalEntries__CustodyId] ON [dbo].[ExternalEntries]([CustodyId]);
GO