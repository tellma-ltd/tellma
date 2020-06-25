CREATE TABLE [dbo].[AccountTypeCenterTypes]
(
	[Id]					INT					CONSTRAINT [PK_AccountTypeCenterTypes] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT					NOT NULL CONSTRAINT [FK_AccountTypeCenterTypes__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[CenterType]			NVARCHAR (50)		NOT NULL CONSTRAINT [CK_AccountTypeCenterTypes__CenterType] CHECK (
													[CenterType] IN (
														N'Segment', N'Abstract', N'Parent', N'CostOfSales',	N'SellingGeneralAndAdministration',
														N'SharedExpenseControl', N'TransitExpenseControl', N'ConstructionExpenseControl',
														N'ProductionExpenseControl'
													)
												),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeCenterTypes__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeCenterTypens__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);