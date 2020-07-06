CREATE TABLE [dbo].[EntryTypes] ( -- inspired by IFRS concepts. However, its main purpose is to facilitate smart posting and reporting
	[Id]					INT					CONSTRAINT [PK_EntryTypes]  PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]				INT					CONSTRAINT [FK_EntryTypes__ParentId] REFERENCES [dbo].[EntryTypes] ([Id]),
	[Code]					NVARCHAR (50)		NOT NULL UNIQUE NONCLUSTERED, -- 50
	[Concept]				NVARCHAR (255)		NOT NULL UNIQUE NONCLUSTERED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Description]			NVARCHAR (1024),
	[Description2]			NVARCHAR (1024),
	[Description3]			NVARCHAR (1024),
	[Node]					HIERARCHYID			NOT NULL CONSTRAINT [UX_EntryTypes__Node] UNIQUE CLUSTERED,
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[IsSystem]				BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	
	[ParentNode]			AS [Node].GetAncestor(1) PERSISTED,
	[Level]						AS [Node].GetLevel() PERSISTED,
	[Path]						AS [Node].ToString() PERSISTED 
);
GO