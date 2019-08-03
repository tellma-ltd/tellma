CREATE TYPE [dbo].[SettingList] AS TABLE (
	[Index]					INT				IDENTITY(0, 1),
	[FunctionalCurrencyId]	INT,
	[ArchiveDate]			Datetime2,
	[TenantLanguage2]		NVARCHAR (255),
	[TenantLanguage3]		NVARCHAR (255),
	[EntityState]			NVARCHAR (255)	NOT NULL DEFAULT(N'Inserted'),
	PRIMARY KEY ([Index]),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);