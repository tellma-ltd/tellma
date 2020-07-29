INSERT INTO @CustodyDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey], [CenterVisibility], [ImageVisibility], [LocationVisibility],[BankAccountNumberVisibility], [RelationVisibility],[RelationDefinitionId]) VALUES
(0, N'BankAccount', N'Bank Account', N'Bank Accounts', N'book', N'Cash',135,N'Required', N'None', N'None', N'None', N'Optional',NULL),
(1, N'Safe', N'Safe', N'Safes', N'door-closed', N'Cash',140,N'Required', N'None', N'None', N'None', N'Optional',@EmployeeCD),
(2, N'Warehouse', N'Warehouse', N'Warehouses', N'warehouse', N'Inventory',145,N'Optional', N'Optional', N'Optional', N'None', N'Optional',@EmployeeCD),
(3, N'PPECustody', N'Fixed Asset Custody', N'Fixed Assets Custodies', N'user-shield', N'FixedAssets',150,N'Required', N'None', N'None', N'None', N'Optional',@EmployeeCD),
(4, N'Shipper', N'Shipper', N'Shippers', N'ship', N'Purchasing',160,N'Required', N'None', N'None', N'None', N'None',NULL);

UPDATE @CustodyDefinitions
SET 
	[CurrencyVisibility] = N'Required', 
	[Text1Visibility] = N'Optional', [Text1Label] =  N'Branch',
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Account Type', [Lookup1DefinitionId] = @BankAccountTypeLKD,
	[Lookup2Visibility] = N'Optional', [Lookup2Label] = N'Bank', [Lookup2DefinitionId] = @BankLKD
WHERE [Code] IN ( N'BankAccount')


EXEC [api].[CustodyDefinitions__Save]
	@Entities = @CustodyDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'CustodyDefinitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

--Declarations
--DECLARE @BankAccountCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'BankAccount');
--DECLARE @SafeCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Safe');
--DECLARE @WarehouseCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Warehouse');
--DECLARE @PPECustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'PPECustody');
--DECLARE @ShipperCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Shipper');