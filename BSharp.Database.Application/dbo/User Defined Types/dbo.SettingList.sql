CREATE TYPE [dbo].[SettingList] AS TABLE (
	[Index]					INT		PRIMARY KEY			IDENTITY,
	[FunctionalCurrency]	NCHAR (3),
	[ArchiveDate]			Datetime2,
	[TenantLanguage2]		NVARCHAR (255),
	[TenantLanguage3]		NVARCHAR (255)
);