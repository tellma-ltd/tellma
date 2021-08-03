CREATE TYPE [dbo].[AdminUserList] AS TABLE
(
	[Index]			INT				PRIMARY KEY DEFAULT 0,
	[Id]			INT				DEFAULT 0,
	[Name]			NVARCHAR (255),
	[Email]			NVARCHAR (255)
)
