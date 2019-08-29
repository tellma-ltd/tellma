CREATE TYPE [dbo].[UserList] AS TABLE
(
	[Index]			INT				PRIMARY KEY,
	[Id]			INT				NOT NULL DEFAULT 0,
	[Email]			NVARCHAR (255)	NOT NULL
)
