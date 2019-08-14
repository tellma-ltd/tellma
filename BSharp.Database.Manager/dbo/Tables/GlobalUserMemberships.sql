CREATE TABLE [dbo].[GlobalUserMemberships]
(
    [UserId] INT NOT NULL, 
    [DatabaseId] INT NOT NULL, 
	PRIMARY KEY ([UserId], [DatabaseId]),
    CONSTRAINT [FK_GlobalUserMemberships_GlobalUsers] FOREIGN KEY ([UserId]) REFERENCES [GlobalUsers]([Id]),
    CONSTRAINT [FK_GlobalUserMemberships_SqlDatabases] FOREIGN KEY ([DatabaseId]) REFERENCES [SqlDatabases]([Id])
)
