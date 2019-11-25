CREATE TABLE [dbo].[TaxAccounts]
(
	[TaxCode] NVARCHAR (30) NOT NULL PRIMARY KEY,
	[AccountId] INT REFERENCES dbo.Accounts([Id]),
	[AccountDefinitionId]	NVARCHAR (50) NOT NULL,
	CONSTRAINT [FK_TaxAccounts__AccountTypeAccountId] FOREIGN KEY ([AccountId], [AccountDefinitionId]) REFERENCES dbo.Accounts([Id], [AccountGroupId])
)
