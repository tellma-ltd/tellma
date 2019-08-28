CREATE TABLE [dbo].[SqlServers]
(
	[Id] INT PRIMARY KEY IDENTITY,
	[ServerName] NVARCHAR (255) NOT NULL,
	[UserName] NVARCHAR (255) NOT NULL, 
	-- For enhanced security we don't store the password itself in the database,
	-- but a key to look the password up from a configuration provider, when
	-- this is left as null, it is assumed to be the same password as the admin DB
    [PasswordKey] NVARCHAR(255) NULL, 
    [Description] NVARCHAR(1024) NULL,
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')), 
    CONSTRAINT [FK_SqlServers_GlobalUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [GlobalUsers]([Id]),
    CONSTRAINT [FK_SqlServers_GlobalUsers_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [GlobalUsers]([Id]),
)
