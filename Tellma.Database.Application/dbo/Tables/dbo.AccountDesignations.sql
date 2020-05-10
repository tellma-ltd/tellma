CREATE TABLE [dbo].[AccountDesignations]
(
	[Id]				INT				CONSTRAINT [PK_AccountDesignations] PRIMARY KEY,-- IDENTITY,
	[MapFunction]		SMALLINT		NOT NULL DEFAULT -1,
	CONSTRAINT [UX_AccountDesignations] UNIQUE([Id], [MapFunction]),
	[ShowOCE]			BIT				NOT NULL DEFAULT 0,
	[Code]				NVARCHAR (50)	NOT NULL,
	[Name]				NVARCHAR (50)	NOT NULL,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50)
	-- -1 No mapping. or when several accounts can map to the same designation.
	-- Applies to basic B/S and basic P/L
	-- 0 Set Value, 1 By Contract, 2 By Resource, 3 By Center
	-- 21: By Resource Lookup1 22: By Resource Lookup1 and Contract Id
);