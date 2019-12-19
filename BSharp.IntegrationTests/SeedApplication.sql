-- This file is executed before any test is run

DECLARE @UserId INT, @RoleId INT;
SELECT @UserId = [Id] FROM [dbo].[Users] WHERE [Email] = @Email;
SELECT @RoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'Administrator';

EXEC sp_set_session_context 'UserId', @UserId;

-- Cleanup, Central records before lookup records

DELETE FROM [dbo].[Documents]
DELETE FROM [dbo].[Permissions];
DELETE FROM [dbo].[RoleMemberships];

DELETE FROM [dbo].[Accounts];

DELETE FROM [dbo].[Roles] WHERE [Id] <> @RoleId;
DELETE FROM [dbo].[Users] WHERE [Id] <> @UserId;
DELETE FROM [dbo].[Agents];
DELETE FROM [dbo].[Resources];
DELETE FROM [dbo].[Currencies] WHERE Id NOT IN (Select FunctionalCurrencyId FROM [dbo].[Settings]);
--DELETE FROM [dbo].[ResourceClassifications] WHERE [Code] NOT IN (N'CashAndCashEquivalents');

DELETE FROM [dbo].[ResourceClassificationsEntryClassifications];
DELETE FROM [dbo].[ResourceClassifications];
DELETE FROM [dbo].[Lookups];
DELETE FROM [dbo].[MeasurementUnits];
DELETE FROM [dbo].[AccountClassifications];
DELETE FROM [dbo].[ResourceDefinitions];
DELETE FROM [dbo].[LookupDefinitions];
DELETE FROM [dbo].[ResponsibilityCenters];
DELETE FROM [dbo].[AccountTypes];
DELETE FROM [dbo].[EntryClassifications];

-- Populate


DECLARE @PTAccountTypes dbo.[AccountTypeList];
INSERT INTO @PTAccountTypes (
	[Id],					[Name],						[Description]) VALUES
(N'AccountsPayable',		N'Accounts Payable',		N'This represents balances owed to vendors for goods, supplies, and services purchased on an open account. Accounts payable balances are used in accrual-based accounting, are generally due in 30 or 60 days, and do not bear interest.'),
(N'AccountsReceivable',		N'Accounts Receivable',		N'This represents amounts owed by customers for items or services sold to them when cash is not received at the time of sale. Typically, accounts receivable balances are recorded on sales invoices that include terms of payment. Accounts receivable are used in accrual-based accounting.');

DECLARE @OdooAccountTypes dbo.[AccountTypeList];

INSERT INTO dbo.AccountTypes SELECT * FROM @PTAccountTypes;

INSERT INTO [dbo].[Permissions] ([RoleId], [ViewId], [Action])
VALUES
(@RoleId, N'users', N'All'),
(@RoleId, N'roles', N'All')


INSERT INTO [dbo].[RoleMemberships] ([UserId], [RoleId])
VALUES (@UserId, @RoleId)

IF NOT EXISTS(SELECT * FROM [dbo].[DocumentDefinitions] WHERE [Id] = N'manual-journal-vouchers')
	INSERT INTO [dbo].[DocumentDefinitions]	([Id], [TitleSingular], [TitlePlural], [Prefix]) VALUES
	(N'manual-journal-vouchers', N'Manual Journal Voucher',	N'Manual Journal Vouchers',	N'JV');
	
IF NOT EXISTS(SELECT * FROM [dbo].[LineDefinitions] WHERE [Id] = N'ManualLine')
	INSERT INTO [dbo].[LineDefinitions]([Id], [TitleSingular], [TitlePlural]) VALUES
	(N'ManualLine', N'Adjustment', N'Adjustments');

IF NOT EXISTS(SELECT * FROM [dbo].[LookupDefinitions] WHERE [Id] = N'colors')
	INSERT INTO [dbo].[LookupDefinitions]([Id])
	VALUES(N'colors');	

IF NOT EXISTS(SELECT * FROM [dbo].[AgentDefinitions] WHERE [Id] = N'customers')
	INSERT INTO [dbo].[AgentDefinitions]([Id])
	VALUES(N'customers');

IF NOT EXISTS(SELECT * FROM [dbo].[ResourceDefinitions] WHERE [Id] = N'currencies')
	INSERT INTO [dbo].[ResourceDefinitions]([Id])
	VALUES(N'currencies');

UPDATE Settings SET DefinitionsVersion = NEWID(), SettingsVersion = NEWID();

-- Resource Types
DECLARE @ResourceClassifications dbo.ResourceClassificationList
INSERT INTO @ResourceClassifications
([Code],									[Name],											[ParentIndex],		[IsAssignable], [Index], [ResourceDefinitionId]) VALUES
(N'CashAndCashEquivalents',					N'Cash and cash equivalents',					NULL,				1,				0,			N'currencies'),
	(N'Cash',								N'Cash',										0,					1,				1,			N'currencies'),
	(N'CashEquivalents',					N'Cash equivalents',							0,					1,				2,			N'currencies');

DECLARE @ValidationErrorsJson nvarchar(max);
EXEC [api].[ResourceClassifications__Save]
	@Entities = @ResourceClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

DECLARE @EntryClassifications dbo.EntryClassificationList;
INSERT INTO @EntryClassifications([IsAssignable], [Index], [ForDebit], [ForCredit], [ParentIndex], [Code], [Name]) VALUES
 (0, 0, 1, 1, NULL, 'ChangesInPropertyPlantAndEquipment', 'Increase (decrease) in property, plant and equipment')
,(1, 1, 1, 0, 0, 'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment', 'Additions other than through business combinations, property, plant and equipment')
,(1, 2, 1, 0, 0, 'AcquisitionsThroughBusinessCombinationsPropertyPlantAndEquipment', 'Acquisitions through business combinations, property, plant and equipment')

EXEC [api].[EntryClassifications__Save]
	@Entities = @EntryClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

-- Add mapping
DECLARE @EntryClassificationId INT = (SELECT Id FROM [EntryClassifications] WHERE Code = 'ChangesInPropertyPlantAndEquipment')
DECLARE @ResourceClassificationId INT = (SELECT Id FROM [ResourceClassifications] WHERE Code = 'CashAndCashEquivalents')
INSERT INTO [dbo].[ResourceClassificationsEntryClassifications] (ResourceClassificationId, EntryClassificationId, IsEnforced)
VALUES
(@ResourceClassificationId, @EntryClassificationId, 1);

-- Currencies
DECLARE @Currencies CurrencyList;
INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [E]) VALUES
(0, N'USD', N'US Dollar',N'دولار أمريكي', 2),
(1, N'ETB', N'ET Birr', N'بر أثيوبي', 2);

EXEC dal.Currencies__Save
	@Entities = @Currencies;
