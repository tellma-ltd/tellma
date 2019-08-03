CREATE TYPE [dbo].[DocumentLineList] AS TABLE (
	[Index]					INT,
	[DocumentIndex]			INT				NOT NULL,
	[Id]					INT NOT NULL,
	[DocumentId]			INT NOT NULL,
	[LineTypeId]			NVARCHAR (255)	NOT NULL,
	[TemplateLineId]		INT,
	[ScalingFactor]			FLOAT,
	
	[EntityState]		NVARCHAR (255)	NOT NULL DEFAULT(N'Inserted'),
	PRIMARY KEY ([Index]),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);