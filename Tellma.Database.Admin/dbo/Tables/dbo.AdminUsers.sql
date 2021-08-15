-- This is a user that is a member of the ADMIN database
CREATE TABLE [dbo].[AdminUsers]
(
	[Id]					INT PRIMARY KEY IDENTITY, 
    [Name]					NVARCHAR(255) NOT NULL,
    [ExternalId]			NVARCHAR(450) NULL, 
	[InvitedAt]				DATETIMEOFFSET(7),
	[State]					AS CAST(IIF([ExternalId] IS NOT NULL, 2, IIF([InvitedAt] IS NOT NULL, 1, 0)) AS TINYINT) PERSISTED, -- 2 = Member, 1 = Invited, 0 = New
	[IsService]				BIT					NOT NULL DEFAULT 0,
	[Email]					NVARCHAR (255),		-- Required when [IsService] = 0
	[ClientId]				NVARCHAR (255),		-- Required when [IsService] = 1
	[IsActive]				BIT NOT NULL DEFAULT 1,
	[LastAccess]			DATETIMEOFFSET(7),
	[PermissionsVersion]	UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	[UserSettingsVersion]	UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	[CreatedAt]				DATETIMEOFFSET(7) NOT NULL,
	[CreatedById]			INT NOT NULL,
	[ModifiedAt]			DATETIMEOFFSET(7) NOT NULL,
	[ModifiedById]			INT	NOT NULL,
    CONSTRAINT [FK_AdminUsers_AdminUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AdminUsers]([Id]),
    CONSTRAINT [FK_AdminUsers_AdminUsers_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [AdminUsers]([Id]),
		
	CONSTRAINT [CK_AdminUsers__EmailOrClientId]
    CHECK (
		([IsService] = 0 AND [Email] IS NOT NULL AND [ClientId] IS NULL) OR -- For service accounts: Email is forbidden and ClientId is required
		([IsService] = 1 AND [Email] IS NULL AND [ClientId] IS NOT NULL)	-- For human accounts: Email is required and Client is forbidden
	)
);
GO

-- Email is unique when not null
CREATE UNIQUE NONCLUSTERED INDEX [IX_AdminUsers__Email]
ON [dbo].[AdminUsers]([Email])
WHERE [Email] IS NOT NULL;
GO

-- ClientId is unique when not null
CREATE UNIQUE NONCLUSTERED INDEX [IX_AdminUsers__ClientId]
ON [dbo].[AdminUsers]([ClientId])
WHERE [ClientId] IS NOT NULL;
GO

