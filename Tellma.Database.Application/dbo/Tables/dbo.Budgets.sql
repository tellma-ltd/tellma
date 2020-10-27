CREATE TABLE [dbo].[Budgets]
(
	[Id]					INT				NOT NULL CONSTRAINT [PK_Budgets] PRIMARY KEY NONCLUSTERED IDENTITY ,
	[Code]					NVARCHAR (50),
	[AccountId]				INT				NOT NULL CONSTRAINT [FK_Budgets__AccountId] REFERENCES [dbo].[Accounts] ([Id]),
	[CurrencyId]			NCHAR (3)		NOT NULL CONSTRAINT [FK_Budgets__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	[CustodianId]			INT				CONSTRAINT [FK_Budgets_CustodianId] REFERENCES dbo.[Relations] ([Id]),
	[CustodyId]				INT				CONSTRAINT [FK_Budgets__CustodyId] REFERENCES dbo.[Custodies]([Id]),
	[ParticipantId]			INT				CONSTRAINT [FK_Budgets__PerticipantId] REFERENCES dbo.[Relations] ([Id]),
	[ResourceId]			INT				CONSTRAINT [FK_Budgets__ResourceId] REFERENCES dbo.[Resources]([Id]),
	[CenterId]				INT				NOT NULL CONSTRAINT [FK_Budgets__CentertId] REFERENCES dbo.[Centers]([Id]),
	[EntryTypeId]			INT				NOT NULL CONSTRAINT [FK_Budgets__EntryTypeId] REFERENCES [dbo].[EntryTypes]
);
GO
CREATE CLUSTERED INDEX [IX_Budgets__Code] ON dbo.Budgets([Code]) --WHERE [Code] IS NOT NULL;
GO