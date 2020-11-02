CREATE TYPE [dbo].[IdStateErrorTimestampList] AS TABLE (
	[Id]		INT					NOT NULL,
	[State]		SMALLINT			NOT NULL,
	[Error]		NVARCHAR (2048)		NULL,
	[Timestamp]	DATETIMEOFFSET (7)	NOT NULL
);