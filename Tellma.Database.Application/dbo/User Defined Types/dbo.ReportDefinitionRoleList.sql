CREATE TYPE [dbo].[ReportDefinitionRoleList] AS TABLE
(
	[Index]								INT DEFAULT 0,
	[HeaderIndex]						INT DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]								INT NOT NULL,

	[RoleId]							INT	NOT NULL
)
