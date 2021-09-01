CREATE TABLE [dbo].[SqlDatabases]
(
	[Id]				INT PRIMARY KEY CONSTRAINT [CK_SqlDatabases_Id] CHECK ([Id] > 0), 
    [DatabaseName]		NVARCHAR(255) NOT NULL, 
    [ServerId]			INT NOT NULL, 
    [Description]		NVARCHAR(1024) NULL, 
	[CreatedAt]		    DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	    INT	NOT NULL,
	[ModifiedAt]	    DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]	    INT	NOT NULL, 
    [AdopterId]         UNIQUEIDENTIFIER,
    CONSTRAINT [FK_SqlDatabases_AdminUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AdminUsers]([Id]),
    CONSTRAINT [FK_SqlDatabases_AdminUsers_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [AdminUsers]([Id]),
    CONSTRAINT [FK_SqlDatabases_SqlServers] FOREIGN KEY ([ServerId]) REFERENCES [SqlServers]([Id])
)
