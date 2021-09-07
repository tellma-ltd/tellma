CREATE TABLE [dbo].[DataProtectionKeys]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[FriendlyName] NVARCHAR(MAX),
	[Xml] NVARCHAR(MAX)
)
