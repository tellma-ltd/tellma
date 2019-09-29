CREATE TABLE [dbo].[AccountsLocations]
(
	[AccountId]		INT					CONSTRAINT [FK_AccountsLocations__AccountId] FOREIGN KEY ([AccountId]) REFERENCES dbo.[AccountClassifications]([Id]),
	[LocationId]	INT					CONSTRAINT [FK_AccountsLocations__LocationId] FOREIGN KEY ([LocationId]) REFERENCES dbo.[Locations]([Id]) ON UPDATE CASCADE,
	[IsActive]		BIT					NOT NULL DEFAULT 1
	CONSTRAINT [PK_AccountsLocations]	PRIMARY KEY ([AccountId], [LocationId])
);