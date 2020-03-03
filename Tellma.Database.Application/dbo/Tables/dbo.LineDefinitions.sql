CREATE TABLE [dbo].[LineDefinitions] (
	[Id]								NVARCHAR (50)	CONSTRAINT [PK_LineDefinitions] PRIMARY KEY,
	[Description]						NVARCHAR (255),
	[Description2]						NVARCHAR (255),
	[Description3]						NVARCHAR (255),
	[TitleSingular]						NVARCHAR (255) NOT NULL,
	[TitleSingular2]					NVARCHAR (255),
	[TitleSingular3]					NVARCHAR (255),
	[TitlePlural]						NVARCHAR (255) NOT NULL,
	[TitlePlural2]						NVARCHAR (255),
	[TitlePlural3]						NVARCHAR (255),
	[AgentDefinitionList]				NVARCHAR (1024),
	[ResponsibilityTypeList]			NVARCHAR (1024),
	[AllowSelectiveSigning]				BIT DEFAULT 0,
	[Script]							NVARCHAR (MAX), -- to store SQL code that populates the line
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionsHistory]));
GO;