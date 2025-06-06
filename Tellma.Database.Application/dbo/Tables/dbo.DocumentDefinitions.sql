CREATE TABLE [dbo].[DocumentDefinitions] (
-- table managed by Tellma implementation partner (e.g., Banan IT)
	[Id]						INT				CONSTRAINT [PK_DocumentDefinitions] PRIMARY KEY IDENTITY,
	[Code]						NVARCHAR (50)	CONSTRAINT [UQ_DocumentDefinitions__Code] UNIQUE NOT NULL,
	-- Is Original, means that we are not copying the data from elsewhere. Instead, this is the only place where it exists
	[IsOriginalDocument]		BIT				DEFAULT 1 NOT NULL,
	[Description]				NVARCHAR (1024)	NOT NULL,
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[TitleSingular]				NVARCHAR (50)	NOT NULL,
	[TitleSingular2]			NVARCHAR (50),
	[TitleSingular3]			NVARCHAR (50),
	[TitlePlural]				NVARCHAR (50)	NOT NULL,
	[TitlePlural2]				NVARCHAR (50),
	[TitlePlural3]				NVARCHAR (50),
	-- UI Specs
	[SortKey]					DECIMAL (9,4),
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[CodeWidth]					TINYINT			DEFAULT 3 NOT NULL, -- For presentation purposes
	
	[PostingDateVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'Optional' CONSTRAINT [CK_DocumentDefinitions__PostingDateVisibility] CHECK ([PostingDateVisibility] IN (N'None', N'Optional', N'Required')),
	[CenterVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'Optional' CONSTRAINT [CK_DocumentDefinitions__CenterVisibility] CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),

	[Lookup1Visibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CONSTRAINT [CK_DocumentDefinitions__Lookup1Visibility] CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
	[Lookup1DefinitionId]		INT				CONSTRAINT [FK_DocumentDefinitions__Lookup1DefinitionId] REFERENCES [dbo].[LookupDefinitions]([Id]),
	[Lookup1Label]				NVARCHAR (50),
	[Lookup1Label2]				NVARCHAR (50),
	[Lookup1Label3]				NVARCHAR (50),
	
	[Lookup2Visibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CONSTRAINT [CK_DocumentDefinitions__Lookup2Visibility] CHECK ([Lookup2Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup2DefinitionId]		INT				CONSTRAINT [FK_DocumentDefinitions__Lookup2DefinitionId] REFERENCES [dbo].[LookupDefinitions]([Id]),
	[Lookup2Label]				NVARCHAR (50),
	[Lookup2Label2]				NVARCHAR (50),
	[Lookup2Label3]				NVARCHAR (50),

	[ZatcaDocumentType]			NVARCHAR (3)  CONSTRAINT [CK_DocumentDefinitions__ZatcaDocumentType] CHECK ([ZatcaDocumentType] IN (N'381', N'383', N'386', N'388', N'389')),

	[ClearanceVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CONSTRAINT [CK_DocumentDefinitions__ClearanceVisibility] CHECK ([ClearanceVisibility] IN (N'None', N'Optional', N'Required')),
	[MemoVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CONSTRAINT [CK_DocumentDefinitions__MemoVisibility] CHECK ([MemoVisibility] IN (N'None', N'Optional', N'Required')),
	
	[AttachmentVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CONSTRAINT [CK_DocumentDefinitions__AttachmentVisibility] CHECK ([AttachmentVisibility] IN (N'None', N'Optional', N'Required')),
	[HasBookkeeping]			BIT				NOT NULL DEFAULT 1,
	[CloseValidateScript]		NVARCHAR (MAX), -- to store SQL code that validates the document in the save pipeline

	[State]						NVARCHAR (50)	NOT NULL DEFAULT N'Hidden' CONSTRAINT [CK_DocumentDefinitions__State] CHECK([State] IN (N'Hidden', N'Visible', N'Archived', N'Testing')),	-- Visible, Readonly (Phased Out)
	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]			DECIMAL (9,4),
	
	[SavedById]					INT				NOT NULL CONSTRAINT [FK_DocumentDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[DocumentDefinitionsHistory]));
GO;