CREATE TABLE [dbo].[AccountDefinitions]
(
	[Id]				INT				PRIMARY KEY,-- IDENTITY,
	[MapFunction]		SMALLINT		NOT NULL DEFAULT 0,
	CONSTRAINT [UX_AccountDefinitions] UNIQUE([Id], [MapFunction]),
	[ShowOCE]			BIT				NOT NULL DEFAULT 0,
	[Code]				NVARCHAR (50)	NOT NULL, -- Kebab case
	[Name]				NVARCHAR (50)	NOT NULL,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50)
	-- 0 Set Value, 1 By Contract, 2 By Resource, 3 By Center
	-- 21: By Resource Lookup1 22: By Resource Lookup1 and Contract Id
);