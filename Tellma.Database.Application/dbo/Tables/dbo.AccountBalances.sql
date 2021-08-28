CREATE TABLE [dbo].[AccountBalances]
-- Note: It allows all mixes and matches. For example, we may want to put a limit on the total paper weight
-- in a shipment, regardless of paper type etc. We set the following
-- Account: GIT, Contract: Specific shipment, Resource: Null, Currency: Null, 
-- Unit Id: Ton.
(
	[Id]					INT CONSTRAINT [PK_AccountBalances] PRIMARY KEY IDENTITY,
	[AccountId]				INT	NOT NULL CONSTRAINT [FK_AccountBalances__AccountId] REFERENCES dbo.Accounts([Id]) ON DELETE CASCADE,
	[CenterId]				INT CONSTRAINT [FK_AccountBalances__CenterId] REFERENCES dbo.Centers([Id]),
	[AgentId]			INT CONSTRAINT [FK_AccountBalances__AgentId] REFERENCES dbo.[Agents]([Id]),
	[ResourceId]			INT CONSTRAINT [FK_AccountBalances__ResourceId] REFERENCES dbo.Resources([Id]),
	[CurrencyId]			NCHAR (3) CONSTRAINT [FK_AccountBalances__CurrencyId] REFERENCES dbo.Currencies([Id]),
	CONSTRAINT [UX_AccountBalances] UNIQUE([AccountId], [CenterId], [AgentId], [ResourceId], [CurrencyId]),
	[BalanceEnforcedState]	TINYINT NOT NULL DEFAULT 6, -- 6: not enforced, 5: only closed documents count
	-- The following two can be made updateable through events as well.
	[MinQuantity]			DECIMAL DEFAULT -999999999999, -- residual asset value
	[MaxQuantity]			DECIMAL DEFAULT +999999999999, -- max store capacity
	CONSTRAINT [CK_AccountBalances__MinQuantity_MaxQuantity] CHECK([MinQuantity] <= [MaxQuantity]),
	[UnitId]				INT CONSTRAINT [FK_AccountBalances__UnitId] REFERENCES dbo.Units([Id]),
	[MinMonetaryBalance]	DECIMAL DEFAULT -999999999999, -- supplier credit line, max accepted customer advances
	[MaxMonetaryBalance]	DECIMAL DEFAULT +999999999999, -- customer credit line, max allowed supplier prepayment, max allowed employee loan
	CONSTRAINT [CK_AccountBalances__MinMonetaryBalance_MaxMonetaryBalance] CHECK([MinMonetaryBalance] <= [MaxMonetaryBalance])
);
GO