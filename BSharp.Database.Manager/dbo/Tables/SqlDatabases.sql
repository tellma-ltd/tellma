CREATE TABLE [dbo].[SqlDatabases]
(
	[Id]				INT NOT NULL PRIMARY KEY, 
    [DatabaseName]		NVARCHAR(255) NOT NULL, 
    [ServerId]			INT NOT NULL, 
    [Description]		NVARCHAR(1024) NULL, 
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')), 
    CONSTRAINT [FK_SqlDatabases_GlobalUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [GlobalUsers]([Id]),
    CONSTRAINT [FK_SqlDatabases_GlobalUsers_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [GlobalUsers]([Id]),
    CONSTRAINT [FK_SqlDatabases_SqlServers] FOREIGN KEY ([ServerId]) REFERENCES [SqlServers]([Id])
)
