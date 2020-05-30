CREATE TABLE [dbo].[AccountTypeResourceDefinitions]
(
	[Id]					INT CONSTRAINT [PK_AccountTypeResourceDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeResourceDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[ResourceDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeResourceDefinitions__ResourceDefinitionId] REFERENCES dbo.ResourceDefinitions([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeResourceDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeResourceDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);