CREATE TYPE [dbo].[PermissionList] AS TABLE (
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
	PRIMARY KEY ([HeaderIndex], [Index]),
	[Id]			INT,
	[View]			NVARCHAR (255),
	[Action]		NVARCHAR (255),
	[Criteria]		NVARCHAR(1024), -- compiles into SQL expression to filter the applicability
	[Mask]			NVARCHAR(1024),
	[Memo]			NVARCHAR (255)
);