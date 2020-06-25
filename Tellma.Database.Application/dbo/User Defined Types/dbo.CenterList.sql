CREATE TYPE [dbo].[CenterList] AS TABLE (
	[Index]				INT					PRIMARY KEY,
	[ParentIndex]		INT,
	[Id]				INT					NOT NULL DEFAULT 0,
	[ParentId]			INT,  
	[CenterType]		NVARCHAR (50)		CHECK (		
												[CenterType] IN (
													N'Segment', N'Abstract', N'Parent', N'CostOfSales',	N'SellingGeneralAndAdministration',
													N'SharedExpenseControl', N'TransitExpenseControl', N'ConstructionExpenseControl',
													N'ProductionExpenseControl'
												)
											),
	[Name]				NVARCHAR (255)		NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[ManagerId]			INT,
	[Code]				NVARCHAR (50)
	INDEX IX_CenterList__Code ([Code])
);