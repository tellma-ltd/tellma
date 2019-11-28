CREATE TYPE [dbo].[AgentRelationList] AS TABLE (
	[Index]						INT				PRIMARY KEY,
	[Id]						INT				NOT NULL DEFAULT 0,
	[OperatingSegmentId]		INT				NOT NULL,
	[AgentId]					INT				NOT NULL,
	[StartDate]					DATE			DEFAULT (CONVERT (date, SYSDATETIME())),
	[Code]						NVARCHAR (50),
--	customer
	--[CustomerRating]			INT,			-- user defined list
	--[ShippingAddress]			NVARCHAR (255), -- default, the full list is in a separate table
	--[BillingAddress]			NVARCHAR (255),
	--[CreditLine]				MONEY			DEFAULT 0,
--	employees
	[JobId]						INT, -- FK to table Jobs
	[BasicSalary]				MONEY,
	[TransportationAllowance]	MONEY,
	[OvertimeRate]				MONEY,
	[BankAccountNumber]			NVARCHAR (34),
--	supplier
	--[SupplierRating]			INT,			-- user defined list
	--[PaymentTerms]				NVARCHAR (255),
--	cost centers
	[CostObjectType]			NVARCHAR (50)		CHECK([CostObjectType] IN (
															N'CostUnit',
															--N'CostCenter', -- replaced by the ones underneath
															N'Production', -- this would be absorbed but not exactly
															N'SellingAndDistribution',
															N'Administration',
															N'Service', -- this should have zero expense after re-allocation
															N'Shared' -- should have zero expense after re-allocation
														)
													)
);