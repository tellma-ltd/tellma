CREATE TABLE [dbo].[ContractUsers] (
	[Id]				INT					CONSTRAINT [PK_ContractUsers] PRIMARY KEY IDENTITY,
	[ContractId]		INT					NOT NULL CONSTRAINT [FK_ContractUsers__ContractId] REFERENCES dbo.[Contracts]([Id]) ON DELETE CASCADE,
	[UserId]			INT					NOT NULL CONSTRAINT [FK_ContractUsers__UserId] REFERENCES dbo.Users([Id]),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ContractUsers__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ContractUsers__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);