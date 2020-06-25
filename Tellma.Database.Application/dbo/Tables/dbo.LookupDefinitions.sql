CREATE TABLE [dbo].[LookupDefinitions]
(
	[Id]						INT						CONSTRAINT [PK_LookupDefinitions] PRIMARY KEY IDENTITY,
	[Code]						NVARCHAR (50)			NOT NULL CONSTRAINT [IX_LookupDefinitions] UNIQUE,
	[TitleSingular]				NVARCHAR (255),
	[TitleSingular2]			NVARCHAR (255),
	[TitleSingular3]			NVARCHAR (255),
	[TitlePlural]				NVARCHAR (255),
	[TitlePlural2]				NVARCHAR (255),
	[TitlePlural3]				NVARCHAR (255),
	[State]						NVARCHAR (50)	NOT NULL DEFAULT N'Hidden' CHECK([State] IN (N'Hidden', N'Visible', N'Archived')),	-- Visible, Readonly (Phased Out)
	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),			-- Required when the state is "Deployed"
	[MainMenuSortKey]			DECIMAL (9,4),
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LookupDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LookupDefinitionsHistory]));
GO;