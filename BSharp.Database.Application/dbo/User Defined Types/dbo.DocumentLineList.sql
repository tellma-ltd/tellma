CREATE TYPE [dbo].[DocumentLineList] AS TABLE (
	[Index]					INT				PRIMARY KEY,--	IDENTITY (0,1),
	[DocumentIndex]			INT				NOT NULL DEFAULT 0,
	[Id]					INT				NOT NULL DEFAULT 0,
	[DocumentId]			INT				NOT NULL DEFAULT 0,
	[LineTypeId]			NVARCHAR (255)	NOT NULL,
	[TemplateLineId]		INT,
	[ScalingFactor]			FLOAT,
	[SortKey]				DECIMAL (9,4)	NOT NULL
);