CREATE TYPE [dbo].[LineDefinitionColumnList] AS TABLE
(
	[Index]					INT				DEFAULT 0,
	[HeaderIndex]			INT				DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT				DEFAULT 0,
	[TableName]				NVARCHAR (10)	NOT NULL,
	[ColumnName]			NVARCHAR (50),
	[EntryNumber]			INT,
	[Label]					NVARCHAR (50),
	[Label2]				NVARCHAR (50),
	[Label3]				NVARCHAR (50),
	[RequiredState]			SMALLINT		NOT NULL DEFAULT 4,
	[ReadOnlyState]			SMALLINT		NOT NULL DEFAULT 4
);