CREATE TABLE [dbo].[DocumentLines] (
--	These are for transactions only. If there are Lines from requests or inquiries, etc=> other tables
	[Id]					INT					PRIMARY KEY IDENTITY,
	[DocumentId]			INT					NOT NULL CONSTRAINT [FK_DocumentLines__DocumentId]	FOREIGN KEY ([DocumentId])	REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[LineDefinitionId]		NVARCHAR (50)		NOT NULL CONSTRAINT [FK_DocumentLines__LineDefinitionId]	FOREIGN KEY ([LineDefinitionId])	REFERENCES [dbo].[LineDefinitions] ([Id]),
	[TemplateLineId]		INT, -- depending on the line type, the user may/may not be allowed to edit
	[ScalingFactor]			FLOAT, -- Qty sold for Price list, Qty produced for BOM
	[AgentId]				INT, -- useful for storing the conversion agent in conversion transactions
-- Additional information to satisfy reporting requirements
	[Memo]						NVARCHAR (255), -- a textual description for statements and reports
-- While Voucher Number referes to the source document, this refers to any other identifying string 
-- for support documents, such as deposit slip reference, invoice number, etc...
	[ExternalReference]			NVARCHAR (255),
-- The following are sort of dynamic properties that capture information for reporting purposes
	[AdditionalReference]		NVARCHAR (255),
-- for debiting asset accounts, related resource is the good/service acquired from supplier/customer/storage
-- for crediting asset accounts, related resource is the good/service delivered to supplier/customer/storage as resource
-- for debiting VAT purchase account, related resource is the good/service purchased
-- for crediting VAT Sales account, related resource is the good/service sold
-- for crediting VAT purchase, debiting VAT sales, or liability account: related resource is N/A
	[RelatedResourceId]			INT, -- Good, Service, Labor, Machine usage
-- The related account is the implicit  account that had two entries, one debiting and one crediting and hence removed
-- Examples include supplier account in cash purchase, customer account in cash sales, employee account in cash payroll, 
-- supplier account in VAT purchase entry, customer account in VAT sales entry, and WIP account in direct production.
	[RelatedAccountId]			INT,
	[RelatedQuantity]			MONEY ,		-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMoneyAmount]		MONEY 			NOT NULL DEFAULT 0, -- e.g., amount subject to tax
	[SortKey]				DECIMAL (9,4)		NOT NULL,
-- for auditing
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET() CONSTRAINT [FK_DocumentLines__CreatedById]	FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	[CreatedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLines__ModifiedById]	FOREIGN KEY ([ModifiedById])REFERENCES [dbo].[Users] ([Id]),
);
GO
CREATE INDEX [IX_DocumentLines__DocumentId] ON [dbo].[DocumentLines]([DocumentId]);
GO