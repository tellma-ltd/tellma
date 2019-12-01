CREATE TABLE [dbo].[UserSettings]
(
	[UserId]	INT, 
    [Key]		NVARCHAR(255),		
	CONSTRAINT [PK_UserSettings] PRIMARY KEY CLUSTERED ([UserId], [Key]),
	[Value]		NVARCHAR(MAX)		NOT NULL
)