CREATE TABLE [dbo].[BudgetEntries] (
	[Id]					INT				CONSTRAINT [PK_BudgetEntries] PRIMARY KEY IDENTITY,
	[FromDate]				DATE			NOT NULL,
	[ToDate]				DATE			NOT NULL,
	CONSTRAINT [CK_BudgetEntries__FromDate_ToDate] CHECK ([FromDate] <= [ToDate]),
	-- TODO: We might be able to use the same pivot trick used in details entries to rely only on default unit Id for budget entry
	[BudgetId]				INT				NOT NULL CONSTRAINT [FK_Budgets__BudgetId] REFERENCES [dbo].[Budgets] ([Id]),
	[MonetaryValue]			DECIMAL (19,4)	NOT NULL DEFAULT 0,
	[Value]					DECIMAL (19,4)	NOT NULL DEFAULT 0,
	[Quantity]					DECIMAL			NOT NULL DEFAULT 0, -- Count Unit
	[QuantityUnitId]			INT,			-- move it to Resources as PlanQuantityUnitId?
	[Mass]					DECIMAL			NOT NULL DEFAULT 0, -- Mass Unit, like LTZ bar, cement bag, etc
	[MassUnitId]			INT,			-- move it to Resources as PlanMassUnitId?

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId'))
)