CREATE TYPE [dbo].[LineDefinitionGenerateParameterList] AS TABLE
(
	[Index]					INT				DEFAULT 0,
	[HeaderIndex]			INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]					INT				NOT NULL DEFAULT 0,
	[Key]					NVARCHAR (50)	NOT NULL,
	[Label]					NVARCHAR (50),
	[Label2]				NVARCHAR (50),
	[Label3]				VARCHAR (50),
	[Visibility]			NVARCHAR (50), -- N'None', N'Optional', N'Required'
	[DataType]				NVARCHAR (50),
	[Filter]				NVARCHAR (255)
);