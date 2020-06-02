INSERT INTO @Roles([Index],[Code],[Name],[Name2],[Name3],[IsPublic]) VALUES
(0, N'FinanceManager', N'Finance Manager', NULL, NULL, 0),
(1, N'GeneralManager', N'General Manager', NULL, NULL, 0),
(2, N'Reader', N'Reader', NULL, NULL, 0),
(3, N'AccountManager', N'Account Manager', NULL, NULL, 0),
(4, N'Comptroller', N'Comptroller', NULL, NULL, 0),
(5, N'CashCustodian', N'Cashier', NULL, NULL, 0),
(6, N'AdminAffairs', N'Admin. Affairs', NULL, NULL, 0),
(7, N'ProductionManager', N'Production Manager', NULL, NULL, 0),
(8, N'HrManager', N'HR Manager', NULL, NULL, 0),
(9, N'SalesManager', N'Sales Manager', NULL, NULL, 0),
(10, N'SalesPerson', N'Sales Person', NULL, NULL, 0),
(11, N'InventoryCustodian', N'Inventory Custodian', NULL, NULL, 0),
(99, N'Public', N'Public', NULL, NULL, 1);

INSERT INTO @Members ([UserId])	VALUES(@AdminUserId);
INSERT INTO @Permissions ([Action], [View]) VALUES (N'All', N'all');

EXEC api.Roles__Save
	@Entities = @Roles,
	@Members = @Members,
	@Permissions = @Permissions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
DELETE FROM @Roles; DELETE FROM @Members; DELETE FROM @Permissions

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Roles: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @AdministratorRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Administrator');
DECLARE @FinanceManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'FinanceManager');
DECLARE @GeneralManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'GeneralManager');
DECLARE @ReaderRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Reader');
DECLARE @AccountManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AccountManager');
DECLARE @ComptrollerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Comptroller');
DECLARE @CashierRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'CashCustodian');
DECLARE @AdminAffairsRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AdminAffairs');
DECLARE @ProductionManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'ProductionManager');
DECLARE @HRManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'HrManager');
DECLARE @SalesManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesManager');
DECLARE @SalesPersonRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesPerson');
DECLARE @InventoryCustodianRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'InventoryCustodian');
DECLARE @PublicRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Public');