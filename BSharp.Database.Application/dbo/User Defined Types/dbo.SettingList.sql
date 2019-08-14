CREATE TYPE [dbo].[SettingList] AS TABLE (
	[Index]					INT		PRIMARY KEY			IDENTITY(0, 1),
	[FunctionalCurrencyId]	INT,
	[ArchiveDate]			Datetime2,
	[TenantLanguage2]		NVARCHAR (255),
	[TenantLanguage3]		NVARCHAR (255)
);