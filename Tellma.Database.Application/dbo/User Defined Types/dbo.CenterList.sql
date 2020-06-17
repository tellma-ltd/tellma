CREATE TYPE [dbo].[CenterList] AS TABLE (
	[Index]				INT					PRIMARY KEY,
	[ParentIndex]		INT,
	[Id]				INT					NOT NULL DEFAULT 0,
	[ParentId]			INT,  
	[CenterType]		NVARCHAR (50) CHECK ([CenterType] IN (N'Segment',
													N'Abstract', N'Common', N'ServiceExtension', N'ProductionExtension',
													N'DistributionCosts', N'AdministrativeExpense', N'CostOfSales')
												),
	[Name]				NVARCHAR (255)		NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[ManagerId]			INT,
	[Code]				NVARCHAR (255)
	INDEX IX_CenterList__Code ([Code])
);