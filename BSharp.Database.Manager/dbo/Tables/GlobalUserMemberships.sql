CREATE TABLE [dbo].[GlobalUserMemberships]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [UserId] INT NOT NULL, 
    [DatabaseId] INT NOT NULL, 
    CONSTRAINT [FK_GlobalUserMemberships_GlobalUsers] FOREIGN KEY ([UserId]) REFERENCES [GlobalUsers]([Id]),
    CONSTRAINT [FK_GlobalUserMemberships_SqlDatabases] FOREIGN KEY ([DatabaseId]) REFERENCES [SqlDatabases]([Id])
)
