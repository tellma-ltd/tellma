CREATE TABLE [dbo].[Lines] (
--	These are for transactions only. If there are Lines from requests or inquiries, etc=> other tables
	[Id]						INT					CONSTRAINT [PK_Lines] PRIMARY KEY IDENTITY,
	[DocumentId]				INT					NOT NULL CONSTRAINT [FK_Lines__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[DefinitionId]				NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Lines__DefinitionId] REFERENCES [dbo].[LineDefinitions] ([Id]),
	[State]						SMALLINT			NOT NULL DEFAULT	0
	--CONSTRAINT [CK_Lines__State] CHECK ([State]	IN (N'Draft', N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Ready To Post')),
	CONSTRAINT [CK_Lines__State] CHECK ([State]		IN (0		,	-1,			+1,			-2,				+2,			-3,			+3,			-4,				+4)),
	--[RequestedAt]				DATETIMEOFFSET(7),
	--[AuthorizeddAt]				DATETIMEOFFSET(7),
	--[CompletedAt]				DATETIMEOFFSET(7),
	--[ReviewedAt]				DATETIMEOFFSET(7),
	[AgentId]					INT					CONSTRAINT [FK_Lines__AgentId] REFERENCES dbo.Agents([Id]), -- useful for storing the conversion agent in conversion transactions
	[ResourceId]				INT					CONSTRAINT [FK_Lines__ResourceId] REFERENCES dbo.Resources([Id]),
	[CurrencyId]				NCHAR (3)			CONSTRAINT [FK_Lines__CurrencyId] REFERENCES dbo.Currencies([Id]),

--	[Amount]					DECIMAL (19,4),

	[MonetaryValue]				DECIMAL (19,4),--			NOT NULL DEFAULT 0,
-- Tracking additive measures, the data type is to be decided by AA
	[Quantity]					DECIMAL (19,4),
	[UnitId]					INT CONSTRAINT [FK_Lines__UnitId] REFERENCES [dbo].[Units] ([Id]),

	[Value]						DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- equivalent in functional currency
-- Additional information to satisfy reporting requirements
	[Memo]						NVARCHAR (255), -- a textual description for statements and reports
-- While Voucher Number referes to the source document, this refers to any other identifying string 
-- for support documents, such as deposit slip reference, invoice number, etc...

	[SortKey]					INT				NOT NULL,
-- for auditing
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET() CONSTRAINT [FK_Lines__CreatedById]	FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	[CreatedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Lines__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
);
GO
CREATE INDEX [IX_Lines__DocumentId] ON [dbo].[Lines]([DocumentId]);
GO