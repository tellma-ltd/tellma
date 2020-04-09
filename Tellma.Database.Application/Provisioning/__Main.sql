BEGIN -- Setup Configuration
	DECLARE @DeployEmail NVARCHAR(255)				= '$(DeployEmail)';-- N'admin@tellma.com';
	DECLARE @ShortCompanyName NVARCHAR(255)			= '$(ShortCompanyName)'; --N'ACME International';
	DECLARE @ShortCompanyName2 NVARCHAR(255);
	DECLARE @ShortCompanyName3 NVARCHAR(255);
	DECLARE @PrimaryLanguageId NVARCHAR(255)		= '$(PrimaryLanguageId)'; --N'en';
	DECLARE @SecondaryLanguageId NVARCHAR(255)		= '$(SecondaryLanguageId)'; --N'en';
	DECLARE @TernaryLanguageId NVARCHAR(255)		= '$(TernaryLanguageId)'; --N'en';
	DECLARE @FunctionalCurrencyId NCHAR(3)			= '$(FunctionalCurrency)'; --N'ETB'
	DECLARE @ProvisionData NVARCHAR(255)			= '$(ProvisionData)'; -- 1 or 0
	DECLARE @DefinitionsVersion UNIQUEIDENTIFIER	= NEWID();
	DECLARE @SettingsVersion UNIQUEIDENTIFIER		= NEWID();
	-- Because there is no way to pass the NULL value to 
	IF @SecondaryLanguageId = N'NULL' SET @SecondaryLanguageId = NULL;
	IF @TernaryLanguageId = N'NULL' SET @TernaryLanguageId = NULL;
END
--IF @ProvisionData = 0 RETURN;
:r .\00_Common\__Declarations.sql
:r .\00_Common\a_AdminUser.sql
:r .\00_Common\b_FunctionalCurrency.sql
:r .\00_Common\c_Settings.sql

:r .\00_Common\d_EntryTypes.sql

:r .\02_Definitions\a_LookupDefinitions.sql
:r .\02_Definitions\b_ResourceDefinitions.sql
:r .\02_Definitions\c_AgentDefinitions.sql

:r .\00_Common\e_AccountTypes.sql
:r .\00_Common\f_RuleTypes.sql

:r .\01_Security\a_Users.sql
:r .\01_Security\b_RolesMemberships.sql




--:r .\01_Definitions\e_LineDefinitions\100_LineDefinitions.sql
:r .\02_Definitions\e_LineDefinitions\101_LineDefinitions.sql
--:r .\01_Definitions\e_LineDefinitions\102_LineDefinitions.sql
--:r .\01_Definitions\e_LineDefinitions\103_LineDefinitions.sql
--:r .\01_Definitions\e_LineDefinitions\104_LineDefinitions.sql
--:r .\01_Definitions\e_LineDefinitions\105_LineDefinitions.sql
:r .\02_Definitions\e_LineDefinitions\999_LineDefinitions.sql
:r .\02_Definitions\f_DocumentDefinitions.sql

:r .\03_Basic\a_Currencies.sql
:r .\03_Basic\b_Units.sql
:r .\03_Basic\c_Lookups.sql

:r .\04_Resources\101_property-plant-and-equipment.sql
:r .\04_Resources\101_employee-benefits.sql
:r .\04_Resources\101_services-expenses.sql
--:r .\04_Resources\102_employee-benefits.sql
--:r .\04_Resources\102_property-plant-and-equipment.sql
--:r .\04_Resources\104_finished_goods.sql
--:r .\04_Resources\104_raw-materials.sql
--:r .\04_Resources\105_merchandise.sql

--:r .\04_Resources\a1_PPE_motor-vehicles.sql
--:r .\04_Resources\a3_PPE_machineries.sql

--:r .\04_Resources\d1_FG_vehicles.sql
----:r .\04_Resources\e1_CCE_received-checks.sql

:r .\05_Agents\02_Creditors.sql
:r .\05_Agents\03_Customers.sql
:r .\05_Agents\04_Debtors.sql
:r .\05_Agents\05_Partners.sql
:r .\05_Agents\06_Suppliers.sql
:r .\05_Agents\09_Custodians.sql
:r .\05_Agents\09a_Warehouses.sql
:r .\05_Agents\10_Employees.sql
:r .\05_Agents\00_Centers.sql

:r .\06_Accounts\b_LegacyClassifications.sql
:r .\06_Accounts\101_Accounts.sql

--:r .\06_Accounts\105_Accounts.sql
--:r .\07_Entries\101\101a_manual-journal-vouchers.sql
--:r .\07_Entries\101\101b_cash-payment-vouchers.sql
--:r .\07_Entries\101\101d_revenue-recognition-vouchers.sql

