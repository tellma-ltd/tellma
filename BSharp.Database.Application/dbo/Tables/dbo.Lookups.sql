CREATE TABLE [dbo].[Lookups] (
	[Id]				INT	PRIMARY KEY NONCLUSTERED IDENTITY,
	[DefinitionId]		NVARCHAR (255)		NOT NULL,	 -- TODO: Add foreign key to definitions table
	[Name]				NVARCHAR (255)		NOT NULL, -- appears in select lists
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[IsActive]			BIT					NOT NULL DEFAULT 1,
	[Code]				NVARCHAR(10), -- code for inter-tenant reporting
	[SortKey]			DECIMAL (9,4),	-- Sort code for reporting purposes
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceLookup1s__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceLookup1s__ModifiedById]  FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE CLUSTERED INDEX [IX_ResourceLookup1s__SortKey]
  ON [dbo].[Lookups]([SortKey])
