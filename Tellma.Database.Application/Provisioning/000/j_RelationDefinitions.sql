INSERT INTO @RelationDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey], [CenterVisibility], [ImageVisibility], [LocationVisibility], [FromDateVisibility], [FromDateLabel], [ToDateVisibility], [ToDateLabel], [AgentVisibility],[TaxIdentificationNumberVisibility],[JobVisibility],[BankAccountNumberVisibility], [RelationVisibility], [RelationDefinitionId], [UserCardinality]) VALUES
(0, N'Creditor', N'Creditor', N'Creditors', N'hands', N'Financials',100,N'None', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'Optional', N'None', N'', N'Single'),
(1, N'Debtor', N'Debtor', N'Debtors', N'hand-holding-usd', N'Financials',105,N'None', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'Optional', N'None', N'', N'None'),
(2, N'Owner', N'Owner', N'Owners', N'power-off', N'Financials',110,N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'Optional', N'None', N'Optional', N'None', N'', N'Single'),
(3, N'Partner', N'Partner', N'Partners', N'user-tie', N'Financials',115,N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'Optional', N'None', N'Optional', N'None', N'', N'Single'),
(4, N'Supplier', N'Supplier', N'Suppliers', N'user-tag', N'Purchasing',120,N'None', N'None', N'None', N'None', N'', N'None', N'', N'None', N'Optional', N'None', N'Optional', N'None', N'', N'Single'),
(5, N'Customer', N'Customer', N'Customers ', N'user-shield', N'Sales',125,N'None', N'None', N'None', N'Optional', N'Customer Since', N'None', N'', N'None', N'Optional', N'None', N'None', N'None', N'', N'Single'),
(6, N'Employee', N'Employee', N'Employees', N'user-friends', N'HumanCapital',130,N'None', N'Optional', N'None', N'Optional', N'Joining Date', N'Optional', N'Termination Date', N'Optional', N'Optional', N'None', N'Optional', N'None', N'', N'Single'),
(7, N'BankBranch', N'Bank Branch', N'Bank Branches', N'university', N'Cash',135,N'None', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'None', N'None', N'', N'None'),
(9, N'Other', N'Other', N'Others', N'air-freshener', N'Financials',140,N'None', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'None', N'None', N'', N'None'),
(10, N'BankAccount', N'Bank Account', N'Bank Accounts', N'book', N'Cash',135,N'Required', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'None', N'Optional', N'@BankBranchRLD', N'None'),
(11, N'CashOnHandAccount', N'Cash Account', N'Cash On Hand Accounts', N'door-closed', N'Cash',140,N'Required', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'None', N'Required', N'@EmployeeRLD', N'None'),
(12, N'Warehouse', N'Warehouse', N'Warehouses', N'warehouse', N'Inventory',145,N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'None', N'None', N'None', N'None', N'Optional', N'@EmployeeRLD', N'None'),
(13, N'PPECustody', N'Fixed Asset Custody', N'Fixed Assets Custodies', N'user-shield', N'FixedAssets',150,N'Required', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'None', N'Optional', N'@EmployeeRLD', N'None'),
(15, N'TransitCustody', N'Transit Custody', N'Transit Custodies', N'ship', N'Purchasing',160,N'Required', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'None', N'None', N'@SupplierRLD', N'None'),
(16, N'TaskCustody', N'Task Assignment', N'Tasks Assignments', N'clipboard-list', N'Administration',170,N'Required', N'None', N'None', N'None', N'', N'None', N'', N'None', N'None', N'None', N'None', N'Required', N'@EmployeeRLD', N'None');

UPDATE @RelationDefinitions
SET
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Market Segment', [Lookup1DefinitionId] = @MarketSegmentLKD
WHERE [Code] IN ( N'Customer')

UPDATE @RelationDefinitions
SET 
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Bank', [Lookup1DefinitionId] = @BankLKD
WHERE [Code] IN ( N'BankBranch')

UPDATE @RelationDefinitions
SET 
	[CurrencyVisibility] = N'Required', [ExternalReferenceVisibility] = N'Optional', [ExternalReferenceLabel] = N'Bank Account Number',
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Bank Account Type', [Lookup1DefinitionId] = @BankAccountTypeLKD
WHERE [Code] IN ( N'BankAccount')


EXEC [api].[RelationDefinitions__Save]
	@Entities = @RelationDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'RelationDefinitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
--Declarations
DECLARE @CreditorRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Creditor');
DECLARE @DebtorRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Debtor');
DECLARE @OwnerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Owner');
DECLARE @PartnerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Partner');
DECLARE @SupplierRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Supplier');
DECLARE @CustomerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Customer');
DECLARE @EmployeeRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Employee');
DECLARE @BankBranchRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BankBranch');
DECLARE @OtherRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Other');
DECLARE @BankAccountCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'BankAccount');
DECLARE @CashOnHandAccountCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'CashOnHandAccount');
DECLARE @WarehouseCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Warehouse');
DECLARE @PPECustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'PPECustody');
DECLARE @TransitCustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'TransitCustody');
DECLARE @TaskCustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'TaskCustody');
