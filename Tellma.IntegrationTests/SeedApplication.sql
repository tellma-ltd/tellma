-- This file is executed before any test is run

DECLARE @UserId INT, @RoleId INT;
SELECT @UserId = [Id] FROM [dbo].[Users] WHERE [Email] = @Email;
SELECT @RoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'Administrator';

EXEC sp_set_session_context 'UserId', @UserId;

-- Cleanup, Central records before lookup records

DELETE FROM [dbo].[ExchangeRates];
DELETE FROM [dbo].[ReportDefinitions];
DELETE FROM [dbo].[Documents]
DELETE FROM [dbo].[Permissions];
DELETE FROM [dbo].[RoleMemberships];

DELETE FROM [dbo].[Accounts];

DELETE FROM [dbo].[WorkflowSignatures];
DELETE FROM [dbo].[Workflows];

DELETE FROM [dbo].[LineDefinitionColumns];
DELETE FROM [dbo].[LineDefinitionEntries];
DELETE FROM [dbo].[LineDefinitionStateReasons];

DELETE FROM [dbo].[Roles] WHERE [Id] <> @RoleId;
DELETE FROM [dbo].[Users] WHERE [Id] <> @UserId;
DELETE FROM [dbo].[Agents];
DELETE FROM [dbo].[Resources];
DELETE FROM [dbo].[Currencies] WHERE Id NOT IN (Select FunctionalCurrencyId FROM [dbo].[Settings]);

DELETE FROM [dbo].[Lookups];
DELETE FROM [dbo].[Units];
DELETE FROM [dbo].[LegacyClassifications];
DELETE FROM [dbo].[ResourceDefinitions];
DELETE FROM [dbo].[LookupDefinitions];
DELETE FROM [dbo].[Centers];
DELETE FROM [dbo].[AccountTypes];
DELETE FROM [dbo].[EntryTypes];
	
DELETE FROM [dbo].[DocumentDefinitionLineDefinitions];
DELETE FROM [dbo].[DocumentDefinitions];
DELETE FROM [dbo].[LineDefinitions];

-- Populate


DECLARE @PTAccountTypes dbo.[AccountTypeList];
INSERT INTO @PTAccountTypes (
	[Index], [Code],					[Name],						[Description]) VALUES
(0, N'AccountsPayable',		N'Accounts Payable',	N'This represents balances owed to vendors for goods, supplies, and services purchased on an open account. Accounts payable balances are used in accrual-based accounting, are generally due in 30 or 60 days, and do not bear interest.'),
(1, N'AccountsReceivable',	N'Accounts Receivable',	N'This represents amounts owed by customers for items or services sold to them when cash is not received at the time of sale. Typically, accounts receivable balances are recorded on sales invoices that include terms of payment. Accounts receivable are used in accrual-based accounting.');

EXEC dal.AccountTypes__Save @Entities = @PTAccountTypes;
-- INSERT INTO dbo.AccountTypes ([Code], [Name], [Description]) SELECT [Code], [Name], [Description] FROM @PTAccountTypes;

INSERT INTO [dbo].[Permissions] ([RoleId], [View], [Action])
VALUES
(@RoleId, N'users', N'All'),
(@RoleId, N'roles', N'All')


INSERT INTO [dbo].[RoleMemberships] ([UserId], [RoleId])
VALUES (@UserId, @RoleId)

--IF NOT EXISTS(SELECT * FROM [dbo].[DocumentDefinitions] WHERE [Id] = N'manual-journal-vouchers')
--	INSERT INTO [dbo].[DocumentDefinitions]	([Id], [TitleSingular], [TitlePlural], [Prefix]) VALUES
--	(N'manual-journal-vouchers', N'Manual Journal Voucher',	N'Manual Journal Vouchers',	N'JV');
	
--IF NOT EXISTS(SELECT * FROM [dbo].[LineDefinitions] WHERE [Id] = N'ManualLine')
--	INSERT INTO [dbo].[LineDefinitions]([Id], [TitleSingular], [TitlePlural]) VALUES
--	(N'ManualLine', N'Adjustment', N'Adjustments');

