CREATE TABLE [dbo].[DocumentDefinitions] (
-- table managed by Banan
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different document types.
	[Id]						NVARCHAR (50)	CONSTRAINT [PK_DocumentDefinitions] PRIMARY KEY,
	-- IsPrimal, means that we are not copying the data from elsewhere. Instead, this is the only place where it exists
	-- Original is less confusing than Source Document. An SIV is not a source document, but can be primal
	[IsOriginalDocument]		BIT				DEFAULT 1,
	[TitleSingular]				NVARCHAR (255),
	[TitleSingular2]			NVARCHAR (255),
	[TitleSingular3]			NVARCHAR (255),
	[TitlePlural]				NVARCHAR (255),
	[TitlePlural2]				NVARCHAR (255),
	[TitlePlural3]				NVARCHAR (255),

--	[IsImmutable]				BIT				NOT NULL DEFAULT 0, -- 1 <=> Cannot change without invalidating signatures
	-- UI Specs
	[SortKey]					DECIMAL (9,4),
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[CodeWidth]					TINYINT			DEFAULT 3, -- For presentation purposes

	[MemoVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([MemoVisibility] IN (N'None', N'Optional', N'Required')),
	[AgentDefinitionId]			NVARCHAR (50)	CONSTRAINT [FK_DocumentDefinitions__AgentDefinitionId] REFERENCES dbo.AgentDefinitions([Id]),
	[AgentLabel]				NVARCHAR (50),
	[AgentLabel2]				NVARCHAR (50),
	[AgentLabel3]				NVARCHAR (50),
	[ClearanceVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ClearanceVisibility] IN (N'None', N'Optional', N'Required')),
	[InvestmentCenterVisibility]NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([InvestmentCenterVisibility] IN (N'None', N'Optional', N'Required')),
	[Time1Visibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Time1Visibility] IN (N'None', N'Optional', N'Required')),
	[Time1Label]				NVARCHAR (50),
	[Time1Label2]				NVARCHAR (50),
	[Time1Label3]				NVARCHAR (50),
	[Time2Visibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Time2Visibility] IN (N'None', N'Optional', N'Required')),
	[Time2Label]				NVARCHAR (50),
	[Time2Label2]				NVARCHAR (50),
	[Time2Label3]				NVARCHAR (50),
	[QuantityVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([QuantityVisibility] IN (N'None', N'Optional', N'Required')),
	[QuantityLabel]			NVARCHAR (50),
	[QuantityLabel2]			NVARCHAR (50),
	[QuantityLabel3]			NVARCHAR (50),
	[UnitVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([UnitVisibility] IN (N'None', N'Optional', N'Required')),
	[UnitLabel]				NVARCHAR (50),
	[UnitLabel2]				NVARCHAR (50),
	[UnitLabel3]				NVARCHAR (50),
	[State]						NVARCHAR (50)			DEFAULT N'Draft',	-- Deployed, Archived (Phased Out)
	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]			DECIMAL (9,4),
	[SavedById]					INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[DocumentDefinitionsHistory]));
GO;