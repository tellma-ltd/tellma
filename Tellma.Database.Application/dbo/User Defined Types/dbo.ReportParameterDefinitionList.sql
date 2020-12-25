CREATE TYPE [dbo].[ReportParameterDefinitionList] AS TABLE
(
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]						INT NOT NULL DEFAULT 0,
	[Key]						NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[Visibility]				NVARCHAR (50), -- N'None', N'Optional', N'Required'
	[Value]						NVARCHAR (255),
	[Control]					NVARCHAR (50),  -- 'text', 'number', 'decimal', 'date', 'boolean', 'Resource'
	[ControlOptions]			NVARCHAR (1024)
)
