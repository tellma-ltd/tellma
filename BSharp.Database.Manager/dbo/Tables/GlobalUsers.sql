-- This table is used to efficiently query the companies that I am a member of, any user defined anywhere will have a corresponding GlobalUser defined here
CREATE TABLE [dbo].[GlobalUsers]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [ExternalId] NVARCHAR(450) NULL, 
    [Email] NVARCHAR(255) NOT NULL,
)

GO

CREATE UNIQUE INDEX [IX_GlobalUsers_Email] ON [dbo].[GlobalUsers] ([Email])

GO

CREATE UNIQUE INDEX [IX_GlobalUsers_ExternalId] ON [dbo].[GlobalUsers] ([ExternalId]) WHERE [ExternalId] IS NOT NULL;