DELETE FROM dbo.ReportDefinitions WHERE [Id] IN (
	N'0c46cb52-739f-4308-82dd-7cd578bb04ff',
	N'281dba1b-7e3d-4497-b396-877ba91087c8',
	N'5aeec2a2-3008-4c62-8559-16896c17cc3f',
	N'6c7ba5e1-4f2d-4882-829e-406d71137ad4',
	N'aa5c998a-bc0b-49f1-8e03-80775cc4c15a',
	N'30d3f1d2-d168-4414-a933-305e99a71269',
	N'9ce0a0e3-772d-406a-8aef-46684b757eac'
); -- ON DELETE CASCADE

INSERT INTO dbo.ReportDefinitions([Id], [Title], [Type], [Collection], [Filter], [OrderBy], ShowColumnsTotal, ShowRowsTotal,ShowInMainMenu) VALUES
(N'0c46cb52-739f-4308-82dd-7cd578bb04ff',N'Statement of comprehensive income',N'Summary',N'DetailsEntry',N'Line/Document/PostingDate >= @fromDate and Line/Document/PostingDate <= @toDate and Account/AccountType/Node DescOf 121',NULL,0,1,0),
(N'281dba1b-7e3d-4497-b396-877ba91087c8',N'Trial Balance - Currency',N'Summary',N'DetailsEntry',N'CurrencyId = @Currency',NULL,0,1,0),
(N'5aeec2a2-3008-4c62-8559-16896c17cc3f',N'Statement of financial position',N'Summary',N'DetailsEntry',N'Line/Document/PostingDate <= @Date and Account/AccountType/Node DescOf 1',NULL,0,1,0),
(N'6c7ba5e1-4f2d-4882-829e-406d71137ad4',N'Statement of cash flow - Direct Method',N'Summary',N'DetailsEntry',N'Account/AccountType/Code = ''CashAndCashEquivalents'' and EntryType/Code <> ''InternalCashTransferExtension''',NULL,0,1,0),
(N'aa5c998a-bc0b-49f1-8e03-80775cc4c15a',N'Trial Balance', N'Summary',N'DetailsEntry',NULL, NULL,0,	1,	0),
(N'30d3f1d2-d168-4414-a933-305e99a71269',N'Trial Balance By State', N'Summary',N'DetailsEntry',NULL, NULL,0,	1,	0),
(N'9ce0a0e3-772d-406a-8aef-46684b757eac',N'Journal', N'Details',N'DetailsEntry',N'Line/Document/PostingDate >= @FromDate and Line/Document/PostingDate <= @ToDate  And Line/Document/State = @DocumentState And Line/State = @LineState And  AccountId = @AccountId And CurrencyId = @Currency', N'Line/Document/PostingDate,Line/Document/Id,Direction desc', NULL,	NULL,	0);

SET IDENTITY_INSERT dbo.ReportDimensionDefinitions ON
INSERT INTO dbo.ReportDimensionDefinitions(Id, [Index], ReportDefinitionId, Discriminator, [Path], OrderDirection, AutoExpand) VALUES
(1,	0,	N'6c7ba5e1-4f2d-4882-829e-406d71137ad4',	N'Row',		N'EntryType',	NULL, 1),
(2,	0,	N'281dba1b-7e3d-4497-b396-877ba91087c8',	N'Row',		N'Account',		NULL, 1),
(3,	0,	N'281dba1b-7e3d-4497-b396-877ba91087c8',	N'Column',	N'Direction',	N'desc', 1),
(4,	0,	N'5aeec2a2-3008-4c62-8559-16896c17cc3f',	N'Row',		N'Account',		NULL, 1),
(5,	0,	N'0c46cb52-739f-4308-82dd-7cd578bb04ff',	N'Row',		N'Account',		NULL, 1),
(6,	0,	N'aa5c998a-bc0b-49f1-8e03-80775cc4c15a',	N'Row',		N'Account',		NULL, 1),
(7,	1,	N'aa5c998a-bc0b-49f1-8e03-80775cc4c15a',	N'Column',	N'Direction',	N'desc', 1),
(8,	0,	N'aa5c998a-bc0b-49f1-8e03-80775cc4c15a',	N'Column',	N'CurrencyId',	NULL, 1),
(9,	0,	N'30d3f1d2-d168-4414-a933-305e99a71269',	N'Row',		N'Account',		NULL, 1),
(10,0,	N'30d3f1d2-d168-4414-a933-305e99a71269',	N'Column',	N'Line/State',	NULL, 1),
(11,1,	N'30d3f1d2-d168-4414-a933-305e99a71269',	N'Column',	N'Direction',	N'desc', 1),
(12,0,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'Row',		N'Account',		NULL, 1),
(13,0,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'Column',	N'Direction',	N'desc', 1),
(14,1,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'Column',	N'CurrencyId',	NULL, 1);
SET IDENTITY_INSERT dbo.ReportDimensionDefinitions OFF

