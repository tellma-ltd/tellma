CREATE TYPE [dbo].[IndexIdWithHeaderList] AS TABLE (
	[Index]	INT, --,
	[HeaderId] INT,
	PRIMARY KEY([Index], [HeaderId]),
	[Id]	INT NOT NULL DEFAULT 0
);