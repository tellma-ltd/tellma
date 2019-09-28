CREATE TABLE [dbo].[AccountsResources]
(
	[AccountId]		INT					CONSTRAINT [FK_AccountsResources__AccountId] FOREIGN KEY ([AccountId]) REFERENCES dbo.[GLAccounts]([Id]),
	[ResourceId]	INT					CONSTRAINT [FK_AccountsResources__ResourceId] FOREIGN KEY ([ResourceId]) REFERENCES dbo.[Resources]([Id]) ON UPDATE CASCADE,
	[IsActive]		BIT					NOT NULL DEFAULT 1
	CONSTRAINT [PK_AccountsResources]	PRIMARY KEY ([AccountId], [ResourceId])
);