SET IDENTITY_INSERT dbo.ReportMeasureDefinitions ON
INSERT INTO dbo.ReportMeasureDefinitions(Id, [Index], ReportDefinitionId, [Path], Label, Aggregation) VALUES
(1,	0,	N'6c7ba5e1-4f2d-4882-829e-406d71137ad4',	N'AlgebraicValue', N'Changes', 'sum'),
(2,	0,	N'281dba1b-7e3d-4497-b396-877ba91087c8',	N'MonetaryValue', NULL, 'sum'),
(3,	0,	N'5aeec2a2-3008-4c62-8559-16896c17cc3f',	N'AlgebraicValue', N'Balance', 'sum'),
(4,	0,	N'0c46cb52-739f-4308-82dd-7cd578bb04ff',	N'AlgebraicValue', N'Change', 'sum'),
(5,	0,	N'aa5c998a-bc0b-49f1-8e03-80775cc4c15a',	N'MonetaryValue', NULL, 'sum'),
(6,	0,	N'30d3f1d2-d168-4414-a933-305e99a71269',	N'Value', NULL, 'sum');
SET IDENTITY_INSERT dbo.ReportMeasureDefinitions OFF

SET IDENTITY_INSERT dbo.ReportParameterDefinitions ON
INSERT INTO dbo.ReportParameterDefinitions([Id], [Index], ReportDefinitionId, [Key], [Label], Visibility) VALUES
(1,	0	,N'281dba1b-7e3d-4497-b396-877ba91087c8',	N'Currency' ,NULL,N'Required'),

(2,	0	,N'5aeec2a2-3008-4c62-8559-16896c17cc3f',	N'Date' ,NULL,N'Optional'),

(3,	0	,N'0c46cb52-739f-4308-82dd-7cd578bb04ff'	,N'fromDate' ,N'From Date',N'Optional'),
(4,	1	,N'0c46cb52-739f-4308-82dd-7cd578bb04ff'	,N'toDate' ,N'To Date',N'Optional'),

(5,	0	,N'9ce0a0e3-772d-406a-8aef-46684b757eac'	,N'fromDate' ,N'From Date',N'Optional'),
(6,	1	,N'9ce0a0e3-772d-406a-8aef-46684b757eac'	,N'ToDate' ,N'To Date',N'Optional'),
(7,	2	,N'9ce0a0e3-772d-406a-8aef-46684b757eac'	,N'DocumentState' ,N'Document State',N'Optional'),
(8,	3	,N'9ce0a0e3-772d-406a-8aef-46684b757eac'	,N'LineState' ,N'Line State',N'Optional'),
(9,	4	,N'9ce0a0e3-772d-406a-8aef-46684b757eac'	,N'Currency' ,NULL,N'Optional'),
(10,5	,N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'AccountId' ,NULL,N'Optional');
SET IDENTITY_INSERT dbo.ReportParameterDefinitions OFF

SET IDENTITY_INSERT dbo.ReportSelectDefinitions ON
INSERT INTO dbo.ReportSelectDefinitions([Id], [Index], ReportDefinitionId, [Path]) VALUES
(3,	2,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'Line/Document/SerialNumber'),
(4,	3,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'Line/State'),
(5,	4,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'Account'),
(6,	5,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'CurrencyId'),
(7,	6,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'Direction'),
(8,	7,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'MonetaryValue'),
(9,	8,	N'9ce0a0e3-772d-406a-8aef-46684b757eac',	N'Value');
SET IDENTITY_INSERT dbo.ReportSelectDefinitions OFF

--UPDATE Settings SET DefinitionsVersion = NewId()
RETURN;
ERR_LABEL:
	SELECT * FROM OpenJson(@ValidationErrorsJson)
	WITH (
		[Key] NVARCHAR (255) '$.Key',
		[ErrorName] NVARCHAR (255) '$.ErrorName',
		[Argument0] NVARCHAR (255) '$.Argument0',
		[Argument1] NVARCHAR (255) '$.Argument1',
		[Argument2] NVARCHAR (255) '$.Argument2',
		[Argument3] NVARCHAR (255) '$.Argument3',
		[Argument4] NVARCHAR (255) '$.Argument4'
	);
RETURN;

IF @DB = N'100' -- ACME, USD, en/ar/zh playground
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh car service
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am manyfacturing and sales
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar trading
BEGIN
	Print N'Tellma'
END