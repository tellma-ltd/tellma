CREATE TABLE [dbo].[IfrsNotes] (
	[Id]				NVARCHAR (255) PRIMARY KEY NONCLUSTERED , -- Ifrs Concept
	[ParentId]			NVARCHAR (255),
	[IsAggregate]		BIT					NOT NULL DEFAULT 1,
-- ForDebit and ForCredit might be concluded from IfrsAccountIfrsNotes table
--	If [ForDebit] = 1, Note can be used with Debit entries
	[ForDebit]			BIT					NOT NULL DEFAULT 1,
--	If [ForCredit] = 1, Note can be used with Credit entries
	[ForCredit]			BIT					NOT NULL DEFAULT 1,
	[Node]				HIERARCHYID,
	[ParentNode]		AS [Node].GetAncestor(1),
	CONSTRAINT [CK_IfrsNotes__ForDebit_ForCredit] CHECK ([ForDebit] = 1 OR [ForCredit] = 1),
	CONSTRAINT [FK_IfrsNotes__IfrsConcepts]	FOREIGN KEY ([Id])	REFERENCES [dbo].[IfrsConcepts] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
	);
GO
CREATE UNIQUE CLUSTERED INDEX IfrsNotes__Node
ON [dbo].[IfrsNotes]([Node]) ;  
GO