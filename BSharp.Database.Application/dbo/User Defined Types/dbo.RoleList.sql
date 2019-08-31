﻿CREATE TYPE [dbo].[RoleList] AS TABLE (
	[Index]				INT					PRIMARY KEY	IDENTITY(0, 1),
	[Id]				INT					NOT NULL DEFAULT 0,
	[Name]				NVARCHAR (255)		NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[IsPublic]			BIT					NOT NULL DEFAULT 0,		
	[Code]				NVARCHAR (255)
);