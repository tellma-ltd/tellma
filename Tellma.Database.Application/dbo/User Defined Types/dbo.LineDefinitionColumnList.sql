CREATE TYPE [dbo].[LineDefinitionColumnList] AS TABLE
(
	[Index]					INT				DEFAULT 0,
	[HeaderIndex]			INT				DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT				DEFAULT 0,
	[ColumnName]			NVARCHAR (50),
	[EntryIndex]			INT,
	[Label]					NVARCHAR (50),
	[Label2]				NVARCHAR (50),
	[Label3]				NVARCHAR (50),
	[Filter]				NVARCHAR (255),
	[InheritsFromHeader]	TINYINT			NOT NULL DEFAULT 0,
	[VisibleState]			SMALLINT		NOT NULL DEFAULT 0,
	[RequiredState]			SMALLINT		NOT NULL DEFAULT 4,
	[ReadOnlyState]			SMALLINT		NOT NULL DEFAULT 4,
	CHECK ([VisibleState] <= [RequiredState] AND [RequiredState] <= [ReadOnlyState])
);