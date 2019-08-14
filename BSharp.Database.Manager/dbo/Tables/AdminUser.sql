-- This is a user that is a member of the ADMIN database
CREATE TABLE [dbo].[AdminUsers]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(255) NOT NULL,
    [ExternalId] NVARCHAR(450) NULL, 
    [Email] NVARCHAR(255) NOT NULL,
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')), 
    CONSTRAINT [FK_AdminUsers_AdminUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AdminUsers]([Id]),
    CONSTRAINT [FK_AdminUsers_AdminUsers_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [AdminUsers]([Id])

)
