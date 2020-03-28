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
	--[DebitAgentDefinitionId]	NVARCHAR (50)	CONSTRAINT [FK_DocumentDefinitions__DebitAgentDefinitionId] REFERENCES dbo.AgentDefinitions([Id]),
	--[CreditAgentDefinitionId]	NVARCHAR (50)	CONSTRAINT [FK_DocumentDefinitions__CreditAgentDefinitionId] REFERENCES dbo.AgentDefinitions([Id]),
	--[NotedAgentDefinitionId]	NVARCHAR (50)	CONSTRAINT [FK_DocumentDefinitions__NotedAgentDefinitionId] REFERENCES dbo.AgentDefinitions([Id]),
	[ClearanceVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ClearanceVisibility] IN (N'None', N'Optional', N'Required')),
	[State]						NVARCHAR (50)	DEFAULT N'Draft',	-- Deployed, Archived (Phased Out)
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