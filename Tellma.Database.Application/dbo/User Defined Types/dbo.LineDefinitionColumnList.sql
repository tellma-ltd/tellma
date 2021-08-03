CREATE TYPE [dbo].[LineDefinitionColumnList] AS TABLE
(
	[Index]					INT,
	[HeaderIndex]			INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT				NOT NULL DEFAULT 0,
	[ColumnName]			NVARCHAR (50),
	[EntryIndex]			INT,
	[Label]					NVARCHAR (50),
	[Label2]				NVARCHAR (50),
	[Label3]				NVARCHAR (50),
	[Filter]				NVARCHAR (255),
	[InheritsFromHeader]	TINYINT,
	[VisibleState]			SMALLINT,
	[RequiredState]			SMALLINT,
	[ReadOnlyState]			SMALLINT
);