CREATE TYPE [dbo].[IdentityServerClientList] AS TABLE
(
	[Index]			INT				PRIMARY KEY DEFAULT 0,
	[Id]			INT				NOT NULL DEFAULT 0,
	[Name]			NVARCHAR (255),
	[Memo]			NVARCHAR (1024),
	[ClientId]		NVARCHAR(35),
	[ClientSecret]	NVARCHAR(255)
)
