CREATE TABLE [dbo].[AgentRelations] (
	[Id]						INT					CONSTRAINT [PK_AgentRelations] PRIMARY KEY IDENTITY,
	[OperatingSegmentId]	INT					NOT NULL CONSTRAINT [FK_AgentRelations__ResponsibilityCenterId] REFERENCES dbo.[ResponsibilityCenters]([Id]),
	[AgentRelationDefinitionId]	NVARCHAR(50)		NOT NULL CONSTRAINT [FK_AgentRelations__AgentRelationDefinitionId] REFERENCES [dbo].[AgentRelationDefinitions]([Id]),
	[AgentId]					INT					NOT NULL CONSTRAINT [FK_AgentRelations__AgentId] REFERENCES [dbo].[Agents] ([Id]) ON DELETE CASCADE,
	[StartDate]					DATE				DEFAULT (CONVERT (date, SYSDATETIME())),
	[Code]						NVARCHAR (50), -- 
	[IsActive]					BIT					NOT NULL DEFAULT 1,
--	customers
	[CustomerRating]			INT,			-- user defined list
	[ShippingAddress]			NVARCHAR (255), -- default, the full list is in a separate table
	[BillingAddress]			NVARCHAR (255),
	[CreditLine]				MONEY				DEFAULT 0,
--	employees
	[JobTitle]					NVARCHAR (50), -- FK to table Jobs
	[BasicSalary]				MONEY,
	[TransportationAllowance]	MONEY,
	[OvertimeRate]				MONEY,
--	suppliers
	[SupplierRating]			INT,			-- user defined list
	[PaymentTerms]				NVARCHAR (255),
--	cost centers
	[CostObjectType]			NVARCHAR (50)		CONSTRAINT [CK_AgentRelations__CostObjectType] CHECK([CostObjectType] IN (
															N'CostUnit',
															--N'CostCenter', -- replaced by the ones underneath
															N'Production', -- this would be absorbed but not exactly
															N'SellingAndDistribution',
															N'Administration',
															N'Service', -- this should have zero expense after re-allocation
															N'Shared' -- should have zero expense after re-allocation
														)
													),
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AgentRelations__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AgentRelations__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
/*
	Agent Relation type		UDL (can only have ONE default account per (agent, relation type)
		N'investor'			-- 
		N'investment'		-- 
		N'custodian'
		N'depositor'
		N'bank'				-- Default, per bank account
		N'customer'			-- Default, per Sales order, per lease out, ...
		N'supplier'			-- Default, per Purchase Order, per LC, per lease in
		N'employee'			-- Default, per Contract
		N'debtor'			-- used by financial institutes
		N'creditor'			-- 
		N'custodian'		-- Default, per storage location. Use the code to define the location structure
							-- Includes warehouse/aisles/shelves/bins, factory/line/unit, farm/zone/..
		N'employer'			-- used for individuals doing personal accounting
		)),
	*/
/*
-- Cost Objects
WSI
	1 Admin
	11 Admin (includes finance, Internal audit and dept managers)
	2 S & D
	21 Bole Office
	22 AG Office
	.. specific marketing campaigns
	3 Service (fully apportioned to production, admin, marketing and sales, based on)
	31 HR & General services: (number of employees)
	32 Maintenance: (service hours), 
	33 MIS: (service hours)
	34 Warehouses: (store requisitions)
	4 Overhead (shared), fully apportioned to production lines
	41 Electricity (area)
	42 Insurance (assets cost)
	.. other 
	5 Production (absorved in cost units)
	51 Slitting (HR, CR): 
	52 pipe making
	53 Cut to size
	54 Coffee Processing (Production)
	9 Cost Units
	911 HSP (Kg)
	912 LTZ (Kg) (Cost Unit: Gross Profit)
	913 CP (Kg) (Cost Unit: Gross Profit)
	914 Other Products (Kg)
	921 Coffee (Kg) (Cost Unit),
*/
);
GO
CREATE UNIQUE INDEX [IX_AgentRelations__OperatingSegmentId_AgentRelationDefinitionId_AgentId] ON dbo.AgentRelations([OperatingSegmentId], [AgentRelationDefinitionId], [AgentId]);
GO
