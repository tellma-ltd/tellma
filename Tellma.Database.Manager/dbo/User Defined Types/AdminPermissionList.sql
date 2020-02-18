CREATE TYPE [dbo].[AdminPermissionList] AS TABLE
(
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
	PRIMARY KEY ([HeaderIndex], [Index]),
	[Id]			INT				NOT NULL	DEFAULT 0,
	[View]			NVARCHAR (255)	NOT NULL,
	[Action]		NVARCHAR (255)	NOT NULL,
	[Criteria]		NVARCHAR(1024), -- compiles into SQL expression to filter the applicability
	[Memo]			NVARCHAR (255)
)
