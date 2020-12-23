CREATE TABLE [dbo].[Entries] (
--	These are for transactions only. If there are entries from requests or inquiries, etc=> other tables
	[Id]						INT				CONSTRAINT [PK_Entries] PRIMARY KEY IDENTITY,
	[LineId]					INT				NOT NULL CONSTRAINT [FK_Entries__LineId] REFERENCES [dbo].[Lines] ([Id]) ON DELETE CASCADE,
	[Index]						INT				NOT NULL DEFAULT 0,
	CONSTRAINT [UX_Entries__LineId_Index] UNIQUE([LineId], [Index]),
	[IsSystem]					BIT				NOT NULL DEFAULT 0,
	[Direction]					SMALLINT		NOT NULL CONSTRAINT [CK_Entries__Direction]	CHECK ([Direction] IN (-1, 1)),
	[AccountId]					INT				NULL CONSTRAINT [FK_Entries__AccountId] REFERENCES [dbo].[Accounts] ([Id]),
	[CurrencyId]				NCHAR (3)		NOT NULL CONSTRAINT [FK_Entries__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	[CustodianId]				INT				CONSTRAINT [FK_Entries_CustodianId] REFERENCES dbo.[Relations] ([Id]),
	[CustodyId]					INT				CONSTRAINT [FK_Entries__CustodyId] REFERENCES dbo.[Custodies]([Id]),
	[ParticipantId]				INT				CONSTRAINT [FK_Entries__PerticipantId] REFERENCES dbo.[Relations] ([Id]),
	[ResourceId]				INT				CONSTRAINT [FK_Entries__ResourceId] REFERENCES dbo.[Resources]([Id]),
	[CenterId]					INT				NOT NULL CONSTRAINT [FK_Entries__CentertId] REFERENCES dbo.[Centers]([Id]),
	-- Entry Type Id is Required in Entries only if we have Parent Entry type in AccountTypes
	[EntryTypeId]				INT				CONSTRAINT [FK_Entries__EntryTypeId] REFERENCES [dbo].[EntryTypes] ([Id]),
	[BudgetId]					INT				CONSTRAINT [FK_Entries__BudgetId] REFERENCES dbo.[Budgets]([Id]),
	[MonetaryValue]				DECIMAL (19,4), --			NOT NULL DEFAULT 0,
-- Tracking additive measures
	-- Quantity & Unit are the ones in which the transaction is held (purchase, sales, production)
	[Quantity]					DECIMAL (19,4),
	[UnitId]					INT				CONSTRAINT [FK_Entries__UnitId] REFERENCES [dbo].[Units] ([Id]),
	[Value]						DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- equivalent in functional currency
	[RValue]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- re-instated in functional currency
	[PValue]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- equivalent in presentation currency
-- The following are sort of dynamic properties that capture information for reporting purposes
	[Time1]						DATETIME2 (2),	-- from time
	[Time2]						DATETIME2 (2),	-- to time
	-- Decimal1, Decimal2, Decimal3: VAT percent, WIP percent completion: DM, DL, O/H
	[ExternalReference]			NVARCHAR (50),
	[InternalReference]			NVARCHAR (50),
	[NotedAgentName]			NVARCHAR (50), -- In case, it is not necessary to define the agent, we simply capture the agent name.
	[NotedAmount]				DECIMAL (19,4),		-- e.g., amount subject to tax, or Control Quantity for poultry
	[NotedDate]					DATE,
-- for auditing
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Entries__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Entries__ModifiedById] REFERENCES [dbo].[Users] ([Id]),	
);
GO
CREATE INDEX [IX_Entries__LineId] ON [dbo].[Entries]([LineId]);
GO
CREATE INDEX [IX_Entries__AccountId] ON [dbo].[Entries]([AccountId]);
GO
CREATE INDEX [IX_Entries__CurrencyId] ON [dbo].[Entries]([CurrencyId]);
GO
CREATE INDEX [IX_Entries__CenterId] ON [dbo].[Entries]([CenterId]);
GO
CREATE INDEX [IX_Entries__ResourceId] ON [dbo].[Entries]([ResourceId]);
GO
CREATE INDEX [IX_Entries__UnitId] ON [dbo].[Entries]([UnitId]);
GO
CREATE INDEX [IX_Entries__CustodyId] ON [dbo].[Entries]([CustodyId]);
GO
CREATE INDEX [IX_Entries__ParticipantId] ON [dbo].[Entries]([ParticipantId]);
GO
CREATE INDEX [IX_Entries__EntryTypeId] ON [dbo].[Entries]([EntryTypeId]);
GO