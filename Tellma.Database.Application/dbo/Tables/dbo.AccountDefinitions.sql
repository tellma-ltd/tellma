CREATE TABLE [dbo].[AccountDefinitions]
(
	[Id]			INT				PRIMARY KEY IDENTITY,
	[Code]			NVARCHAR (50)	NOT NULL, -- Kebab case
	[Name]			NVARCHAR (50)	NOT NULL,
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	-- filters what can appear in Accounts or in Entries
	[EntryTypeParentId] INT
);