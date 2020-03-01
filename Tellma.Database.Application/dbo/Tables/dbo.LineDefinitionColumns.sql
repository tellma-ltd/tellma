CREATE TABLE [dbo].[LineDefinitionColumns]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionColumns] PRIMARY KEY IDENTITY,
	[LineDefinitionId]		NVARCHAR (50)	NOT NULL,
	[Index]					INT				NOT NULL,
	CONSTRAINT [IX_LineDefinitionColumns] UNIQUE ([LineDefinitionId], [Index]),
	[TableName]				NVARCHAR (10)	NOT NULL CHECK([TableName] IN (N'Lines', N'Entries')),
	[ColumnName]			NVARCHAR (50)	NOT NULL,
	[EntryNumber]			INT				NOT NULL DEFAULT 0,
	[Label]					NVARCHAR (50)	NOT NULL,
	[Label2]				NVARCHAR (50),
	[Label3]				NVARCHAR (50),
	[RequiredState]			SMALLINT		NOT NULL DEFAULT 4,
	[ReadOnlyState]			SMALLINT		NOT NULL DEFAULT 4
);