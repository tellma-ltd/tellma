CREATE TABLE [dbo].[IdentityServerClients]
(
	[Id] INT PRIMARY KEY IDENTITY, 
	[Name] NVARCHAR(255) NOT NULL,
	[Memo] NVARCHAR (1024),
	[ClientId] NVARCHAR(35) NOT NULL UNIQUE,
	[ClientSecret] NVARCHAR(255) NOT NULL,
	[CreatedAt]	DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT NOT NULL CONSTRAINT [FK_IdentityServerClients__CreatedById] REFERENCES [dbo].[AdminUsers] ([Id]),
	[ModifiedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById] INT NOT NULL CONSTRAINT [FK_IdentityServerClients__ModifiedById] REFERENCES [dbo].[AdminUsers] ([Id])
)
