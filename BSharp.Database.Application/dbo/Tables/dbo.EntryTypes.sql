CREATE TABLE [dbo].[EntryTypes] ( -- inspired by IFRS concepts. However, its main purpose is to facilitate smart posting and reporting
	[Id]					NVARCHAR (255)		PRIMARY KEY NONCLUSTERED,
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[ForDebit]				BIT					NOT NULL DEFAULT 1,
	[ForCredit]				BIT					NOT NULL DEFAULT 1,
	[Node]					HIERARCHYID			NOT NULL,
	[ParentNode]			AS [Node].GetAncestor(1),
);
GO
CREATE UNIQUE CLUSTERED INDEX [IX_EntryTypes__Node]
	ON [dbo].[EntryTypes]([Node]);
GO