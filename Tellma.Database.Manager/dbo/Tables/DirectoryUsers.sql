-- This table is used to efficiently query the companies that I am a member of, any user defined anywhere will have a corresponding DirectoryUser defined here
CREATE TABLE [dbo].[DirectoryUsers]
(
	[Id] INT PRIMARY KEY IDENTITY, 
    [ExternalId] NVARCHAR(450) NULL, 
    [Email] NVARCHAR(255) NOT NULL,
    [IsAdmin] BIT NOT NULL DEFAULT 0
)

GO

CREATE UNIQUE INDEX [IX_DirectoryUsers_Email] ON [dbo].[DirectoryUsers] ([Email])

GO

CREATE UNIQUE INDEX [IX_DirectoryUsers_ExternalId] ON [dbo].[DirectoryUsers] ([ExternalId]) WHERE [ExternalId] IS NOT NULL;
