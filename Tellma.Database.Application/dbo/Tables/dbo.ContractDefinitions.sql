CREATE TABLE [dbo].[ContractDefinitions]
(
	[Id]								INT				CONSTRAINT [PK_ContractDefinitions] PRIMARY KEY IDENTITY,
	[Code]								NVARCHAR(50)	NOT NULL CONSTRAINT [IX_ContractDefinitions__Code] UNIQUE,
	[TitleSingular]						NVARCHAR (255),
	[TitleSingular2]					NVARCHAR (255),
	[TitleSingular3]					NVARCHAR (255),
	[TitlePlural]						NVARCHAR (255),
	[TitlePlural2]						NVARCHAR (255),
	[TitlePlural3]						NVARCHAR (255),
	-----Contract properties common with resources
	[CurrencyVisibility]				NVARCHAR (50),
	[CenterVisibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),
 	[ImageVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ImageVisibility] IN (N'None', N'Optional', N'Required')),
	[DescriptionVisibility]				NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([DescriptionVisibility] IN (N'None', N'Optional', N'Required')),
	[LocationVisibility]/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([LocationVisibility] IN (N'None', N'Optional', N'Required')),

	-- need to refactor to fromDate and toDate
	[StartDateVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([StartDateVisibility] IN (N'None', N'Optional', N'Required')),
	[StartDateLabel]					NVARCHAR (50),
	[StartDateLabel2]					NVARCHAR (50),
	[StartDateLabel3]					NVARCHAR (50),

	[ToDateVisibility]	/*New*/			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ToDateVisibility] IN (N'None', N'Optional', N'Required')),
	[ToDateLabel]		/*New*/			NVARCHAR (50),
	[ToDateLabel2]		/*New*/			NVARCHAR (50),
	[ToDateLabel3]		/*New*/			NVARCHAR (50),
	
	[Decimal1Label]		/*New*/			NVARCHAR (50),
	[Decimal1Label2]	/*New*/			NVARCHAR (50),
	[Decimal1Label3]	/*New*/			NVARCHAR (50),		
	[Decimal1Visibility]/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Decimal1Visibility] IN (N'None', N'Optional', N'Required')),

	[Decimal2Label]		/*New*/			NVARCHAR (50),
	[Decimal2Label2]	/*New*/			NVARCHAR (50),
	[Decimal2Label3]	/*New*/			NVARCHAR (50),		
	[Decimal2Visibility]/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Decimal2Visibility] IN (N'None', N'Optional', N'Required')),

	[Int1Label]			/*New*/			NVARCHAR (50),
	[Int1Label2]		/*New*/			NVARCHAR (50),
	[Int1Label3]		/*New*/			NVARCHAR (50),		
	[Int1Visibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Int1Visibility] IN (N'None', N'Optional', N'Required')),

	[Int2Label]			/*New*/			NVARCHAR (50),
	[Int2Label2]		/*New*/			NVARCHAR (50),
	[Int2Label3]		/*New*/			NVARCHAR (50),		
	[Int2Visibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Int2Visibility] IN (N'None', N'Optional', N'Required')),

	[Lookup1Label]		/*New*/			NVARCHAR (50),
	[Lookup1Label2]		/*New*/			NVARCHAR (50),
	[Lookup1Label3]		/*New*/			NVARCHAR (50),
	[Lookup1Visibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
	[Lookup1DefinitionId]/*New*/		INT				CONSTRAINT [FK_ContractDefinitions__Lookup1DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup2Label]		/*New*/			NVARCHAR (50),
	[Lookup2Label2]		/*New*/			NVARCHAR (50),
	[Lookup2Label3]		/*New*/			NVARCHAR (50),
	[Lookup2Visibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Lookup2Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup2DefinitionId]/*New*/		INT				CONSTRAINT [FK_ContractDefinitions__Lookup2DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup3Visibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Lookup3Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup3DefinitionId]/*New*/		INT				CONSTRAINT [FK_ContractDefinitions__Lookup3DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup3Label]		/*New*/			NVARCHAR (50),
	[Lookup3Label2]		/*New*/			NVARCHAR (50),
	[Lookup3Label3]		/*New*/			NVARCHAR (50),
	[Lookup4Visibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Lookup4Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup4DefinitionId]/*New*/		INT				CONSTRAINT [FK_ContractDefinitions__Lookup4DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup4Label]		/*New*/			NVARCHAR (50),
	[Lookup4Label2]		/*New*/			NVARCHAR (50),
	[Lookup4Label3]		/*New*/			NVARCHAR (50),

	[Text1Label]		/*New*/			NVARCHAR (50),
	[Text1Label2]		/*New*/			NVARCHAR (50),
	[Text1Label3]		/*New*/			NVARCHAR (50),		
	[Text1Visibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Text1Visibility] IN (N'None', N'Optional', N'Required')),

	[Text2Label]		/*New*/			NVARCHAR (50),
	[Text2Label2]		/*New*/			NVARCHAR (50),
	[Text2Label3]		/*New*/			NVARCHAR (50),		
	[Text2Visibility]	/*New*/			NVARCHAR (50) NOT NULL DEFAULT N'None' CHECK ([Text2Visibility] IN (N'None', N'Optional', N'Required')),
	
	[Script]			/*New*/			NVARCHAR (MAX),
	[Prefix]							NVARCHAR (30)	DEFAULT (N''),
	[CodeWidth]							TINYINT			DEFAULT (3), -- For presentation purposes, used by script to generate resource code
	-----Properties applicable to contracts only
	[AgentVisibility]					NVARCHAR (50),
	[TaxIdentificationNumberVisibility] NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([TaxIdentificationNumberVisibility] IN (N'None', N'Optional', N'Required')),

	[JobVisibility]						NVARCHAR (50), -- None, Visible, Required
	[BankAccountNumberVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([BankAccountNumberVisibility] IN (N'None', N'Optional', N'Required')),

	[UserVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'Optional' CHECK ([UserVisibility] IN (N'None', N'Optional', N'Required')),
	[AllowMultipleUsers]				BIT				NOT NULL DEFAULT 0,

	[State]								NVARCHAR (50)	NOT NULL DEFAULT N'Hidden' CHECK([State] IN (N'Hidden', N'Visible', N'Archived')),	-- Visible, Readonly (Phased Out)
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSection]					NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]					DECIMAL (9,4),

	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ContracttDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[ContractDefinitionsHistory]));
GO;