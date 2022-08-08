CREATE TYPE [dbo].[PeriodBenefitsList] AS TABLE (
	[Id] INT PRIMARY KEY DEFAULT 0,
	[EmployeeId] INT,
	[ResourceCode] NVARCHAR (255),
	[CurrencyId] NCHAR (3),
	[MonetaryValue] DECIMAL (19, 6),
	[Value] DECIMAL (19, 6)
);