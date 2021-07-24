CREATE TABLE [dbo].[RelationDefinitions]
(
	[Id]								INT				CONSTRAINT [PK_RelationDefinitions] PRIMARY KEY IDENTITY,
	[Code]								NVARCHAR(255)	NOT NULL CONSTRAINT [IX_RelationDefinitions__Code] UNIQUE,
	[TitleSingular]						NVARCHAR (255),
	[TitleSingular2]					NVARCHAR (255),
	[TitleSingular3]					NVARCHAR (255),
	[TitlePlural]						NVARCHAR (255)	NOT NULL,
	[TitlePlural2]						NVARCHAR (255),
	[TitlePlural3]						NVARCHAR (255),
	-----Contract properties common with resources
	[CurrencyVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CurrencyVisibility] IN (N'None', N'Optional', N'Required')),
	[CenterVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),
	[ImageVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ImageVisibility] IN (N'None', N'Optional', N'Required')),
	[DescriptionVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([DescriptionVisibility] IN (N'None', N'Optional', N'Required')),
	[LocationVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([LocationVisibility] IN (N'None', N'Optional', N'Required')),

	[FromDateVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([FromDateVisibility] IN (N'None', N'Optional', N'Required')),
	[FromDateLabel]						NVARCHAR (50),
	[FromDateLabel2]					NVARCHAR (50),
	[FromDateLabel3]					NVARCHAR (50),		

	[ToDateVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ToDateVisibility] IN (N'None', N'Optional', N'Required')),
	[ToDateLabel]						NVARCHAR (50),
	[ToDateLabel2]						NVARCHAR (50),
	[ToDateLabel3]						NVARCHAR (50),

	[DateOfBirthVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([DateOfBirthVisibility] IN (N'None', N'Optional', N'Required')),
	[ContactEmailVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ContactEmailVisibility] IN (N'None', N'Optional', N'Required')),
	[ContactMobileVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ContactMobileVisibility] IN (N'None', N'Optional', N'Required')),
	[ContactAddressVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ContactAddressVisibility] IN (N'None', N'Optional', N'Required')),

	[Date1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Date1Visibility] IN (N'None', N'Optional', N'Required')),
	[Date1Label]						NVARCHAR (50),
	[Date1Label2]						NVARCHAR (50),
	[Date1Label3]						NVARCHAR (50),		

	[Date2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Date2Visibility] IN (N'None', N'Optional', N'Required')),
	[Date2Label]						NVARCHAR (50),
	[Date2Label2]						NVARCHAR (50),
	[Date2Label3]						NVARCHAR (50),		

	[Date3Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Date3Visibility] IN (N'None', N'Optional', N'Required')),
	[Date3Label]						NVARCHAR (50),
	[Date3Label2]						NVARCHAR (50),
	[Date3Label3]						NVARCHAR (50),		

	[Date4Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Date4Visibility] IN (N'None', N'Optional', N'Required')),
	[Date4Label]						NVARCHAR (50),
	[Date4Label2]						NVARCHAR (50),
	[Date4Label3]						NVARCHAR (50),		

	[Decimal1Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Decimal1Visibility] IN (N'None', N'Optional', N'Required')),
	[Decimal1Label]						NVARCHAR (50),
	[Decimal1Label2]					NVARCHAR (50),
	[Decimal1Label3]					NVARCHAR (50),		

	[Decimal2Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Decimal2Visibility] IN (N'None', N'Optional', N'Required')),
	[Decimal2Label]						NVARCHAR (50),
	[Decimal2Label2]					NVARCHAR (50),
	[Decimal2Label3]					NVARCHAR (50),		

	[Int1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Int1Visibility] IN (N'None', N'Optional', N'Required')),
	[Int1Label]							NVARCHAR (50),
	[Int1Label2]						NVARCHAR (50),
	[Int1Label3]						NVARCHAR (50),		

	[Int2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Int2Visibility] IN (N'None', N'Optional', N'Required')),
	[Int2Label]							NVARCHAR (50),
	[Int2Label2]						NVARCHAR (50),
	[Int2Label3]						NVARCHAR (50),		

	[Lookup1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
	[Lookup1DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Lookup1DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup1Label]						NVARCHAR (50),
	[Lookup1Label2]						NVARCHAR (50),
	[Lookup1Label3]						NVARCHAR (50),
	
	[Lookup2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup2Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup2DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Lookup2DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup2Label]						NVARCHAR (50),
	[Lookup2Label2]						NVARCHAR (50),
	[Lookup2Label3]						NVARCHAR (50),

	[Lookup3Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup3Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup3DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Lookup3DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup3Label]						NVARCHAR (50),
	[Lookup3Label2]						NVARCHAR (50),
	[Lookup3Label3]						NVARCHAR (50),

	[Lookup4Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup4Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup4DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Lookup4DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup4Label]						NVARCHAR (50),
	[Lookup4Label2]						NVARCHAR (50),
	[Lookup4Label3]						NVARCHAR (50),

	[Lookup5Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup5Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup5DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Lookup5DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup5Label]						NVARCHAR (50),
	[Lookup5Label2]						NVARCHAR (50),
	[Lookup5Label3]						NVARCHAR (50),

	[Lookup6Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup6Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup6DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Lookup6DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup6Label]						NVARCHAR (50),
	[Lookup6Label2]						NVARCHAR (50),
	[Lookup6Label3]						NVARCHAR (50),

	[Lookup7Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup7Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup7DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Lookup7DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup7Label]						NVARCHAR (50),
	[Lookup7Label2]						NVARCHAR (50),
	[Lookup7Label3]						NVARCHAR (50),

	[Lookup8Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup8Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup8DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Lookup8DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup8Label]						NVARCHAR (50),
	[Lookup8Label2]						NVARCHAR (50),
	[Lookup8Label3]						NVARCHAR (50),

	[Text1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text1Visibility] IN (N'None', N'Optional', N'Required')),
	[Text1Label]						NVARCHAR (50),
	[Text1Label2]						NVARCHAR (50),
	[Text1Label3]						NVARCHAR (50),		

	[Text2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text2Visibility] IN (N'None', N'Optional', N'Required')),
	[Text2Label]						NVARCHAR (50),
	[Text2Label2]						NVARCHAR (50),
	[Text2Label3]						NVARCHAR (50),		

	[Text3Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text3Visibility] IN (N'None', N'Optional', N'Required')),
	[Text3Label]						NVARCHAR (50),
	[Text3Label2]						NVARCHAR (50),
	[Text3Label3]						NVARCHAR (50),		

	[Text4Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text4Visibility] IN (N'None', N'Optional', N'Required')),
	[Text4Label]						NVARCHAR (50),
	[Text4Label2]						NVARCHAR (50),
	[Text4Label3]						NVARCHAR (50),		

	[PreprocessScript]					NVARCHAR (MAX),
	[ValidateScript]					NVARCHAR (MAX),
	-----Properties applicable to relations only
	[Relation1Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Relation1Visibility] IN (N'None', N'Required', N'Optional')),
	[Relation1DefinitionId]				INT				CONSTRAINT [FK_RelationDefinitions__Relation1DefinitionId] REFERENCES dbo.RelationDefinitions([Id]),
	[Relation1Label]					NVARCHAR (50),
	[Relation1Label2]					NVARCHAR (50),
	[Relation1Label3]					NVARCHAR (50),

	[AgentVisibility]					NVARCHAR (50),
	[TaxIdentificationNumberVisibility] NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([TaxIdentificationNumberVisibility] IN (N'None', N'Optional', N'Required')),

	[BankAccountNumberVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([BankAccountNumberVisibility] IN (N'None', N'Optional', N'Required')),
	[ExternalReferenceVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ExternalReferenceVisibility] IN (N'None', N'Optional', N'Required')),
	[ExternalReferenceLabel]			NVARCHAR (50),
	[ExternalReferenceLabel2]			NVARCHAR (50),
	[ExternalReferenceLabel3]			NVARCHAR (50),

	[UserCardinality]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([UserCardinality] IN (N'None', N'Single', N'Multiple')),
	[HasAttachments]					BIT				NOT NULL DEFAULT 0,
	[AttachmentsCategoryDefinitionId]	INT				CONSTRAINT [FK_RelationDefinitions__AttachmentsCategoryDefinitionId] REFERENCES dbo.LookupDefinitions([Id]),

	[State]								NVARCHAR (50)	NOT NULL DEFAULT N'Hidden' CHECK([State] IN (N'Hidden', N'Visible', N'Archived')),	-- Visible, Readonly (Phased Out)
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSection]					NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]					DECIMAL (9,4),

	[SavedById]			INT				NOT NULL CONSTRAINT [FK_RelationDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[RelationDefinitionsHistory]));
GO;