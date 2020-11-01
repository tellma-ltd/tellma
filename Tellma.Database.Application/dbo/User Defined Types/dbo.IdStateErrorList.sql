CREATE TYPE [dbo].[IdStateErrorList] AS TABLE (
	[Id]		INT					NOT NULL,
	[State]		SMALLINT			NOT NULL,
	[Error]		NVARCHAR (2048)		NULL
);