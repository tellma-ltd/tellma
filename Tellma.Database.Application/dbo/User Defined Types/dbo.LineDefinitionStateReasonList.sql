CREATE TYPE [dbo].[LineDefinitionStateReasonList] AS TABLE (
	[Index]				INT		DEFAULT 0,
	[HeaderIndex]		INT		DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]				INT		DEFAULT 0,
	[State]				SMALLINT		NOT NULL,
	[Name]				NVARCHAR (50)	NOT NULL,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[IsActive]			BIT				DEFAULT 1
);