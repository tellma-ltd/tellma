CREATE TABLE [dbo].[CustodyDefinitions]
(
	[Id]								INT				CONSTRAINT [PK_CustodyDefinitions] PRIMARY KEY IDENTITY,
	[Code]								NVARCHAR(50)	NOT NULL CONSTRAINT [IX_CustodyDefinitions__Code] UNIQUE,
	[TitleSingular]						NVARCHAR (50),
	[TitleSingular2]					NVARCHAR (50),
	[TitleSingular3]					NVARCHAR (50),
	[TitlePlural]						NVARCHAR (50)	NOT NULL,
	[TitlePlural2]						NVARCHAR (50),
	[TitlePlural3]						NVARCHAR (50),
	-----Contract properties common with resources
	[CurrencyVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CurrencyVisibility] IN (N'None', N'Optional', N'Required')),
	[CenterVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),
	[ImageVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ImageVisibility] IN (N'None', N'Optional', N'Required')),
	[DescriptionVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([DescriptionVisibility] IN (N'None', N'Optional', N'Required')),
	[LocationVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([LocationVisibility] IN (N'None', N'Optional', N'Required')),

	[FromDateLabel]						NVARCHAR (50),
	[FromDateLabel2]					NVARCHAR (50),
	[FromDateLabel3]					NVARCHAR (50),		
	[FromDateVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([FromDateVisibility] IN (N'None', N'Optional', N'Required')),
	[ToDateLabel]						NVARCHAR (50),
	[ToDateLabel2]						NVARCHAR (50),
	[ToDateLabel3]						NVARCHAR (50),
	[ToDateVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ToDateVisibility] IN (N'None', N'Optional', N'Required')),

	[Decimal1Label]						NVARCHAR (50),
	[Decimal1Label2]					NVARCHAR (50),
	[Decimal1Label3]					NVARCHAR (50),		
	[Decimal1Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Decimal1Visibility] IN (N'None', N'Optional', N'Required')),

	[Decimal2Label]						NVARCHAR (50),
	[Decimal2Label2]					NVARCHAR (50),
	[Decimal2Label3]					NVARCHAR (50),		
	[Decimal2Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Decimal2Visibility] IN (N'None', N'Optional', N'Required')),

	[Int1Label]							NVARCHAR (50),
	[Int1Label2]						NVARCHAR (50),
	[Int1Label3]						NVARCHAR (50),		
	[Int1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Int1Visibility] IN (N'None', N'Optional', N'Required')),

	[Int2Label]							NVARCHAR (50),
	[Int2Label2]						NVARCHAR (50),
	[Int2Label3]						NVARCHAR (50),		
	[Int2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Int2Visibility] IN (N'None', N'Optional', N'Required')),

	[Lookup1Label]						NVARCHAR (50),
	[Lookup1Label2]						NVARCHAR (50),
	[Lookup1Label3]						NVARCHAR (50),
	[Lookup1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
	[Lookup1DefinitionId]				INT				CONSTRAINT [FK_CustodyDefinitions__Lookup1DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup2Label]						NVARCHAR (50),
	[Lookup2Label2]						NVARCHAR (50),
	[Lookup2Label3]						NVARCHAR (50),
	[Lookup2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup2Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup2DefinitionId]				INT				CONSTRAINT [FK_CustodyDefinitions__Lookup2DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup3Label]						NVARCHAR (50),
	[Lookup3Label2]						NVARCHAR (50),
	[Lookup3Label3]						NVARCHAR (50),
	[Lookup3Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup3Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup3DefinitionId]				INT				CONSTRAINT [FK_CustodyDefinitions__Lookup3DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup4Label]						NVARCHAR (50),
	[Lookup4Label2]						NVARCHAR (50),
	[Lookup4Label3]						NVARCHAR (50),
	[Lookup4Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup4Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup4DefinitionId]				INT				CONSTRAINT [FK_CustodyDefinitions__Lookup4DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),

	[Text1Label]						NVARCHAR (50),
	[Text1Label2]						NVARCHAR (50),
	[Text1Label3]						NVARCHAR (50),		
	[Text1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text1Visibility] IN (N'None', N'Optional', N'Required')),

	[Text2Label]						NVARCHAR (50),
	[Text2Label2]						NVARCHAR (50),
	[Text2Label3]						NVARCHAR (50),		
	[Text2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text2Visibility] IN (N'None', N'Optional', N'Required')),

	[Script]							NVARCHAR (MAX),
	-----Properties applicable to custodies only
	[CustodianVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CustodianVisibility] IN (N'None', N'Optional', N'Required')),
	[CustodianDefinitionId]				INT				CONSTRAINT [FK_CustodyDefinitions__CustodianDefinitionId] REFERENCES dbo.RelationDefinitions([Id]),

	[AgentVisibility]					NVARCHAR (50),
	[TaxIdentificationNumberVisibility] NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([TaxIdentificationNumberVisibility] IN (N'None', N'Optional', N'Required')),

	[JobVisibility]						NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([JobVisibility] IN (N'None', N'Optional', N'Required')),
	[BankAccountNumberVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([BankAccountNumberVisibility] IN (N'None', N'Optional', N'Required')),

	[UserCardinality]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([UserCardinality] IN (N'None', N'Single', N'Multiple')),

	[State]								NVARCHAR (50)	NOT NULL DEFAULT N'Hidden' CHECK([State] IN (N'Hidden', N'Visible', N'Archived')),	-- Visible, Readonly (Phased Out)
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSection]					NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]					DECIMAL (9,4),

	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_CustodyDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[CustodyDefinitionsHistory]));
GO;