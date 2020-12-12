CREATE TYPE [dbo].[DocumentDefinitionList] AS TABLE (
	[Index]						INT	PRIMARY KEY,
	[Id]						INT	NOT NULL DEFAULT 0,
	[Code]						NVARCHAR (50) NOT NULL UNIQUE,
	[IsOriginalDocument]		BIT				DEFAULT 1, -- <=> IsVoucherReferenceRequired = 0
	[DocumentType]				TINYINT			NOT NULL DEFAULT 2,
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
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[CodeWidth]					TINYINT			DEFAULT 3, -- For presentation purposes
	
	[PostingDateVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([PostingDateVisibility] IN (N'None', N'Optional', N'Required')),
	[CenterVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),

	[ClearanceVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ClearanceVisibility] IN (N'None', N'Optional', N'Required')),
	[MemoVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([MemoVisibility] IN (N'None', N'Optional', N'Required')),

	[HasAttachments]			BIT NOT NULL DEFAULT 1,
	[HasBookkeeping]			BIT NOT NULL DEFAULT 1,

	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),	-- IF Null, it does not show on the main menu
	[MainMenuSortKey]			DECIMAL (9,4)
);