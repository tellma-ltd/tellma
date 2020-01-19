CREATE TABLE [dbo].[EntryTypes] ( -- inspired by IFRS concepts. However, its main purpose is to facilitate smart posting and reporting
	[Id]					INT					CONSTRAINT [PK_EntryTypes]  PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]				INT					CONSTRAINT [FK_EntryTypes__ParentId] REFERENCES [dbo].[EntryTypes] ([Id]),
	[Code]					NVARCHAR (255)		UNIQUE NONCLUSTERED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Node]					HIERARCHYID			NOT NULL,
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[ForDebit]				BIT					NOT NULL DEFAULT 1,
	[ForCredit]				BIT					NOT NULL DEFAULT 1,
	[IsSystem]				BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	
	[ParentNode]			AS [Node].GetAncestor(1),
);
GO
CREATE UNIQUE CLUSTERED INDEX [IX_EntryTypes__Node]
	ON [dbo].[EntryTypes]([Node]);
GO