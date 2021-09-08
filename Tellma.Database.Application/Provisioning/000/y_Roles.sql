INSERT INTO @Roles([Index],[Code],[Name],[IsPublic]) VALUES
(0, N'Administrator', N'Administrator', 0),
(1, N'GeneralManager', N'General Manager', 0),
(2, N'FinanceManager', N'Finance Manager', 0),
(3, N'Comptroller', N'Comptroller', 0),
(4, N'Accountant', N'Accountant', 0),
(5, N'Cashier', N'Cashier', 0),
(6, N'InternalAuditor', N'Internal Auditor', 0),
(7, N'ExternalAuditor', N'External Auditor', 0),
(8, N'StoreKeeper', N'Store Keeper', 0),
(9, N'AdminAffairs', N'Admin. Affairs', 0),
(10, N'ProductionManager', N'Production Manager', 0),
(11, N'ProjectManager', N'Project Manager', 0),
(12, N'HrManager', N'HR Manager', 0),
(13, N'SalesManager', N'Sales Manager', 0),
(14, N'SalesPerson', N'Sales Person', 0),
(15, N'AccountManager', N'Account Manager', 0),
(98, N'Reader', N'Reader', 0),
(99, N'Public', N'Public', 1);

INSERT INTO @Members([Index], [HeaderIndex], [UserId]) VALUES(0, 0, @AdminUserId);

INSERT INTO @Permissions([Index], [HeaderIndex],
--Action: N'Read', N'Update', N'Delete', N'IsActive', N'SendInvitationEmail', N'State', N'All'))
			[Action],	[Criteria],			[View]) VALUES
 (0,0,		N'All',		NULL,				N'all');

INSERT INTO @ValidationErrors
EXEC [api].[Roles__Save]
	@Entities = @Roles,
	@Members = @Members,
	@Permissions = @Permissions,
	@ReturnIds = 0,
	@UserId = @AdminUserId;

IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'Roles: Error Provisioning'
	GOTO Err_Label;
END;

-- Declarations
--DECLARE @AdministratorRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Administrator');
--DECLARE @GeneralManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'GeneralManager');
--DECLARE @FinanceManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'FinanceManager');
--DECLARE @ComptrollerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Comptroller');
--DECLARE @AccountantRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Accountant');
--DECLARE @CashierRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Cashier');
--DECLARE @InternalAuditorRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'InternalAuditor');
--DECLARE @ExternalAuditorRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'ExternalAuditor');
--DECLARE @StoreKeeperRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'StoeKeeper');
--DECLARE @AdminAffairsRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AdminAffairs');
--DECLARE @ProductionManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'ProductionManager');
--DECLARE @ProjectManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'ProjectManager');
--DECLARE @HrManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'HrManager');
--DECLARE @SalesManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesManager');
--DECLARE @SalesPersonRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesPerson');
--DECLARE @AccountManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AccountManager');
--DECLARE @ReaderRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Reader');
--DECLARE @PublicRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Public');