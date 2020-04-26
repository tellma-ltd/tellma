CREATE TABLE [dbo].[RelationDefinitions]
(
	[Id]								INT				CONSTRAINT [PK_RelationDefinitions] PRIMARY KEY IDENTITY,
	[Code]								NVARCHAR(50)	NOT NULL CONSTRAINT [IX_RelationDefinitions__Code] UNIQUE,
	[TitleSingular]						NVARCHAR (255),
	[TitleSingular2]					NVARCHAR (255),
	[TitleSingular3]					NVARCHAR (255),
	[TitlePlural]						NVARCHAR (255),
	[TitlePlural2]						NVARCHAR (255),
	[TitlePlural3]						NVARCHAR (255),
	[TaxIdentificationNumberVisibility] NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([TaxIdentificationNumberVisibility] IN (N'None', N'Optional', N'Required')),
	[ImageVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'Optional' CHECK ([ImageVisibility] IN (N'None', N'Optional', N'Required')),
	[StartDateVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([StartDateVisibility] IN (N'None', N'Optional', N'Required')),
	[StartDateLabel]					NVARCHAR (50),
	[StartDateLabel2]					NVARCHAR (50),
	[StartDateLabel3]					NVARCHAR (50),

	--[Prefix]							NVARCHAR (30)	DEFAULT (N''),
	--[CodeWidth]							TINYINT			DEFAULT (3), -- For presentation purposes
	[IsActive]							BIT				NOT NULL DEFAULT 1,

	[JobVisibility]						NVARCHAR (50), -- None, Visible, Required
	[RatesVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([StartDateVisibility] IN (N'None', N'Optional', N'Required')),
-- Filter on the resources allowed in table AgentRates
--	[AccountTypeParentId]				INT,
	[RatesLabel]						NVARCHAR (50),
	[RatesLabel2]						NVARCHAR (50),
	[RatesLabel3]						NVARCHAR (50),
	[BankAccountNumberVisibility]		NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([BankAccountNumberVisibility] IN (N'None', N'Optional', N'Required')),

	[UserVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'Optional' CHECK ([UserVisibility] IN (N'None', N'Optional', N'Required')),
	[AllowMultipleUsers]				BIT				NOT NULL DEFAULT 0,
	
	[State]								NVARCHAR (50)	DEFAULT N'Draft',	-- Deployed, Archived (Phased Out)
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSection]					NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]					DECIMAL (9,4),

	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_RelationtDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[RelationDefinitionsHistory]));
GO;