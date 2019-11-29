CREATE TYPE [dbo].[UserList] AS TABLE
(
	[Index]			INT				PRIMARY KEY,
	[Id]			INT				NOT NULL DEFAULT 0,
	[Name]			NVARCHAR (255)	NOT NULL,
	[Name2]			NVARCHAR (255),
	[Name3]			NVARCHAR (255),
	[Email]			NVARCHAR (255)	NOT NULL
)
