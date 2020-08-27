INSERT INTO @CustodyDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey], [CenterVisibility], [ImageVisibility], [LocationVisibility], [CustodianVisibility],[CustodianDefinitionId]) VALUES
(0, N'BankAccount', N'Bank Account', N'Bank Accounts', N'book', N'Cash',135,N'Required', N'None', N'None', N'Optional',@BankBranchRLD),
(1, N'CashOnHandAccount', N'Cash Account', N'Cash On Hand Accounts', N'door-closed', N'Cash',140,N'Required', N'None', N'None', N'Required',@EmployeeRLD),
(2, N'Warehouse', N'Warehouse', N'Warehouses', N'warehouse', N'Inventory',145,N'Optional', N'Optional', N'Optional', N'Optional',@EmployeeRLD),
(3, N'PPECustody', N'Fixed Asset Custody', N'Fixed Assets Custodies', N'user-shield', N'FixedAssets',150,N'Required', N'None', N'None', N'Optional',@EmployeeRLD),
(4, N'Rental', N'Rental', N'Rentals', N'calendar-alt', N'Sales',155,N'Required', N'None', N'None', N'Optional',@CustomerRLD),
(5, N'TransitCustody', N'Transit Custody', N'Transit Custodies', N'ship', N'Purchasing',160,N'Required', N'None', N'None', N'None',@SupplierRLD),
(6, N'TaskCustody', N'Task Assignment', N'Tasks Assignments', N'clipboard-list', N'Administration',170,N'Required', N'None', N'None', N'Required',@EmployeeRLD);

UPDATE @CustodyDefinitions
SET 
	[CurrencyVisibility] = N'Required', [ExternalReferenceVisibility] = N'Optional', [ExternalReferenceLabel] = N'Bank Account Number',
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Bank Account Type', [Lookup1DefinitionId] = @BankAccountTypeLKD
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
DECLARE @BankAccountCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'BankAccount');
DECLARE @CashOnHandAccountCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'CashOnHandAccount');
DECLARE @WarehouseCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Warehouse');
DECLARE @PPECustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'PPECustody');
DECLARE @RentalCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Rental');
DECLARE @TransitCustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'TransitCustody');
DECLARE @TaskCustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'TaskCustody');