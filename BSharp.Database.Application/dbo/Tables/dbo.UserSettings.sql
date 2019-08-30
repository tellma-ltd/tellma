CREATE TABLE [dbo].[UserSettings]
(
	[UserId] INT, 
    [Key] NVARCHAR(255), 
    [Value] NVARCHAR(MAX) NOT NULL
	
	 CONSTRAINT [PK_UserSettings] PRIMARY KEY CLUSTERED 
	(
		[UserId] ASC,
		[Key] ASC
	)
)