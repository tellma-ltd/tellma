DECLARE @ManualJournalVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@ManualJournalVoucherDD AS NVARCHAR(50));
DECLARE @ClosingPeriodVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@ClosingPeriodVoucherDD AS NVARCHAR(50));
DECLARE @ClosingYearVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@ClosingYearVoucherDD AS NVARCHAR(50));
DECLARE @PaymentIssueToNonTradingAgentsDDPath NVARCHAR(50) = N'documents/' + CAST(@PaymentIssueToNonTradingAgentsDD AS NVARCHAR(50));
DECLARE @DepositCashToBankDDPath NVARCHAR(50) = N'documents/' + CAST(@DepositCashToBankDD AS NVARCHAR(50));
DECLARE @PaymentReceiptFromNonTradingAgentsDDPath NVARCHAR(50) = N'documents/' + CAST(@PaymentReceiptFromNonTradingAgentsDD AS NVARCHAR(50));
DECLARE @StockIssueToNonTradingAgentDDPath NVARCHAR(50) = N'documents/' + CAST(@StockIssueToNonTradingAgentDD AS NVARCHAR(50));
DECLARE @StockTransferDDPath NVARCHAR(50) = N'documents/' + CAST(@StockTransferDD AS NVARCHAR(50));
DECLARE @StockReceiptFromNonTradingAgentDDPath NVARCHAR(50) = N'documents/' + CAST(@StockReceiptFromNonTradingAgentDD AS NVARCHAR(50));
DECLARE @InventoryAdjustmentDDPath NVARCHAR(50) = N'documents/' + CAST(@InventoryAdjustmentDD AS NVARCHAR(50));
DECLARE @PaymentIssueToTradePayableDDPath NVARCHAR(50) = N'documents/' + CAST(@PaymentIssueToTradePayableDD AS NVARCHAR(50));
DECLARE @RefundFromTradePayableDDPath NVARCHAR(50) = N'documents/' + CAST(@RefundFromTradePayableDD AS NVARCHAR(50));
DECLARE @WithholdingTaxFromTradePayableDDPath NVARCHAR(50) = N'documents/' + CAST(@WithholdingTaxFromTradePayableDD AS NVARCHAR(50));
DECLARE @ImportFromTradePayableDDPath NVARCHAR(50) = N'documents/' + CAST(@ImportFromTradePayableDD AS NVARCHAR(50));
DECLARE @GoodReceiptFromImportDDPath NVARCHAR(50) = N'documents/' + CAST(@GoodReceiptFromImportDD AS NVARCHAR(50));
DECLARE @GoodServiceReceiptFromTradePayableDDPath NVARCHAR(50) = N'documents/' + CAST(@GoodServiceReceiptFromTradePayableDD AS NVARCHAR(50));
DECLARE @PaymentReceiptFromTradeReceivableDDPath NVARCHAR(50) = N'documents/' + CAST(@PaymentReceiptFromTradeReceivableDD AS NVARCHAR(50));
DECLARE @RefundToTradeReceivableDDPath NVARCHAR(50) = N'documents/' + CAST(@RefundToTradeReceivableDD AS NVARCHAR(50));
DECLARE @WithholdingTaxByTradeReceivableDDPath NVARCHAR(50) = N'documents/' + CAST(@WithholdingTaxByTradeReceivableDD AS NVARCHAR(50));
DECLARE @GoodIssueToExportDDPath NVARCHAR(50) = N'documents/' + CAST(@GoodIssueToExportDD AS NVARCHAR(50));
DECLARE @ExportToTradeReceivableDDPath NVARCHAR(50) = N'documents/' + CAST(@ExportToTradeReceivableDD AS NVARCHAR(50));
DECLARE @GoodServiceIssueToTradeReceivableDDPath NVARCHAR(50) = N'documents/' + CAST(@GoodServiceIssueToTradeReceivableDD AS NVARCHAR(50));
DECLARE @SteelProductionDDPath NVARCHAR(50) = N'documents/' + CAST(@SteelProductionDD AS NVARCHAR(50));
DECLARE @PlasticProductionDDPath NVARCHAR(50) = N'documents/' + CAST(@PlasticProductionDD AS NVARCHAR(50));
DECLARE @PaintProductionDDPath NVARCHAR(50) = N'documents/' + CAST(@PaintProductionDD AS NVARCHAR(50));
DECLARE @VehicleAssemblyDDPath NVARCHAR(50) = N'documents/' + CAST(@VehicleAssemblyDD AS NVARCHAR(50));
DECLARE @GrainProcessingDDPath NVARCHAR(50) = N'documents/' + CAST(@GrainProcessingDD AS NVARCHAR(50));
DECLARE @OilMillingDDPath NVARCHAR(50) = N'documents/' + CAST(@OilMillingDD AS NVARCHAR(50));
DECLARE @MaintenanceDDPath NVARCHAR(50) = N'documents/' + CAST(@MaintenanceDD AS NVARCHAR(50));
DECLARE @PaymentIssueToEmployeeDDPath NVARCHAR(50) = N'documents/' + CAST(@PaymentIssueToEmployeeDD AS NVARCHAR(50));
DECLARE @EmployeeLoanDDPath NVARCHAR(50) = N'documents/' + CAST(@EmployeeLoanDD AS NVARCHAR(50));
DECLARE @AttendanceRegisterDDPath NVARCHAR(50) = N'documents/' + CAST(@AttendanceRegisterDD AS NVARCHAR(50));
DECLARE @EmployeeOvertimeDDPath NVARCHAR(50) = N'documents/' + CAST(@EmployeeOvertimeDD AS NVARCHAR(50));
DECLARE @EmployeePenaltyDDPath NVARCHAR(50) = N'documents/' + CAST(@EmployeePenaltyDD AS NVARCHAR(50));
DECLARE @EmployeeRewardDDPath NVARCHAR(50) = N'documents/' + CAST(@EmployeeRewardDD AS NVARCHAR(50));
DECLARE @EmployeeLeaveDDPath NVARCHAR(50) = N'documents/' + CAST(@EmployeeLeaveDD AS NVARCHAR(50));
DECLARE @EmployeeLeaveAllowanceDDPath NVARCHAR(50) = N'documents/' + CAST(@EmployeeLeaveAllowanceDD AS NVARCHAR(50));
DECLARE @EmployeeTravelDDPath NVARCHAR(50) = N'documents/' + CAST(@EmployeeTravelDD AS NVARCHAR(50));
INSERT INTO @Roles([Index],
	[Code],					[Name]) VALUES
