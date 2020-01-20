CREATE TABLE [dbo].[AccountTypes] (
	[Id]					INT					CONSTRAINT [PK_AccountTypes]  PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]				INT					CONSTRAINT [FK_AccountTypes__ParentId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[Code]					NVARCHAR (255)		NOT NULL CONSTRAINT [IX_AccountTypes__Code] UNIQUE NONCLUSTERED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Description]			NVARCHAR (1024),
	[Description2]			NVARCHAR (1024),
	[Description3]			NVARCHAR (1024),
	[Node]					HIERARCHYID			NOT NULL CONSTRAINT [IX_AccountTypes__Node] UNIQUE CLUSTERED,
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[IsCurrent]				BIT,
	[IsReal]				BIT					NOT NULL DEFAULT 0,
	[IsResourceClassification]BIT				NOT NULL DEFAULT 0,
	[IsPersonal]			BIT					NOT NULL DEFAULT 0,
	[EntryTypeParentId]		INT					CONSTRAINT [FK_AccountTypes__EntryTypeParentId] REFERENCES [dbo].[EntryTypes] ([Id]),	
	-- Additional properties, Is Active at the end
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[IsSystem]				BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	-- Pure SQL properties and computed properties
	[ParentNode]			AS [Node].GetAncestor(1)
);
GO