CREATE TYPE [dbo].[CenterList] AS TABLE (
	[Index]				INT					PRIMARY KEY,
	[Id]				INT					NOT NULL DEFAULT 0,
	[ParentIndex]		INT,
	[ParentId]			INT,  
	[CenterType]		NVARCHAR (255)		CHECK (
													[CenterType] IN (
														N'Abstract', N'BusinessUnit', N'CostOfSales',	N'SellingGeneralAndAdministration',
														N'SharedExpenseControl',  N'ConstructionInProgressExpendituresControl',
														N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl',
														N'WorkInProgressExpendituresControl', N'CurrentInventoriesInTransitExpendituresControl',
														N'OtherPL'
													)
												),
	[Name]				NVARCHAR (255)		NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[ManagerId]			INT,
	[Code]				NVARCHAR (50)
	INDEX IX_CenterList__Code ([Code])
);