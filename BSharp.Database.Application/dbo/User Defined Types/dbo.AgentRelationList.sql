CREATE TYPE [dbo].[AgentRelationList] AS TABLE (
	[Index]						INT				PRIMARY KEY,
	[Id]						INT				NOT NULL DEFAULT 0,
	[AgentId]					INT				NOT NULL,
	[StartDate]					DATE			DEFAULT (CONVERT (date, SYSDATETIME())),
	[Code]						NVARCHAR (50),
--	customer
	[CustomerRating]			INT,			-- user defined list
	[ShippingAddress]			NVARCHAR (255), -- default, the full list is in a separate table
	[BillingAddress]			NVARCHAR (255),
	[CreditLine]				MONEY			DEFAULT 0,
--	employees
	[JobTitle]					NVARCHAR (50), -- FK to table Jobs
	[BasicSalary]				MONEY,
	[TransportationAllowance]	MONEY,
	[OvertimeRate]				MONEY,			-- probably better moved to a template table
	[PerDiemRate]				MONEY,			-- probably better moved to a template table
--	supplier
	[SupplierRating]			INT,			-- user defined list
	[PaymentTerms]				NVARCHAR (255)
);