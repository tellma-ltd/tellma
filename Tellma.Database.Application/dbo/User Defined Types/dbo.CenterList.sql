CREATE TYPE [dbo].[CenterList] AS TABLE (
	[Index]				INT					PRIMARY KEY,
	[Id]				INT					NOT NULL DEFAULT 0,
	[ParentIndex]		INT,
	[ParentId]			INT,  
	[CenterType]		NVARCHAR (255),
	[Name]				NVARCHAR (255),
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (50)
	INDEX IX_CenterList__Code ([Code])
);