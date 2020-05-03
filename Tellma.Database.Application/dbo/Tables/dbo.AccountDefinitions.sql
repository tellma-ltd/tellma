CREATE TABLE [dbo].[AccountDefinitions]
(
	[Id]				INT				PRIMARY KEY,-- IDENTITY,
	[ShowOCE]			BIT				NOT NULL DEFAULT 0,
	[IsCenterMapped]	BIT				NOT NULL DEFAULT 0,
	[IsCurrencyMapped]	BIT				NOT NULL DEFAULT 0,
	[IsContractMapped]	BIT				NOT NULL DEFAULT 0,
	[IsResourceMapped]	BIT				NOT NULL DEFAULT 0,
	[IsEntryTypeMapped]	BIT				NOT NULL DEFAULT 0,
	[Code]				NVARCHAR (50)	NOT NULL, -- Kebab case
	[Name]				NVARCHAR (50)	NOT NULL,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[EntryTypeParentId]	INT CONSTRAINT [FK_AccountDefinitions__EntryTypeParentId] REFERENCES dbo.EntryTypes([Id])
);