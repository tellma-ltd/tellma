CREATE TABLE [dbo].[BudgetEntries] (
	[Id]					INT				CONSTRAINT [PK_BudgetEntries] PRIMARY KEY IDENTITY,
	[FromDate]				DATE			NOT NULL,
	[ToDate]				DATE			NOT NULL,
	CONSTRAINT [CK_BudgetEntries__FromDate_ToDate] CHECK ([FromDate] <= [ToDate]),
	[BudgetId]				INT				NOT NULL CONSTRAINT [FK_Budgets__BudgetId] REFERENCES [dbo].[Budgets] ([Id]),
	[MonetaryValue]			DECIMAL (19,4)	NOT NULL DEFAULT 0,
	[Value]					DECIMAL (19,4)	NOT NULL DEFAULT 0,
	[Count]					DECIMAL			NOT NULL DEFAULT 0, -- Count Unit
	[CountUnitId]			INT,			-- move it to Resources as PlanCountUnitId?
	[Mass]					DECIMAL			NOT NULL DEFAULT 0, -- Mass Unit, like LTZ bar, cement bag, etc
	[MassUnitId]			INT,			-- move it to Resources as PlanMassUnitId?
	[Volume]				DECIMAL			NOT NULL DEFAULT 0, -- Volume Unit, possibly for shipping
	[VolumeUnitId]			INT,			-- move it to Resources as PlanVolumeUnitId?
	[Area]					DECIMAL			NOT NULL DEFAULT 0, -- Area Unit, possibly for lands
	[AreaUnitId]			INT,			-- move it to Resources as PlanAreaUnitId?
	[Time]					DECIMAL			NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[TimeUnitId]			INT,

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId'))
)