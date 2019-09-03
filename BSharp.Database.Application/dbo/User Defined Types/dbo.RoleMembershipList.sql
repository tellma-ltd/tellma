CREATE TYPE [dbo].[RoleMembershipList] AS TABLE
(
	[Index]		INT,
	[HeaderIndex] INT,
	[Id]		INT NOT NULL DEFAULT 0,
	[AgentId]	INT NULL,
	[RoleId]	INT NULL,
	[Memo]		NVARCHAR(255) NULL,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex] ASC)
)
