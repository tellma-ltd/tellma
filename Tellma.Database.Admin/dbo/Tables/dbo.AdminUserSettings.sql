CREATE TABLE [dbo].[AdminUserSettings]
(
	[AdminUserId]	INT, 
    [Key]			NVARCHAR(255),		
	CONSTRAINT [PK_AdminUserSettings] PRIMARY KEY CLUSTERED ([AdminUserId], [Key]),
	[Value]			NVARCHAR(MAX)		NOT NULL
)
