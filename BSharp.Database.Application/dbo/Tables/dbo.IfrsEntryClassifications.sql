CREATE TABLE [dbo].[IfrsEntryClassifications] (
	-- To generate financial statements: Cash flow - direct, Changes of Equity
	-- However, it can also be used to generate several other notes as well such as PPE, Intangible, Biological, Expenses by function
	[Id]						NVARCHAR (255)		PRIMARY KEY NONCLUSTERED CONSTRAINT [FK_IfrsEntryClassifications__IfrsConcepts]	FOREIGN KEY ([Id])	REFERENCES [dbo].[IfrsConcepts] ([Id]),
	[Node]						HIERARCHYID			NOT NULL CONSTRAINT [CK_IfrsEntryClassifications__Node] UNIQUE INDEX [IX_IfrsEntryClassifications__Node] CLUSTERED,
	[ParentNode]				AS [Node].GetAncestor(1),
	[IsActive]					BIT					NOT NULL DEFAULT 1, -- update to 0 those who do appear as ancestors
	[IsLeaf]					BIT					NOT NULL DEFAULT 1, -- update to 0 those who do appear as ancestors
	-- helpful for aggregating all children into parent, or some into catch all "Other"
	[StatementClassificationId]			NVARCHAR (255)		NULL,

	[Label]						NVARCHAR (1024)		NOT NULL,
	[Label2]					NVARCHAR (1024),
	[Label3]					NVARCHAR (1024),
	[Documentation]				NVARCHAR (MAX),
	[Documentation2]			NVARCHAR (MAX),
	[Documentation3]			NVARCHAR (MAX),
	[EffectiveDate]				DATETIME2(7)		NOT NULL DEFAULT('0001-01-01 00:00:00'),
	[ExpiryDate]				DATETIME2(7)		NOT NULL DEFAULT('9999-12-31 23:59:59'),

--	If [ForDebit] = 1, Note can be used with Debit entries, i.e. Direction = -1, even if amount was negative.
	[ForDebit]			BIT					NOT NULL DEFAULT 1,
--	If [ForCredit] = 1, Note can be used with Credit entries
	[ForCredit]			BIT					NOT NULL DEFAULT 1,

	CONSTRAINT [CK_IfrsEntryClassifications__ForDebit_ForCredit] CHECK ([ForDebit] = 1 OR [ForCredit] = 1),
	);
GO