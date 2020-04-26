CREATE TABLE [dbo].[LineDefinitionColumns]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionColumns] PRIMARY KEY IDENTITY,
	[LineDefinitionId]		INT				NOT NULL,
	[Index]					INT				NOT NULL,
	CONSTRAINT [IX_LineDefinitionColumns] UNIQUE ([LineDefinitionId], [Index]),
	[ColumnName]			NVARCHAR (50)	NOT NULL,
	[EntryIndex]			INT				NOT NULL DEFAULT 0,
	CONSTRAINT [FK_LineDefinitionColumns__LineDefinitionId_EntryIndex] FOREIGN KEY ([LineDefinitionId], [EntryIndex]) REFERENCES dbo.LineDefinitionEntries([LineDefinitionId], [Index]),
	[Label]					NVARCHAR (50)	NOT NULL,
	[Label2]				NVARCHAR (50),
	[Label3]				NVARCHAR (50),
	[RequiredState]			SMALLINT		NOT NULL DEFAULT 4,
	[ReadOnlyState]			SMALLINT		NOT NULL DEFAULT 4,
	[InheritsFromHeader]	BIT				NOT NULL DEFAULT 0,
	[SavedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionColumns__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionColumnsHistory]));
GO;