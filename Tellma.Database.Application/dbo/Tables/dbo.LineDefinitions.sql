CREATE TABLE [dbo].[LineDefinitions] (
	[Id]						INT 			CONSTRAINT [PK_LineDefinitions] PRIMARY KEY IDENTITY,
	[Code]						NVARCHAR (50)	NOT NULL CONSTRAINT [IX_LineDefinitions] UNIQUE,
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[TitleSingular]				NVARCHAR (50)	NOT NULL,
	[TitleSingular2]			NVARCHAR (50),
	[TitleSingular3]			NVARCHAR (50),
	[TitlePlural]				NVARCHAR (50)	NOT NULL,
	[TitlePlural2]				NVARCHAR (50),
	[TitlePlural3]				NVARCHAR (50),
	[AllowSelectiveSigning]		BIT				NOT NULL DEFAULT 0,
	[ViewDefaultsToForm]		BIT				NOT NULL DEFAULT 0,
	[GenerateScript]			NVARCHAR (MAX), -- to store SQL code that generates the line in the UI
	[GenerateLabel]				NVARCHAR (50),
	[GenerateLabel2]			NVARCHAR (50),
	[GenerateLabel3]			NVARCHAR (50),
	-- Preprocess script
	[Script]					NVARCHAR (MAX), -- to store SQL code that preprocesses the line in the save pipeline
	[SavedById]					INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionsHistory]));
GO;