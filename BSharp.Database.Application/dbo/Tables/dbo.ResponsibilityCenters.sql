CREATE TABLE [dbo].[ResponsibilityCenters] (
/*
WSI
	10 Executive Office (includes Internal audit and dept managers)  (Cost Center: Admin)
	11 Finance (Cost Center: Admin)
	12 HR & General services (Cost Center: Service)
		111 HR Team (Cost Center: Service) apportioned to others by number of staff in each cost center
		112 Cafeteria (Cost Center: Service) apportioned to others by number of staff in each cost center
	13 Maintenance (Cost Center: Service) apportioned to other cost centers, based on the services hours they provided, 
	14 MIS (Cost Center: Service) apportioned to other cost centers, based on the services hours they provided
	15 Marketing & Sales
		211 AG Sales Unit (Cost Center: S&D, Salaries, PPE depreciation), (Revenue Center)
		212 Bole Sales Unit (Cost Center: S & D, Salaries, office rental, PPE depreciation), (Revenue Center)
		213 Marketing (Cost Center: S&D), includes labor, PPE and campaigns.
	16 Warehouses (Cost Center: Service) All expenses (labor, depreciation) apportioned by store requisitions		
	17 Production, -- each stage consumes Direct Materials, Services, Labor, Machines in addition to output of previous stage
		171 Slitting (HR, CR) (Cost Center: Production)
		172 pipe making (Cost Center: Production)
		173 Cut to size (Cost Center: Production)
		179 Production - Shared. e.g., electricity
	18 Coffee Processing
		30 Administration (Cost Center: Admin), [Offloads admin share from HR]
		91 Selling & Distribution (Cost Center: S&D),
		92 Processing (Cost Center: Production)
		99 Coffee Processing - Shared
	99 Shared (Steel & Coffee)

	Steel (Ooperating Segment)
		HSP (Kg) (Cost Unit: Gross Profit)
		LTZ (Kg) (Cost Unit: Gross Profit)
		CP (Kg) (Cost Unit: Gross Profit)
		Other Products (Kg)
	Coffee Processing (Operating Segment)
		
		Coffee (Kg) (Cost Unit: Gross Profit),


We identify business units as those whose managers may potentially prepare and submit a yearly budget. Eventually, those managers signatures 
are required for expense approvals
We may go further down by identifying activities in a business unit, and allocate expenses to those, for more accurate cost accounting
Budgets table: Id, Business unit, Direction, Account, Resource, ValueMeasure, Value, Period
Produced: Business Unit (sales), Direction (Debit), Account (FG inventory), Resource ...
Sold: Business Unit (sales), Direction (Debit), Account (Revenues), Resource ...
Closing FG Balance: Account(FG inventory), Resource, ValueMeasure Balance, Value Balance
Opening FG Balance: Account(FG inventory), Resource, ValueMeasure Balance, Value Balance
Produced = Sold + Closing - Opening

*/
-- some operations are used in the line corresponding to production event
	[Id]					INT					CONSTRAINT [PK_ResponsibilityCenters] PRIMARY KEY IDENTITY,
	[ResponsibilityDomain]	NVARCHAR (255)		NOT NULL, -- Investment, Profit, Revenue, Cost
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
-- (Ifrs 8) Profit or Investment Center, Performance regularly reviewed by CODM, discrete financial information is available
	[IsOperatingSegment]	BIT					NOT NULL DEFAULT 0, -- on each path from root to leaf, at most one O/S
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[ParentId]				INT, -- Only leaves can have data. Parents are represented by an extra leaf.
	[Code]					NVARCHAR (255),
-- Optional. used for convenient reporting
	[OperationId]			INT, -- e.g., general, admin, S&M, HR, finance, production, maintenance. FK to Agents (Departments)
	[ProductCategoryId]		INT, -- e.g., general, sales, services OR, Steel, Real Estate, Coffee, ..
	[GeographicRegionId]	INT, -- e.g., general, Oromia, Merkato, Kersa
	[CustomerSegmentId]		INT, -- e.g., general, then corporate, individual or M, F or Adult youth, etc...
	[TaxSegmentId]			INT, -- e.g., general, existing (30%), expansion (0%)

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResponsibilityCenters__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResponsibilityCenters__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [CK_ResponsibilityCenters__ResponsibilityDomain] CHECK ([ResponsibilityDomain] IN (N'Investment', N'Profit', N'Revenue', N'Cost')),
	CONSTRAINT [FK_ResponsibilityCenters__ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[ResponsibilityCenters] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ResponsibilityCenters__Name]
  ON [dbo].[ResponsibilityCenters]([Name]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ResponsibilityCenters__Name2]
  ON [dbo].[ResponsibilityCenters]([Name2]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ResponsibilityCenters__Name3]
  ON [dbo].[ResponsibilityCenters]([Name3]) WHERE [Name3] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ResponsibilityCenters__Code]
  ON [dbo].[ResponsibilityCenters]([Code]) WHERE [Code] IS NOT NULL;
GO