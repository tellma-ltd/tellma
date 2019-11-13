CREATE TABLE [dbo].[AgentRelations] (
	[Id]						INT					CONSTRAINT [PK_AgentRelations] PRIMARY KEY IDENTITY,
	[AgentRelationDefinitionId]	NVARCHAR(50)		NOT NULL CONSTRAINT [FK_AgentRelations__AgentRelationDefinitionId] FOREIGN KEY ([AgentRelationDefinitionId]) REFERENCES [dbo].[AgentRelationDefinitions] ([Id]),
	[AgentId]					INT					NOT NULL CONSTRAINT [FK_AgentRelations__AgentId] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agents] ([Id]) ON DELETE CASCADE,
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
	[BasicSalary]				MONEY,			-- As of now, typically part of direct labor expenses
	[TransportationAllowance]	MONEY,			-- As of now, typically part of overhead expenses.
	[OvertimeRate]				MONEY,			-- probably better moved to a template table
	[PerDiemRate]				MONEY,			-- probably better moved to a template table
--	suppliers
	[SupplierRating]			INT,			-- user defined list
	[PaymentTerms]				NVARCHAR (255),
	
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
);
GO
CREATE UNIQUE INDEX [IX_AgentRelations__AgentRelationDefinitionId_AgentId] ON dbo.AgentRelations([AgentRelationDefinitionId], [AgentId]);
GO
