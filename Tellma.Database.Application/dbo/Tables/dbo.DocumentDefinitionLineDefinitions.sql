CREATE TABLE [dbo].[DocumentDefinitionLineDefinitions]
(
	[Id]					INT				CONSTRAINT [PK_DocumentDefinitionLineDefinitions] PRIMARY KEY IDENTITY,
	[DocumentDefinitionId]	INT				NOT NULL CONSTRAINT [FK_DocumentDefinitionLineDefinitions_DocumentDefinitionId] REFERENCES dbo.DocumentDefinitions([Id]),
	[LineDefinitionId]		INT				NOT NULL CONSTRAINT [FK_DocumentDefinitionLineDefinitions_LineDefinitionId] REFERENCES dbo.LineDefinitions([Id]) ON DELETE CASCADE,
	UNIQUE ([DocumentDefinitionId], [LineDefinitionId]),
	[Index]					INT				NOT NULL,
	[IsVisibleByDefault]	BIT,
	[SavedById]				INT				NOT NULL CONSTRAINT [FK_DocumentDefinitionLineDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[DocumentDefinitionLineDefinitionsHistory]));
GO;