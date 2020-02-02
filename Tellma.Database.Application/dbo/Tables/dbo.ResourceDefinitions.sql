CREATE TABLE [dbo].[ResourceDefinitions]
(
	[Id]								NVARCHAR (50)	NOT NULL CONSTRAINT [PK_ResourceDefinitions] PRIMARY KEY,
	[TitleSingular]						NVARCHAR (255),
	[TitleSingular2]					NVARCHAR (255),
	[TitleSingular3]					NVARCHAR (255),
	[TitlePlural]						NVARCHAR (255),
	[TitlePlural2]						NVARCHAR (255),
	[TitlePlural3]						NVARCHAR (255),
	[ParentAccountTypeId]				INT				CONSTRAINT [FK_ResourceDefinitions__ParentAccountTypeId] REFERENCES dbo.AccountTypes([Id]),
	-- If null, no restriction. Otherwise, it restricts the types to those stemming from one of the nodes in the parent list
	--[CodeRegEx]							NVARCHAR (255), -- Null means manually defined
	--[NameRegEx]							NVARCHAR (255), -- Null means manually defined
	-- Resource properties
	-- [AccountTypeVisibility]	NVARCHAR (50) DEFAULT N'None' CHECK ([ResourceClassificationVisibility] IN (N'None', N'Optional', N'Required')),
	[IdentifierLabel]					NVARCHAR (50),
	[IdentifierLabel2]					NVARCHAR (50),
	[IdentifierLabel3]					NVARCHAR (50),		
	[IdentifierVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([IdentifierVisibility] IN (N'None', N'Optional', N'Required')),
	[CurrencyVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([CurrencyVisibility] IN (N'None', N'Optional', N'Required')),
	[CountUnitVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([CountUnitVisibility] IN (N'None', N'Optional', N'Required')),
	[MassUnitVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([MassUnitVisibility] IN (N'None', N'Optional', N'Required')),
	[VolumeUnitVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([VolumeUnitVisibility] IN (N'None', N'Optional', N'Required')),
	[TimeUnitVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([TimeUnitVisibility] IN (N'None', N'Optional', N'Required')),
	-- [CustomsReferenceVisibility]		NVARCHAR (50) DEFAULT N'None' CHECK ([CustomsReferenceVisibility] IN (N'None', N'Optional', N'Required')),
	-- [PreferredSupplierVisibility]		NVARCHAR (50) DEFAULT N'None' CHECK ([PreferredSupplierVisibility] IN (N'None', N'Optional', N'Required')),
	[DescriptionVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([DescriptionVisibility] IN (N'None', N'Optional', N'Required')),
	[AvailableSinceLabel]				NVARCHAR (50),
	[AvailableSinceLabel2]				NVARCHAR (50),
	[AvailableSinceLabel3]				NVARCHAR (50),		
	[AvailableSinceVisibility]			NVARCHAR (50) DEFAULT N'None' CHECK ([AvailableSinceVisibility] IN (N'None', N'Optional', N'Required')),
	[AvailableTillLabel]				NVARCHAR (50),
	[AvailableTillLabel2]				NVARCHAR (50),
	[AvailableTillLabel3]				NVARCHAR (50),
	[AvailableTillVisibility]			NVARCHAR (50) DEFAULT N'None' CHECK ([AvailableTillVisibility] IN (N'None', N'Optional', N'Required')),
	[Lookup1Label]						NVARCHAR (50),
	[Lookup1Label2]						NVARCHAR (50),
	[Lookup1Label3]						NVARCHAR (50),
	[Lookup1Visibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
	[Lookup1DefinitionId]				NVARCHAR (50) CONSTRAINT [FK_ResourceDefinitions__Lookup1DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup2Label]						NVARCHAR (50),
	[Lookup2Label2]						NVARCHAR (50),
	[Lookup2Label3]						NVARCHAR (50),
	[Lookup2Visibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([Lookup2Visibility] IN (N'None', N'Required', N'Optional')),
	[Lookup2DefinitionId]				NVARCHAR (50) CONSTRAINT [FK_ResourceDefinitions__Lookup2DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	--[Lookup3Visibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([Lookup3Visibility] IN (N'None', N'Required', N'Optional')),
	--[Lookup3DefinitionId]				NVARCHAR (50) CONSTRAINT [FK_ResourceDefinitions__Lookup3DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	--[Lookup3Label]						NVARCHAR (50),
	--[Lookup3Label2]						NVARCHAR (50),
	--[Lookup3Label3]						NVARCHAR (50),
	--[Lookup4Visibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([Lookup4Visibility] IN (N'None', N'Required', N'Optional')),
	--[Lookup4DefinitionId]				NVARCHAR (50) CONSTRAINT [FK_ResourceDefinitions__Lookup4DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	--[Lookup4Label]						NVARCHAR (50),
	--[Lookup4Label2]						NVARCHAR (50),
	--[Lookup4Label3]						NVARCHAR (50),
	[DueDateLabel]						NVARCHAR (50),
	[DueDateLabel2]						NVARCHAR (50),
	[DueDateLabel3]						NVARCHAR (50),
	[DueDateVisibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([DueDateVisibility] IN (N'None', N'Required', N'Optional')),
	-- more properties from Resource Instances to come..

	[State]								NVARCHAR (50)				DEFAULT N'Draft',	-- Deployed, Archived (Phased Out)
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSection]					NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]					DECIMAL (9,4),
	
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[ResourceDefinitionsHistory]));
GO;