CREATE TYPE [dbo].[DocumentDefinitionList] AS TABLE (
	[Index]						INT	PRIMARY KEY,
	[Id]						INT NOT NULL DEFAULT 0,
	[Code]						NVARCHAR (50),
	[IsOriginalDocument]		BIT,
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[TitleSingular]				NVARCHAR (50),
	[TitleSingular2]			NVARCHAR (50),
	[TitleSingular3]			NVARCHAR (50),
	[TitlePlural]				NVARCHAR (50),
	[TitlePlural2]				NVARCHAR (50),
	[TitlePlural3]				NVARCHAR (50),

	-- UI Specs
	[Prefix]					NVARCHAR (5),
	[CodeWidth]					TINYINT, -- For presentation purposes
	
	[PostingDateVisibility]		NVARCHAR (50),
	[CenterVisibility]			NVARCHAR (50),
	
	[Lookup1Label]						NVARCHAR (50),
	[Lookup1Label2]						NVARCHAR (50),
	[Lookup1Label3]						NVARCHAR (50),
	[Lookup1Visibility]					NVARCHAR (50),
	[Lookup1DefinitionId]				INT,
	[Lookup2Label]						NVARCHAR (50),
	[Lookup2Label2]						NVARCHAR (50),
	[Lookup2Label3]						NVARCHAR (50),
	[Lookup2Visibility]					NVARCHAR (50),
	[Lookup2DefinitionId]				INT,

	[ZatcaDocumentType]			NVARCHAR (3), -- 381, 383, 388, 389

	[ClearanceVisibility]		NVARCHAR (50),
	[MemoVisibility]			NVARCHAR (50),

	[AttachmentVisibility]		NVARCHAR (50),
	[HasBookkeeping]			BIT,
	[CloseValidateScript]		NVARCHAR (MAX),

	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),	-- IF Null, it does not show on the main menu
	[MainMenuSortKey]			DECIMAL (9,4)
);