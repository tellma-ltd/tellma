﻿CREATE TABLE [dbo].[DocumentDefinitions] (
-- table managed by Banan
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different document types.
	[Id]						INT				CONSTRAINT [PK_DocumentDefinitions] PRIMARY KEY IDENTITY,
	[Code]						NVARCHAR (50)	CONSTRAINT [UX_DocumentDefinitions__Code] UNIQUE NOT NULL,
	-- Is Original, means that we are not copying the data from elsewhere. Instead, this is the only place where it exists
	[IsOriginalDocument]		BIT				DEFAULT 1 NOT NULL,
	[DocumentType]				TINYINT			NOT NULL DEFAULT 2, -- 0: Template, 1: Plan, 2: Event, 3: Regulatory
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
	
	[PostingDateVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'Optional' CHECK ([PostingDateVisibility] IN (N'None', N'Optional', N'Required')),
	[CenterVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'Optional' CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),

	[ClearanceVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ClearanceVisibility] IN (N'None', N'Optional', N'Required')),
	[MemoVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([MemoVisibility] IN (N'None', N'Optional', N'Required')),

	[HasAttachments]			BIT NOT NULL DEFAULT 1,
	[HasBookkeeping]			BIT NOT NULL DEFAULT 1,

	[State]						NVARCHAR (50)	NOT NULL DEFAULT N'Hidden' CHECK([State] IN (N'Hidden', N'Visible', N'Archived')),	-- Visible, Readonly (Phased Out)
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