(0, N'Administrator',		N'Administrator'),
(1, N'Shareholder',			N'Sahareholder'),
(2, N'GeneralManager',		N'General Manager'),

(10,N'FinanceManager',		N'Finance Manager'),
(11,N'Comptroller',			N'Comptroller'),
(12,N'Accountant',			N'Accountant'),
(13,N'Cashier',				N'Cashier'),

(20,N'InternalAuditor',		N'Internal Auditor'),
(21,N'ExternalAuditor',		N'External Auditor'),

(30,N'StoreKeeper',			N'Store Keeper'),

(40,N'ProductionManager',	N'Production Manager'),

(50,N'SalesManager',		N'Sales Manager'),
(51,N'SalesPerson',			N'Sales Person'),

(90,N'HrManager',			N'HR Manager')
INSERT INTO @Roles([Index], [Code],	[Name], [IsPublic]) VALUES (99, N'Public', N'Public', 1);

INSERT INTO @Members([Index], [HeaderIndex], [UserId]) VALUES(0, 0, @AdminUserId);

INSERT INTO @Permissions([Index], [HeaderIndex],
--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'State', N'All'))
			[Action],	[Criteria],			[View]) VALUES
 (0,0,		N'All',		NULL,				N'all'),
-- Board
(100,1,		N'Read',	NULL,				N'all'),
-- 2:GeneralManager
(200,2,		N'Read',	NULL,				N'all'),
-- 3:FinanceManager
(1000,10,	N'Read',	NULL,				N'all'),
-- 11:Comptroller
(1100,11,	N'All',		NULL,				@ManualJournalVoucherDDPath),
(1101,11,	N'All',		NULL,				@PaymentIssueToNonTradingAgentsDDPath),
(1102,11,	N'All',		NULL,				@GoodServiceIssueToTradeReceivableDDPath),
(1103,11,	N'All',		NULL,				N'accounts'),
(1104,11,	N'All',		NULL,				N'centers'),
(1105,11,	N'All',		NULL,				N'currencies'),
-- 12:Accountant
(1200,12,	N'All',		NULL,				@ManualJournalVoucherDDPath),
(1201,12,	N'All',		NULL,				@PaymentIssueToNonTradingAgentsDDPath),
(1202,12,	N'All',		NULL,				@GoodServiceIssueToTradeReceivableDDPath),
(1203,12,	N'Read',	NULL,				N'accounts'),
(1204,12,	N'Read',	NULL,				N'centers'),
-- 13:Cashier
(1301,13,	N'Update',	N'Agent/UserId = Me or AssigneeId = Me',
											@PaymentReceiptFromNonTradingAgentsDDPath),
(1302,13,	N'Update',	N'AssignedById = Me or AssigneeId = Me',
											@PaymentIssueToNonTradingAgentsDDPath),
-- 20:InternalAuditor
(2000,20,	N'Read',	NULL,				N'all'), -- GM
-- 3:ExtenralAuditor
(2100,21,	N'Read',	NULL,				N'all'), -- GM
-- 99:Public
(9001,99,	N'Read',	NULL,				N'currencies'),-- inbox public permission is hardcoded
(9903,99,	N'Read',	NULL,				N'entry-types'),
(9905,99,	N'Read',	NULL,				N'exchange-rates'),
(9907,99,	N'Read',	NULL,				N'roles'),
(9909,99,	N'Read',	NULL,				N'units'),
(9911,99,	N'Read',	NULL,				N'users'),

(9921,99,	N'Read',	NULL,				N'lookups/@ITEquipmentManufacturerLKD'),
(9922,99,	N'Read',	NULL,				N'lookups/@OperatingSystemLKD'),
(9923,99,	N'Read',	NULL,				N'lookups/@BodyColorLKD'),
(9924,99,	N'Read',	NULL,				N'lookups/@VehicleMakeLKD'),
(9925,99,	N'Read',	NULL,				N'lookups/@SteelThicknessLKD'),
(9926,99,	N'Read',	NULL,				N'lookups/@PapreOriginLKD'),
(9927,99,	N'Read',	NULL,				N'lookups/@PaperGroupLKD'),
(9928,99,	N'Read',	NULL,				N'lookups/@PaperTypeLKD'),
(9929,99,	N'Read',	NULL,				N'lookups/@GrainClassificationLKD'),
(9930,99,	N'Read',	NULL,				N'lookups/@GrainTypeLKD'),
(9931,99,	N'Read',	NULL,				N'lookups/@BankAccountTypeLKD'),

(9951,99,	N'Read',	NULL,				N'resources/@RevenueServiceRD'),
(9961,99,	N'Read',	NULL,				N'resources/@EmployeeBenefitRD'),

(9971,99,	N'Read',	NULL,				N'contracts/@WarehouseCD'),

(9981,99,	N'Update',	N'CreatedById = Me',@PaymentIssueToNonTradingAgentsDDPath)
--(9991,99,	N'Read',	NULL,				N'account-statement'), permission is based on detailentries
;

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
