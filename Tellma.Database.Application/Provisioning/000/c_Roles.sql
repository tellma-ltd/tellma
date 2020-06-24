INSERT INTO @Roles([Index],[Code],[Name],[IsPublic]) VALUES
(0, N'Administrator', N'Administrator', 0),
(1, N'FinanceManager', N'Finance Manager', 0),
(2, N'GeneralManager', N'General Manager', 0),
(3, N'Reader', N'Reader', 0),
(4, N'AccountManager', N'Account Manager', 0),
(5, N'Comptroller', N'Comptroller', 0),
(6, N'CashCustodian', N'Cashier', 0),
(7, N'AdminAffairs', N'Admin. Affairs', 0),
(8, N'ProductionManager', N'Production Manager', 0),
(9, N'HrManager', N'HR Manager', 0),
(10, N'SalesManager', N'Sales Manager', 0),
(11, N'SalesPerson', N'Sales Person', 0),
(12, N'InventoryCustodian', N'Inventory Custodian', 0),
(99, N'Public', N'Public', 1);

INSERT INTO @Members
([Index],	[HeaderIndex],	[UserId]) VALUES
(0,			0,				@AdminUserId);

INSERT INTO @Permissions([Index], [HeaderIndex],
--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'State', N'All'))
	[Action],	[Criteria],			[View]) VALUES
(0,0,N'All',	NULL,				N'all'),
(0,1,N'All',	NULL,				N'all'),
(0,2,N'Read',	NULL,				N'all'),
(0,3,N'All',	N'CreatedById = Me',N'documents/revenue-recognition-vouchers'),
(1,3,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
									N'documents/cash-payment-vouchers'),
(2,3,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
									N'documents/cash-receipt-vouchers'),
(0,4,N'All',	NULL,				N'documents/manual-journal-vouchers'),
(1,4,N'All',	NULL,				N'documents/cash-payment-vouchers'),
(2,4,N'All',	NULL,				N'documents/revenue-recognition-vouchers'),
(3,4,N'Read',	NULL,				N'accounts'),
(0,5,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
									N'documents/cash-payment-vouchers'),
(1,5,N'All',	N'Agent/UserId = Me or AssigneeId = Me',
									N'documents/cash-receipt-vouchers'),
(2,5,N'Update', NULL,				N'contracts/suppliers'),

(0,9,N'Read',	NULL,				N'contracts/cash-custodians'),
(1,9,N'Read',	NULL,				N'centers'),
(2,9,N'Read',	NULL,				N'currencies'),
(3,9,N'Update',	N'CreatedById = Me',N'documents/cash-payment-vouchers'),
(4,9,N'Read',	NULL,				N'resources/employee-benefits'),
(5,9,N'Read',	NULL,				N'entry-types'),
(6,9,N'Read',	NULL,				N'lookups/it-equipment-manufacturers'),
(7,9,N'Read',	NULL,				N'units'),
(8,9,N'Read',	NULL,				N'lookups/operating-systems'),
(9,9,N'Read',	NULL,				N'centers'),
(10,9,N'Read',	NULL,				N'resources/services-expenses'),
(11,9,N'Read',	NULL,				N'roles'),
(12,9,N'Read',	NULL,				N'contracts/suppliers'),
(13,9,N'Read',	NULL,				N'users');

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

-- Declarations
DECLARE @AdministratorRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Administrator');
DECLARE @FinanceManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'FinanceManager');
DECLARE @GeneralManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'GeneralManager');
DECLARE @ReaderRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Reader');
DECLARE @AccountManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AccountManager');
DECLARE @ComptrollerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Comptroller');
DECLARE @CashCustodianRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'CashCustodian');
DECLARE @AdminAffairsRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AdminAffairs');
DECLARE @ProductionManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'ProductionManager');
DECLARE @HrManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'HrManager');
DECLARE @SalesManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesManager');
DECLARE @SalesPersonRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesPerson');
DECLARE @InventoryCustodianRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'InventoryCustodian');
DECLARE @PublicRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Public');
