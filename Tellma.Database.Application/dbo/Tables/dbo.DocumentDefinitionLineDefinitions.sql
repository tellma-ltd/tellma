CREATE TABLE [dbo].[DocumentDefinitionLineDefinitions]
(
	[Id]					INT				CONSTRAINT [PK_DocumentDefinitionLineDefinitions] PRIMARY KEY IDENTITY,
	[DocumentDefinitionId]	NVARCHAR (50),
	[LineDefinitionId]		NVARCHAR (50),
	UNIQUE ([DocumentDefinitionId], [LineDefinitionId]),
	[IsVisibleByDefault]	BIT,
	[SavedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentDefinitionLineDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[DocumentDefinitionLineDefinitionsHistory]));
GO;