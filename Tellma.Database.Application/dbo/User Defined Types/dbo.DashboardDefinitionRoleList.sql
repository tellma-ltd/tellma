CREATE TYPE [dbo].[DashboardDefinitionRoleList] AS TABLE
(
	[Index]								INT DEFAULT 0,
	[HeaderIndex]						INT DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]								INT	NOT NULL DEFAULT 0,

	[RoleId]							INT
)
