CREATE TABLE [dbo].[AccountsResponsibilityCenters]
(
	[AccountId]					INT					CONSTRAINT [FK_AccountsResponsibilityCenters__AccountId] FOREIGN KEY ([AccountId]) REFERENCES dbo.[GLAccounts]([Id]),
	[ResponsibilityCenterId]	INT					CONSTRAINT [FK_AccountsResponsibilityCenters__ResponsibilityCenterId] FOREIGN KEY ([ResponsibilityCenterId]) REFERENCES dbo.[ResponsibilityCenters]([Id]) ON UPDATE CASCADE,
	[IsActive]					BIT					NOT NULL DEFAULT 1
	CONSTRAINT [PK_AccountsResponsibilityCenters]	PRIMARY KEY ([AccountId], [ResponsibilityCenterId])
)
