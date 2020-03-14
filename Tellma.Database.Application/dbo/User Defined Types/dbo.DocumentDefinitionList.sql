CREATE TYPE [dbo].[DocumentDefinitionList] AS TABLE (
	[Index]						INT	PRIMARY KEY,
	[Id]						NVARCHAR (50) NOT NULL UNIQUE,
	[IsOriginalDocument]		BIT				DEFAULT 1, -- <=> IsVoucherReferenceRequired = 0
	[TitleSingular]				NVARCHAR (255),
	[TitleSingular2]			NVARCHAR (255),
	[TitleSingular3]			NVARCHAR (255),
	[TitlePlural]				NVARCHAR (255),
	[TitlePlural2]				NVARCHAR (255),
	[TitlePlural3]				NVARCHAR (255),

	[IsImmutable]				BIT				NOT NULL DEFAULT 0, -- 1 <=> Cannot change without invalidating signatures
	-- UI Specs
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[CodeWidth]					TINYINT			DEFAULT 3, -- For presentation purposes
	[AgentDefinitionId]			NVARCHAR (50),
	[ClearanceVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ClearanceVisibility] IN (N'None', N'Optional', N'Required')),
	[InvestmentCenterVisibility]NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([InvestmentCenterVisibility] IN (N'None', N'Optional', N'Required')),
	[Time1Visibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Time1Visibility] IN (N'None', N'Optional', N'Required')),
	[Time1Label1]				NVARCHAR (50),
	[Time1Label2]				NVARCHAR (50),
	[Time1Label3]				NVARCHAR (50),
	[Time2Visibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Time2Visibility] IN (N'None', N'Optional', N'Required')),
	[Time2Label1]				NVARCHAR (50),
	[Time2Label2]				NVARCHAR (50),
	[Time2Label3]				NVARCHAR (50),
	[QuantityVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([QuantityVisibility] IN (N'None', N'Optional', N'Required')),
	[QuantityLabel1]			NVARCHAR (50),
	[QuantityLabel2]			NVARCHAR (50),
	[QuantityLabel3]			NVARCHAR (50),
	[UnitVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([UnitVisibility] IN (N'None', N'Optional', N'Required')),
	[UnitLabel1]				NVARCHAR (50),
	[UnitLabel2]				NVARCHAR (50),
	[UnitLabel3]				NVARCHAR (50),
	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),	-- IF Null, it does not show on the main menu
	[MainMenuSortKey]			DECIMAL (9,4)
);