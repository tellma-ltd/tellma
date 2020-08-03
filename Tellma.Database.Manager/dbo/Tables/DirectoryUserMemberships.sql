CREATE TABLE [dbo].[DirectoryUserMemberships]
(
    [UserId] INT NOT NULL, 
    [DatabaseId] INT NOT NULL, 
	PRIMARY KEY ([UserId], [DatabaseId]),
    CONSTRAINT [FK_DirectoryUserMemberships_DirectoryUsers] FOREIGN KEY ([UserId]) REFERENCES [DirectoryUsers]([Id]),
    CONSTRAINT [FK_DirectoryUserMemberships_SqlDatabases] FOREIGN KEY ([DatabaseId]) REFERENCES [SqlDatabases]([Id]) ON UPDATE CASCADE ON DELETE CASCADE
)
