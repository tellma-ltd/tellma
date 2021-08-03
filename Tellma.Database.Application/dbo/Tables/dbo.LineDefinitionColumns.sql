CREATE TABLE [dbo].[LineDefinitionColumns]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionColumns] PRIMARY KEY IDENTITY,
	[LineDefinitionId]		INT				NOT NULL CONSTRAINT [FK_LineDefinitionColumns_LineDefinitionId] REFERENCES dbo.[LineDefinitions]([Id]) ON DELETE CASCADE,
	[Index]					INT				NOT NULL,
	CONSTRAINT [IX_LineDefinitionColumns] UNIQUE ([LineDefinitionId], [Index]),
	[ColumnName]			NVARCHAR (50)	NOT NULL,
	[EntryIndex]			INT				NOT NULL DEFAULT 0,
	-- TODO: replace the following constraint with logic validation in Save
--	CONSTRAINT [FK_LineDefinitionColumns__LineDefinitionId_EntryIndex] FOREIGN KEY ([LineDefinitionId], [EntryIndex]) REFERENCES dbo.LineDefinitionEntries([LineDefinitionId], [Index]),
	[Label]					NVARCHAR (50)	NOT NULL,
	[Label2]				NVARCHAR (50),
	[Label3]				NVARCHAR (50),
	[Filter]				NVARCHAR (255),
	[InheritsFromHeader]	TINYINT			NOT NULL DEFAULT 0, -- 0: No, 1: Tab, 2: Document
	[VisibleState]			SMALLINT		NOT NULL DEFAULT 0,
	[RequiredState]			SMALLINT		NOT NULL DEFAULT 4,
	[ReadOnlyState]			SMALLINT		NOT NULL DEFAULT 4,
	CONSTRAINT [CK_LineDefinitionColumns__VisibleState_ReadOnlyState_RequiredState] CHECK (
			[VisibleState] <= [RequiredState] AND [RequiredState] <= [ReadOnlyState]
		),
	CONSTRAINT [CK_LineDefinitionColumns__ColumnName_RequiredState] CHECK (
			[ColumnName] NOT IN (N'CurrencyId', N'CenterId') OR [RequiredState] = 0
		),
	[SavedById]				INT				NOT NULL CONSTRAINT [FK_LineDefinitionColumns__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionColumnsHistory]));
GO;