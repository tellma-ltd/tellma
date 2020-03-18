CREATE TYPE [dbo].[IndexIdWithStringHeaderList] AS TABLE
(
	[Index]	INT, --,
	[HeaderId] NVARCHAR(50),
	PRIMARY KEY([Index], [HeaderId]),
	[Id]	INT NOT NULL DEFAULT 0
)
