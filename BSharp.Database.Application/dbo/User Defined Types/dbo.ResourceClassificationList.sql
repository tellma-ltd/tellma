CREATE TYPE [dbo].[ResourceClassificationList] AS TABLE (
	[Index]					INT PRIMARY KEY ,
	[Id]					INT NOT NULL DEFAULT 0,
	[Code]					NVARCHAR (255)	NOT NULL UNIQUE,
	[Name]					NVARCHAR (255)	NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Path]					NVARCHAR (255),
	[ResourceDefinitionId]	NVARCHAR (50)	DEFAULT N'monetary-resources', -- Basic shows only the name and the primary measurement unit type
	[IsAssignable]			BIT				DEFAULT 1
);