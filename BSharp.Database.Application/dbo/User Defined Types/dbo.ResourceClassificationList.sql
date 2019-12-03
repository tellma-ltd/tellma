CREATE TYPE [dbo].[ResourceClassificationList] AS TABLE (
	[Index]				INT PRIMARY KEY ,
	[ParentIndex]		INT,
	[Id]				INT NOT NULL DEFAULT 0,
	[ParentId]			INT,
	[Name]					NVARCHAR (255)	NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Code]					NVARCHAR (255)	NOT NULL UNIQUE,
	[ResourceDefinitionId]	NVARCHAR (50)	DEFAULT N'monetary-resources', -- Basic shows only the name and the primary measurement unit type
	[IsAssignable]			BIT
);