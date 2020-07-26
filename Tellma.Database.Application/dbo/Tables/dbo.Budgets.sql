CREATE TABLE [dbo].[Budgets]
(
	[Id]					INT				NOT NULL CONSTRAINT [PK_Budgets] PRIMARY KEY NONCLUSTERED IDENTITY ,
	[Code]					NVARCHAR (50),
	[AccountId]				INT				NOT NULL CONSTRAINT [FK_Budgets__AccountId] REFERENCES [dbo].[Accounts] ([Id]),
	[CenterId]				INT				NOT NULL CONSTRAINT [FK_Budgets__CenterId] REFERENCES [dbo].[Centers] ([Id]),
	[ResourceId]			INT				CONSTRAINT [FK_Budgets__ResourceId] REFERENCES [dbo].[Resources] ([Id]),
	[CustodianId]			INT				CONSTRAINT [FK_Budgets__CustodianId] REFERENCES [dbo].[Relations] ([Id]),
	[CurrencyId]			NCHAR (3)		CONSTRAINT [FK_Budgets__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	[EntryTypeId]			INT				NOT NULL CONSTRAINT [FK_Budgets__EntryTypeId] REFERENCES [dbo].[EntryTypes]
);
GO
CREATE CLUSTERED INDEX [IX_Budgets__Code] ON dbo.Budgets([Code]) --WHERE [Code] IS NOT NULL;
GO