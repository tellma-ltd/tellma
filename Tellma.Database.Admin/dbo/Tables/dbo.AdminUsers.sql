-- This is a user that is a member of the ADMIN database
CREATE TABLE [dbo].[AdminUsers]
(
	[Id]					INT PRIMARY KEY IDENTITY, 
    [Name]					NVARCHAR(255) NOT NULL,
    [ExternalId]			NVARCHAR(450) NULL, 
	[InvitedAt]				DATETIMEOFFSET(7),
	[State]					AS CAST(IIF([ExternalId] IS NOT NULL, 2, IIF([InvitedAt] IS NOT NULL, 1, 0)) AS TINYINT) PERSISTED, -- 2 = Member, 1 = Invited, 0 = New
    [Email]					NVARCHAR(255) NOT NULL,
	[IsActive]				BIT NOT NULL DEFAULT 1,
	[LastAccess]			DATETIMEOFFSET(7),
	[PermissionsVersion]	UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	[UserSettingsVersion]	UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	[CreatedAt]				DATETIMEOFFSET(7) NOT NULL,
	[CreatedById]			INT NOT NULL,
	[ModifiedAt]			DATETIMEOFFSET(7) NOT NULL,
	[ModifiedById]			INT	NOT NULL,
    CONSTRAINT [FK_AdminUsers_AdminUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AdminUsers]([Id]),
    CONSTRAINT [FK_AdminUsers_AdminUsers_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [AdminUsers]([Id])
)
