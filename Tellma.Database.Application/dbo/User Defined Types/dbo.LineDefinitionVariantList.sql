CREATE TYPE [dbo].[LineDefinitionVariantList] AS TABLE
(
	[Index]					TINYINT,
	[HeaderIndex]			INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT			DEFAULT 0,
	[Name]					NVARCHAR (50) NOT NULL,
	[Name2]					NVARCHAR (50),
	[Name3]					NVARCHAR (50)
);