IF NOT EXISTS(SELECT * FROM [dbo].[LookupDefinitions] WHERE [Id] = N'colors')
	INSERT INTO [dbo].[LookupDefinitions]([Id])
	VALUES(N'colors');	

IF NOT EXISTS(SELECT * FROM [dbo].[AgentDefinitions] WHERE [Id] = N'customers')
	INSERT INTO [dbo].[AgentDefinitions]([Id])
	VALUES(N'customers');

IF NOT EXISTS(SELECT * FROM [dbo].[ResourceDefinitions] WHERE [Id] = N'currencies')
	INSERT INTO [dbo].[ResourceDefinitions]([Id], [ParentAccountTypeId])
	SELECT TOP 1 N'currencies', [Id] FROM [dbo].[AccountTypes];

UPDATE Settings SET DefinitionsVersion = NEWID(), SettingsVersion = NEWID();

DECLARE @ValidationErrorsJson nvarchar(max);

DECLARE @EntryTypes dbo.EntryTypeList;
INSERT INTO @EntryTypes([IsAssignable], [Index], [ParentIndex], [Code], [Name]) VALUES
 (0, 0,  NULL, 'ChangesInPropertyPlantAndEquipment', 'Increase (decrease) in property, plant and equipment')
,(1, 1, 0, 'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment', 'Additions other than through business combinations, property, plant and equipment')
,(1, 2, 0, 'AcquisitionsThroughBusinessCombinationsPropertyPlantAndEquipment', 'Acquisitions through business combinations, property, plant and equipment')

EXEC [api].[EntryTypes__Save]
	@Entities = @EntryTypes,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

-- Currencies
DECLARE @Currencies CurrencyList;
INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [E]) VALUES
(0, N'USD', N'US Dollar',N'دولار أمريكي', 2),
(1, N'ETB', N'ET Birr', N'بر أثيوبي', 2);

EXEC dal.Currencies__Save
	@Entities = @Currencies;

-- Line Definitions
DECLARE @AccountsPayable INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'AccountsPayable');
DECLARE @LineDefinitions dbo.LineDefinitionList;
DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
DECLARE @LineDefinitionStateReasons dbo.LineDefinitionStateReasonList;
DECLARE @Workflows dbo.WorkflowList;
DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;

INSERT @LineDefinitions([Index],
[Id],			[TitleSingular], [TitlePlural], [TitleSingular2], [TitlePlural3]) VALUES
(0,N'ManualLine', N'Adjustment', N'Adjustments',N'تسوية',			N'تسويات');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,0,+1,	@AccountsPayable);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],		[RequiredState],
																[ReadOnlyState],
																[InheritsFromHeader]) VALUES
(0,0,	N'Entries',	N'Account',		0,			N'Account',		4,4,0), -- together with properties
(1,0,	N'Entries',	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,0,	N'Entries',	N'Value',		0,			N'Credit',		4,4,0),
(3,0,	N'Lines',	N'Memo',		0,			N'Memo',		5,4,1); -- only if it appears,
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name],					[Name2]) VALUES
(0,0,-4,	N'Duplicate Line',		N'بيانات مكررة'),
(1,0,-4,	N'Incorrect Analysis',	N'تحليل خطأ'),
(2,0,-4,	N'Other reasons',		N'أسباب أخرى');

EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

-- Document Definitions
DECLARE @DocumentDefinitions dbo.DocumentDefinitionList;
DECLARE @DocumentDefinitionLineDefinitions dbo.DocumentDefinitionLineDefinitionList;
INSERT @DocumentDefinitions([Index],	
	[Id],							[TitleSingular],				[TitleSingular2],		[TitlePlural],					[TitlePlural2],			[Prefix],	[MainMenuIcon],			[MainMenuSection],	[MainMenuSortKey]) VALUES
(0,	N'manual-journal-vouchers',		N'Manual Journal Voucher',		N'قيد تسوية يدوي',		N'Manual Journal Vouchers',		N'قيود تسوية يدوية',	N'JV',		N'book',				N'Financials',		0);

INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId],			[IsVisibleByDefault]) VALUES
	(0,0,	N'ManualLine',				1);

EXEC dal.DocumentDefinitions__Save
	@Entities = @DocumentDefinitions,
	@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions;