INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(1000, N'ManualLine', N'Making any accounting adjustment', N'Adjustment', N'Adjustments', 0, 0),
(1020, N'PPEFromIPC', N'Reclassifying investment property as property for use', N'IPC => PPE', N'IPC => PPE', 0, 0),
(1030, N'PPEFromInventory', N'Reclassifying Inventory as property, plant and equipment', N'Inventory => PPE', N'Inventories => PPE', 0, 1),
(1040, N'PPEFromSupplier', N'Receiving property, plant and equipment from supplier, invoiced separately', N'PPE Purchase', N'PPE Purchases', 0, 1),
(1050, N'PPEFromSupplierWithPointInvoice', N'Receiving property, plant and equipment from supplier + point invoice', N'PPE Purchase + Invoice', N'PPE Purchases + Invoices', 0, 1),
(1060, N'CIPFromConstructionExpense', N'Capitalization of costs into construction in progress', N'Cost => CIP', N'Costs => CIP', 0, 0),
(1110, N'IPUCDFromDevelopmentExpense', N'Capitalization of costs into investment property under construction or development', N'Cost => IPCD', N'Costs => IPCD', 0, 0),
(1120, N'InventoryFromPPE', N'Reclassifying property, plant and equipment as inventory', N'PPE => Inventory', N'PPE => Inventories', 0, 0),
(1130, N'InventoryFromIPC', N'Reclassifying investment property as property for sale', N'IPC => Inventory', N'IPC => Inventories', 0, 0),
(1140, N'InventoryTransfer', N'Inventory transfer between warehouses (1-1)', N'Stock Transfer', N'Stock Transfers + Conversions', 0, 0),
(1150, N'InventoryConversion', N'Inventory conversion (1-1)', N'Stock Conversion', N'Stock Conversions', 0, 0),
(1190, N'InventoryFromIIT', N'Receiving inventory from transit', N'Transit => Inventory', N'Transit => Inventories', 0, 0),
(1200, N'InventoryFromSupplier', N'Receiving inventory from supplier/contractor', N'Stock Purchase', N'Stock Purchases', 0, 0),
(1210, N'InventoryFromSupplierWithPointInvoice', N'Receiving inventory from supplier/contractor + point invoice', N'Stock Purchase + Invoice', N'Stock Purchases + Invoices', 0, 0),
(1220, N'IITFromLC', N'Receiving inventory in transit, paid by LC', N'LC => Transit', N'LCs => Transit', 0, 0),
(1225, N'IITFromTransitExpense', N'Capitalization of transit expense into inventories in transit', N'Cost => IIT', N'Costs => IIT', 0, 0),
(1230, N'WIPFromProductionExpense', N'Capitalization of production expense into work in progress', N'Cost => WIP', N'Costs => WIP', 0, 0),
(1270, N'RevenueFromInventory', N'Issuing inventory to customer, invoiced separately', N'Inventory (Sale)', N'Inventories (Sale)', 0, 0),
(1290, N'RevenueFromPeriodService', N'Rendering period services to customer, invoiced separately', N'Lease Out', N'Leases Out', 0, 1),
(1300, N'RevenueFromInventoryWithPointInvoice', N'Issuing inventory to customer + point invoice', N'Inventory (Sale) + Invoice', N'Inventories (Sale) + Invoices', 0, 0),
(1301, N'RevenueFromInventoryWithPointInvoiceFromTemplate', N'Issuing inventory to customer + point invoice (Price List)', N'Inventory (Sale) + Invoice (PL)', N'Inventories (Sale) + Invoices (PL)', 0, 0),
(1310, N'RevenueFromPointServiceWithPointInvoice', N'Rendering point services to customer + point invoice', N'Service (Sale) + Invoice (Point)', N'Services (Sale) + Invoices (Point)', 0, 1),
(1320, N'RevenueFromPeriodServiceWithPeriodInvoice', N'Rendering period services to customer + period invoice', N'Service (Sale) + Invoice (Period)', N'Services (Sale) + Invoices (Period)', 0, 1),
(1350, N'CashFromCustomer', N'Collecting cash from customer/lessee, Invoiced separately', N'Cash Receipt (Sale)', N'Cash Receipts (Sale)', 0, 1),
(1360, N'CashFromCustomerWithWT', N'Collecting cash from customer/lessee with WT, Invoiced separately', N'Cash Receipt + WT', N'Cash Receipts + WT', 0, 1),
(1370, N'CashFromCustomerWithPointInvoice', N'Collecting cash from customer + point invoice', N'Cash Receipt + Point Invoice', N'Cash Receipts + Point Invoices', 0, 1),
(1380, N'CashFromCustomerWithPeriodInvoice', N'Collecting cash from lessee + period invoice', N'Cash Receipt + Period Invoice', N'Cash Receipts + Period Invoices', 0, 1),
(1390, N'CashFromCustomerWithWTWithPointInvoice', N'Collecting cash from customer + WT + point invoice', N'Cash Receipt + WT + Point Invoice', N'Cash Receipts + WT + Point Invoices', 0, 1),
(1400, N'CashFromCustomerWithWTWithPeriodInvoice', N'Collecting cash from lessee + WT + period invoice', N'Cash Receipt + WT + Period Invoice', N'Cash Receipts + WT + Period Invoices', 0, 1),
(1490, N'CashExchange', N'Currency exchange', N'Cash Exchange', N'Cash Exchanges', 0, 1),
(1500, N'CashTransfer', N'Cash transfer, same currency', N'Cash Transfer', N'Cash Transfers', 0, 1),
(1560, N'CashToSalaries', N'Paying cash to employees (payroll)', N'Payment (Payroll)', N'Payments (Payroll)', 0, 0),
(1590, N'CashToSupplierWithPointInvoice', N'Paying cash to supplier/lessor/.. + point invoice', N'Cash Payment + Point Invoice', N'Cash Payments + Point Invoices', 0, 1),
(1600, N'CashToSupplierWithPeriodInvoice', N'Paying cash to supplier/lessor/.. + period invoice', N'Cash Payment + Period Invoice', N'Cash Payments + Period Invoices', 0, 1),
(1610, N'CashToSupplierWithPointInvoiceWithWT', N'Paying cash to supplier/lessor/.. + point invoice + WT', N'Cash Payment + Point Invoice + WT', N'Cash Payments + Point Invoices + WT', 0, 1),
(1630, N'CashToSupplier', N'Paying cash to supplier/lessor/.., invoiced separately', N'Payment (Purchase)', N'Payments (Purchase)', 0, 1),
(1660, N'SupplierWT', N'WT from supplier', N'WT (Purchase)', N'WT (Purchases)', 0, 1),
(1700, N'PointExpenseFromInventory', N'Issuing inventory to cost center (maintenance, job order, production line, construction project..)', N'Stock Consumption', N'Stock Consumptions', 0, 0),
(1710, N'PointExpenseFromSupplier', N'Receiving consumables and point services from supplier, invoiced separately', N'C/S Purchase', N'C/S Purchases', 0, 0),
(1720, N'PeriodExpenseFromSupplier', N'Receiving period service from supplier, invoiced separately', N'Lease In', N'Leases In', 0, 1),
(1730, N'ExpenseFromSupplierWithInvoice', N'Receiving Point/Period service from supplier + Point/Period invoice', N'C/S Purchase + Invoice', N'C/S Purchases + Invoices', 0, 1),
(1750, N'SalaryTransportationOtherOvertimePension', N'Salary, transportation allowance, other allowances, overtime, pension', N'Salary v1', N'Salaries v1', 0, 0),
(1760, N'SalaryTransportationOvertimePenaltyPensionProvidentCostSharing', N'Salary, transportation allowance, overtime, penalty, pension, provident, cost sharing', N'Salary v2', N'Salaries v2', 0, 0),
(1770, N'Overtime', N'Receiving overtime from employees', N'Overtime', N'Overtimes', 0, 0),
(1780, N'DepreciationPPE', N'Depreciating assets that are time based, and using the number of days as criteria', N'Depreciation (PPE)', N'Depreciations (PPE)', 0, 0),
(1810, N'AnnualLeaveAllowance', N'Yearly allowance for paid leaves', N'Leave Allowance', N'Leave Allowances', 0, 0),
(1820, N'AnnualLeaveUsage', N'Usage of paid leave', N'Paid Leave', N'Paid Leaves', 0, 1),
(1830, N'UnpaidLeave', N'Usage of unpaid leave', N'Unpaid Leave', N'Unpaid Leaves', 0, 1),
(1850, N'TradeSettlement', N'Adjusting trade payables and trade receivables balances', N'Settlement', N'Settlements', 0, 0),
(1870, N'TaskAssignment', N'Tracking task assignments', N'Assignment', N'', 1, 1);
--1000: ManualLine
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Direction], [ParentAccountTypeId]) VALUES (0,1000,+1, @StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,1000,	N'AccountId',	0,			N'Account',		4,4,0), -- together with properties
(1,1000,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,1000,	N'Value',		0,			N'Credit',		4,4,0),
(3,1000,	N'Memo',		0,			N'Memo',		4,4,1);
--1040: PPEFromSupplier, appears in Purchase vouchers, with purchases of inventory, Investment property, and C/S 
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	DECLARE @FunctionalCurrencyId = dbo.fn_FunctionalCurrencyId();
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= ISNULL([CurrencyId0], @FunctionalCurrencyId), -- overridden by resource/currency
		[CurrencyId1]		= ISNULL([CurrencyId1], @FunctionalCurrencyId), -- overridden by resource/currency
		[CenterId0]			= ISNULL([CenterId0], [CenterId2]),
		[CenterId1]			= ISNULL([CenterId1], [CenterId2]),
		[CustodianId1]		= [CustodianId0], -- Same custodian for both standard and pure
		[MonetaryValue0]	= dbo.fn_[bll].[fn_ConvertCurrencies]([PostingDate] , [CurrencyId2],
								ISNULL([CurrencyId0], @FunctionalCurrencyId), 
								[NotedAmount0]),
		[MonetaryValue1]	= dbo.fn_[bll].[fn_ConvertCurrencies]([PostingDate] , [CurrencyId2],
								ISNULL([CurrencyId1], @FunctionalCurrencyId),
								ISNULL([MonetaryValue2],0) - ISNULL([NotedAmount0],0), -- <==  Depreciable value in Supplier currency
								),
		[ResourceId1]		= [ResourceId0],
		[Quantity0]			= 1,
		[UnitId0]			= (SELECT MIN([Id]) FROM dbo.Units WHERE [UnitType] = N''Pure''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [Participant2])	
'
WHERE [Index] = 1040;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],			[EntryTypeId]) VALUES
(0,1040,+1,	@PropertyPlantAndEquipment,	@AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment),
(1,1040,+1,	@PropertyPlantAndEquipment,	@AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment),
(2,1040,-1,	@ReceiptsAtPointInTimeFromSuppliersControlExtension,NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader], [Filter]) VALUES
(0,1040,	N'Memo',				1,	N'Memo',			1,4,1,NULL), -- Document Header
(1,1040,	N'ParticipantId',		2,	N'Supplier',		3,4,1,NULL), -- Document Header. TODO: Participant Id Is Common
(2,1040,	N'CustodianId',			0,	N'Custodian',		5,5,0,NULL), -- TODO: No CustodyDefinitions for PPEs. Just Custodian Definition
(3,1040,	N'ResourceId',			0,	N'Fixed Asset',		2,4,0,NULL),
(4,1040,	N'Quantity',			1,	N'Life/Usage',		2,4,0,NULL),
(5,1040,	N'UnitId',				1,	N'Unit',			2,4,0,NULL),
(6,1040,	N'CurrencyId',			2,	N'Currency',		1,2,1,NULL), -- Document Header, Supplier's currency
(7,1040,	N'MonetaryValue',		2,	N'Cost (VAT Excl.)',1,2,0,NULL), -- In Supplier's currency
(8,1040,	N'NotedAmount',			0,	N'Residual Value',	1,2,0,NULL), -- In Supplier's currency
(10,1040,	N'PostingDate',			1,	N'Purchase Date',0,4,1,NULL), -- Document Header, shared with other purchases
(11,1040,	N'CenterId',			2,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''); -- Document Heade
--1140:InventoryTransfer, appears in SIV, with issue to expenditure, (to sale is either in CSV or CRSV)
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE PWL
	SET
		[CurrencyId0]		= R.[CurrencyId],
		[CurrencyId1]		= R.[CurrencyId],
		[ResourceId0]		= PWL.[ResourceId1], [Quantity0] = PWL.[Quantity1], [UnitId0] = PWL.[UnitId1],
		[MonetaryValue0]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
							),
		-- Assuming that, for foreign currency, value is credit at same ratio as Monetary Value
		[Value0]			= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
							),
		[MonetaryValue1]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
							),
		[Value1]			= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
							),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = PWL.[CustodyId1]),
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = PWL.[CustodyId0])
	FROM @ProcessedWideLines PWL
	LEFT JOIN [bll].[fi_InventoryAverageCosts] (@ProcessedWideLines) RC ON PWL.[ResourceId1] = RC.[ResourceId] AND PWL.[CustodyId1] = RC.[CustodyId] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
',
[ValidateScript] = N'
 INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		''['' + CAST(FE.[Index] AS NVARCHAR (255)) + ''].Lines['' + CAST(L.[Index]  AS NVARCHAR(255)) + ''].Entries[0].CustodyId'',
		dbo.fn_Localize(N''Must transfer to a different warehouse'', NULL, NULL) AS ErrorMessage
	FROM @Documents FE
	JOIN @Lines L ON L.[DocumentIndex] = FE.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.DocumentIndex = L.DocumentIndex
	GROUP BY FE.[Index], L.[Index], E.[CustodyId]
	HAVING COUNT(DISTINCT E.[CustodyId]) = 1
'
WHERE [Index] = 1140;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],	[EntryTypeId]) VALUES
(0,1140,+1,	@Inventories,		@InternalInventoryTransferExtension),
(1,1140,-1,	@Inventories,		NULL);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,1140,@WarehouseCD),
(0,1,1140,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1140,	N'Memo',				1,	N'Memo',			1,4,1), -- Document Header
(1,1140,	N'CustodyId'	,		1,	N'From Warehouse',	3,4,1), -- Document Header. -- Warehouse is a custody, because one employee 
(2,1140,	N'CustodyId',			0,	N'To Warehouse',	3,4,1), -- Tab Header		-- might be in charge of many warehouses  
(3,1140,	N'ResourceId',			0,	N'Item',			2,4,0),						-- containing same items
(4,1140,	N'Quantity',			0,	N'Qty',				2,4,0),
(5,1140,	N'UnitId',				0,	N'Unit',			2,4,0),
(6,1140,	N'PostingDate',			1,	N'Issue Date',		1,4,1); -- Document Header
--1200:InventoryFromSupplier, appears in Purchase vouchers, with purchases of PPE, Investment property, and C/S. (SRV is from non purchases)
UPDATE @LineDefinitions
SET
	[PreprocessScript] = N'
	DECLARE @FunctionalCurrencyId = dbo.fn_FunctionalCurrencyId();
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= ISNULL([CurrencyId0], @FunctionalCurrencyId), -- overridden by resource/currency
		[CenterId0]			= ISNULL([CenterId0], [CenterId1]), -- Overridden by Custody/Business Unit
		[MonetaryValue0]	=  dbo.fn_[bll].[fn_ConvertCurrencies]([PostingDate] , [CurrencyId1],
								ISNULL([CurrencyId0], @FunctionalCurrencyId), 
								[MonetaryValue1]),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [Participant1])',
	[ValidateScript] = NULL
WHERE [Index] = 1200;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],										[EntryTypeId]) VALUES
(0,1200,+1,	@Inventories,											@AdditionsFromPurchasesInventoriesExtension),
(1,1200,-1,	@ReceiptsAtPointInTimeFromSuppliersControlExtension,	NULL);
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,0,1200,@MerchandiseRD),
(1,0,1200,@CurrentFoodAndBeverageRD),
(2,0,1200,@CurrentAgriculturalProduceRD),
(3,0,1200,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(4,0,1200,@RawMaterialsRD),
(5,0,1200,@ProductionSuppliesRD),
(6,0,1200,@CurrentPackagingAndStorageMaterialsRD),
(7,0,1200,@SparePartsRD),
(8,0,1200,@CurrentFuelRD),
(9,0,1200,@OtherInventoriesRD),
(10,0,1200,@TradeMedicineRD),
(11,0,1200,@TradeConstructionMaterialRD),
(12,0,1200,@TradeSparePartRD),
(13,0,1200,@RawGrainRD),
(14,0,1200,@RawVehicleRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,1200,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader], [Filter]) VALUES
(0,1200,	N'Memo',				1,	N'Memo',			1,4,1,NULL), -- Document Header
(1,1200,	N'ParticipantId',		1,	N'Supplier',		3,4,1,NULL), -- Document Header
(2,1200,	N'CustodyId',			0,	N'Warehouse',		3,4,1,NULL), -- Tab Header
(3,1200,	N'ResourceId',			0,	N'Item',			2,4,0,NULL),
(4,1200,	N'Quantity',			0,	N'Qty',				2,4,0,NULL),
(5,1200,	N'UnitId',				0,	N'Unit',			2,4,0,N'UnitType<>''Time'''),
(6,1200,	N'CurrencyId',			1,	N'Currency',		1,2,1,NULL), -- Document Header, Supplier's currency
(7,1200,	N'MonetaryValue',		1,	N'Cost (VAT Excl.)',1,2,0,NULL), -- In Supplier's currency
(8,1200,	N'PostingDate',			1,	N'Purchase Date',	1,4,1,NULL),
(9,1200,	N'CenterId',			1,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''); -- Document Header, ignored for cash purchase. useful for credit purchase
--1270:RevenueFromInventory, appears in CSV and CRSV, but not in SRV
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE PWL
	SET
		[CustodyId0]		= PWL.[CustodyId1], -- Warehouse in COS account
		[CustodyId3]		= PWL.[CustodyId1], -- Warehouse in Revenue account
		[CurrencyId0]		= ISNULL(R.[CurrencyId], PWL.[CurrencyId2]),
		[CurrencyId1]		= ISNULL(R.[CurrencyId], PWL.[CurrencyId2]),
		[CurrencyId3]		= ISNULL(R.[CurrencyId], PWL.[CurrencyId2]), -- revenues is calculated in resource currency
		[CenterId1]			= ISNULL(C.[CenterId], PWL.[CenterId2]),
		[CenterId0]			= ( -- cost of sale center
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((
									SELECT [Node] FROM dbo.Centers
									WHERE [Id] = ISNULL(C.[CenterId], PWL.[CenterId2])
								)) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'')
								AND [IsLeaf] = 1
							),
		[CenterId3]			= ( -- cost of sale cenrer
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((
									SELECT [Node] FROM dbo.Centers
									WHERE [Id] = ISNULL(C.[CenterId], PWL.[CenterId2])
								)) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'')
								AND [IsLeaf] = 1
							),
		[ResourceId0]		= PWL.[ResourceId1], [Quantity0] = PWL.[Quantity1], [UnitId0] = PWL.[UnitId1],
		[ResourceId3]		= PWL.[ResourceId1], [Quantity3] = PWL.[Quantity1],	[UnitId3] = PWL.[UnitId1],
		[MonetaryValue0]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		-- Assuming that, for foreign currency, value is credit at same ratio as Monetary Value
		[Value0]			= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue1]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[Value1]			= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue2]	= [MonetaryValue3],
		[NotedRelationId0]	= [ParticipantId3],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [ParticipantId3]),
		[NotedRelationId2]	= [NotedRelationId3]
	FROM @ProcessedWideLines PWL
	LEFT JOIN [bll].[fi_InventoryAverageCosts] (@ProcessedWideLines) RC ON PWL.[ResourceId1] = RC.[ResourceId] AND PWL.[CustodyId1] = RC.[CustodyId] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.[Custodies] C ON C.[Id] = PWL.[CustodyId1]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
'
WHERE [Index] = 1270;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],									[EntryTypeId]) VALUES
(0,1270,+1,	@CostOfMerchandiseSold,								NULL),
(1,1270,-1,	@Inventories,										@InventoriesIssuesToSaleExtension),
(2,1270,+1,	@IssuesAtPointInTimeToCustomersControlExtension,	NULL),
(3,1270,-1,	@Revenue,											NULL)
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,1,1270,@MerchandiseRD),
(1,1,1270,@CurrentFoodAndBeverageRD),
(2,1,1270,@CurrentAgriculturalProduceRD),
(3,1,1270,@FinishedGoodsRD),
(4,1,1270,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(5,1,1270,@TradeMedicineRD),
(6,1,1270,@TradeConstructionMaterialRD),
(7,1,1270,@TradeSparePartRD),
(8,1,1270,@FinishedGrainRD),
(9,1,1270,@ByproductGrainRD),
(10,1,1270,@FinishedVehicleRD),
(11,1,1270,@FinishedOilRD),
(12,1,1270,@ByproductOilRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1270,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1270,	N'Memo',				1,	N'Memo',			1,4,1,NULL), -- Document Header
(1,1270,	N'ParticipantId',		2,	N'Customer',		3,4,1,NULL), -- Document Header
(2,1270,	N'CustodyId',			1,	N'Warehouse',		3,4,1,NULL), -- Tab Header
(3,1270,	N'ResourceId',			1,	N'Item',			2,4,0,NULL),
(4,1270,	N'Quantity',			1,	N'Qty',				2,4,0,NULL),
(5,1270,	N'UnitId',				1,	N'Unit',			2,4,0,N'UnitType<>''Time'''),
(6,1270,	N'CurrencyId',			2,	N'Currency',		1,2,1,NULL), -- Document Header, Customer's currency
(7,1270,	N'MonetaryValue',		3,	N'Price (VAT Excl.)',1,2,0,NULL), -- In Customer's currency
(8,1270,	N'PostingDate',			1,	N'Sale Date',		1,4,1,NULL),
(9,1270,	N'CenterId',			2,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''); -- Document Header, ignored for cash sale. useful for credit purchase
--1300:RevenueFromInventoryWithPointInvoice, appears in cash sales and credit sales, in addition to Revenue from 
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
UPDATE PWL
	SET
		[CustodyId0]		= PWL.[CustodyId1],
		[CustodyId4]		= PWL.[CustodyId1],
		[CurrencyId0]		= R.[CurrencyId],
		[CurrencyId1]		= R.[CurrencyId],
		[CurrencyId3]		= PWL.[CurrencyId2],
		[CurrencyId4]		= PWL.[CurrencyId2],
		[CenterId1]			= COALESCE(PWL.[CenterId1], PWL.[CenterId2]),
		[CenterId0]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[CenterId3]			= PWL.[CenterId2],
		[CenterId4]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[ResourceId0]		= PWL.[ResourceId1], [Quantity0] = PWL.[Quantity1], [UnitId0] = PWL.[UnitId1],
		[ResourceId4]		= PWL.[ResourceId1], [Quantity4] = PWL.[Quantity1],	[UnitId4] = PWL.[UnitId1],
		[MonetaryValue0]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		-- Assuming that, for foreign currency, value is credit at same ratio as Monetary Value
		[Value0]			= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue1]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[Value1]			= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue2]	= ISNULL([MonetaryValue4],0) * ( 1 + R.[VatRate]), -- Total Due
		[MonetaryValue3]	= ISNULL([MonetaryValue4],0) * R.[VatRate], -- VAT
		[NotedAmount3]		= ISNULL([MonetaryValue4],0), -- Revenues, Taxable Amount
		[NotedRelationId0]	= [NotedRelationId4],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = PWL.[NotedRelationId4]),
		[ParticipantId2]	= [NotedRelationId4],
		[NotedRelationId3]	= [NotedRelationId4]
	FROM @ProcessedWideLines PWL
	LEFT JOIN [bll].[fi_InventoryAverageCosts] (@ProcessedWideLines) RC ON PWL.[ResourceId1] = RC.[ResourceId] AND PWL.[CustodyId1] = RC.[CustodyId] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
'
WHERE [Index] = 1300;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],									[EntryTypeId]) VALUES
(0,1300,+1,	@CostOfMerchandiseSold,								NULL),
(1,1300,-1,	@Inventories,										@InventoriesIssuesToSaleExtension),
(2,1300,+1,	@CashReceiptsFromCustomersControlExtension,			NULL),
(3,1300,-1,	@CurrentValueAddedTaxPayables,						NULL),
(4,1300,-1,	@Revenue,											NULL)
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,1,1300,@MerchandiseRD),
(1,1,1300,@CurrentFoodAndBeverageRD),
(2,1,1300,@CurrentAgriculturalProduceRD),
(3,1,1300,@FinishedGoodsRD),
(4,1,1300,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(5,1,1300,@TradeMedicineRD),
(6,1,1300,@TradeConstructionMaterialRD),
(7,1,1300,@TradeSparePartRD),
(8,1,1300,@FinishedGrainRD),
(9,1,1300,@ByproductGrainRD),
(10,1,1300,@FinishedVehicleRD),
(11,1,1300,@FinishedOilRD),
(12,1,1300,@ByproductOilRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1300,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1300,	N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1300,	N'NotedRelationId',		4,	N'Customer',		3,4,1,NULL),
(2,1300,	N'CustodyId',			1,	N'Warehouse',		3,4,1,NULL),
(3,1300,	N'ResourceId',			1,	N'Item',			2,4,0,NULL),
(4,1300,	N'Quantity',			1,	N'Qty',				2,4,0,NULL),
(5,1300,	N'UnitId',				1,	N'Unit',			2,4,0,N'UnitType<>''Time'''),
(6,1300,	N'CurrencyId',			2,	N'Currency',		1,2,1,NULL),
(7,1300,	N'MonetaryValue',		4,	N'Price (VAT Excl.)',1,2,0,NULL),
(8,1300,	N'MonetaryValue',		3,	N'VAT',				0,0,0,NULL),
(9,1300,	N'MonetaryValue',		2,	N'Line Total',		0,0,0,NULL),
(10,1300,	N'ExternalReference',	2,	N'Invoice #',		1,4,0,NULL),
(11,1300,	N'PostingDate',			1,	N'Sale Date',		1,4,1,NULL),
(13,1300,	N'CenterId',			2,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit''');
--1301:RevenueFromInventoryWithPointInvoiceFromTemplate
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
DECLARE @LineDefinitionId INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N''RevenueFromInventoryWithPointInvoiceTemplate'');
WITH Prices AS (
	SELECT
	L.PostingDate AS ValidFrom,
	E.ResourceId,
	--@Quantity / E.Quantity * [bll].[fn_ConvertUnits](@UnitId, E.[UnitId]) AS Multiplier,
	E.[MonetaryValue], -- VAT exclusive
	E.[Quantity],
	E.[UnitId],
	E.[CurrencyId]
	FROM dbo.Lines L
	JOIN dbo.Entries E ON L.[Id] = E.[LineId]
	WHERE L.[DefinitionId] = @LineDefinitionId
	AND L.[State] > 0
	AND E.[Index] = 0
)
UPDATE PWL
	SET
		[CustodyId0]		= PWL.[CustodyId1],
		[CustodyId4]		= PWL.[CustodyId1],
		[CurrencyId0]		= R.[CurrencyId],
		[CurrencyId1]		= R.[CurrencyId],
		[CurrencyId2]		= R.[CurrencyId],
		[CurrencyId3]		= R.[CurrencyId],
		[CurrencyId4]		= R.[CurrencyId],
		[CenterId0]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[CenterId1]			= COALESCE(PWL.[CenterId1], PWL.[CenterId2]),
		[CenterId3]			= PWL.[CenterId2],
		[CenterId4]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[ResourceId0]		= PWL.[ResourceId1], [Quantity0] = PWL.[Quantity1], [UnitId0] = P.[UnitId],
		[ResourceId4]		= PWL.[ResourceId1], [Quantity4] = PWL.[Quantity1], [UnitId4] = P.[UnitId],
		[UnitId1]		= P.[UnitId],
		[MonetaryValue0]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		-- Assuming that, for foreign currency, value is credit at same ratio as Monetary Value
		[Value0]			= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue1]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[Value1]			= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
--		[MonetaryValue2]	= (P.[MonetaryValue] *  [bll].[fn_ConvertUnits](PWL.[UnitId1], P.[UnitId]) * [Quantity1] / P.Quantity) * ( 1 + R.[VatRate]), -- Total Due
		[MonetaryValue2]	= (P.[MonetaryValue] * [Quantity1] / P.Quantity) * ( 1 + R.[VatRate]), -- Total Due
		[MonetaryValue3]	= (P.[MonetaryValue] * [Quantity1] / P.Quantity) * R.[VatRate], -- VAT
		[MonetaryValue4]	= (P.[MonetaryValue] * [Quantity1] / P.Quantity),
		[NotedAmount3]		= (P.[MonetaryValue] * [Quantity1] / P.Quantity),
		[NotedRelationId0]	= [NotedRelationId4],
		[NotedRelationId3]	= [NotedRelationId4],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = PWL.[NotedRelationId4]),
		[ParticipantId2]	= [NotedRelationId4]
	FROM @ProcessedWideLines PWL
	LEFT JOIN Prices P ON PWL.[ResourceId1] = P.[ResourceId]
	LEFT JOIN [bll].[fi_InventoryAverageCosts] (@ProcessedWideLines) RC ON PWL.[ResourceId1] = RC.[ResourceId] AND PWL.[CustodyId1] = RC.[CustodyId] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
'
WHERE [Index] = 1301;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],									[EntryTypeId]) VALUES
(0,1301,+1,	@CostOfMerchandiseSold,								NULL),
(1,1301,-1,	@Inventories,										@InventoriesIssuesToSaleExtension),
(2,1301,+1,	@CashReceiptsFromCustomersControlExtension,			NULL),
(3,1301,-1,	@CurrentValueAddedTaxPayables,						NULL),
(4,1301,-1,	@Revenue,											NULL)
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,1,1301,@MerchandiseRD),
(1,1,1301,@CurrentFoodAndBeverageRD),
(2,1,1301,@CurrentAgriculturalProduceRD),
(3,1,1301,@FinishedGoodsRD),
(4,1,1301,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1301,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1301,	N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1301,	N'NotedRelationId',		4,	N'Customer',		3,4,1,NULL),
(2,1301,	N'CustodyId',			1,	N'Warehouse',		3,4,1,NULL),
(3,1301,	N'ResourceId',			1,	N'Item',			2,4,0,NULL),
(4,1301,	N'Quantity',			1,	N'Qty',				2,4,0,NULL),
(5,1301,	N'UnitId',				1,	N'Unit',			0,0,0,NULL),
(6,1301,	N'CurrencyId',			2,	N'Currency',		0,0,1,NULL),
(7,1301,	N'MonetaryValue',		4,	N'Price (VAT Excl.)',0,0,0,NULL),
(8,1301,	N'MonetaryValue',		3,	N'VAT',				0,0,0,NULL),
(9,1301,	N'MonetaryValue',		2,	N'Line Total',		0,0,0,NULL),
(10,1301,	N'ExternalReference',	2,	N'Invoice #',		1,4,0,NULL),
(11,1301,	N'PostingDate',			1,	N'Sale Date',		1,4,1,NULL),
(12,1301,	N'CenterId',			2,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit''');
--1350:CashFromCustomer
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
 UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= COALESCE([CurrencyId0], [CurrencyId1]),
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue1]	= ISNULL([MonetaryValue1], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue1], 0),
		[ParticipantId1]	= [NotedRelationId0],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId0]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1350;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId],							[EntryTypeId]) VALUES
(0,1350,+1,		@CashAndCashEquivalents,					@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,1350,-1,		@CashReceiptsFromCustomersControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1350,	N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1350,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0,NULL),
(2,1350,	N'MonetaryValue',		1,	N'Amount',			1,2,0,NULL), 
(3,1350,	N'CurrencyId',			1,	N'Currency',		1,2,1,NULL),
(4,1350,	N'NotedRelationId',		0,	N'Customer',		1,4,1,NULL),
(5,1350,	N'PostingDate',			1,	N'Received On',		1,4,1,NULL),
(6,1350,	N'ExternalReference',	0,	N'Check #',			5,5,0,NULL),
(7,1350,	N'CenterId',			1,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''),
(8,1350,	N'AdditionalReference',	0,	N'CRV #',			5,5,0,NULL);
--1360:CashFromCustomerWithWT
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
 UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[CurrencyId0]		= [CurrencyId2],
		[CenterId1]		= [CenterId2],
		[CenterId0]		= COALESCE([CenterId0], [CenterId2]),
		[MonetaryValue2]	= ISNULL([MonetaryValue2], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue2], 0) - ISNULL([MonetaryValue1], 0),
		[NotedRelationId1]	= [NotedRelationId2],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId2]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0]) 
'
WHERE [Index] = 1360;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId],							[EntryTypeId]) VALUES
(0,1360,+1,		@CashAndCashEquivalents,					@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,1360,+1,		@WithholdingTaxReceivablesExtension,		NULL),
(2,1360,-1,		@CashReceiptsFromCustomersControlExtension,	NULL); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1360,	N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1360,	N'NotedRelationId',		2,	N'Customer',		1,4,1,NULL),
(2,1360,	N'CurrencyId',			2,	N'Currency',		1,2,1,NULL),
(3,1360,	N'MonetaryValue',		2,	N'Due Amount',		1,2,0,NULL), -- 
(6,1360,	N'MonetaryValue',		1,	N'Amount Withheld',	4,4,0,NULL),
(7,1360,	N'ExternalReference',	1,	N'WT Voucher #',	5,5,0,NULL),
(8,1360,	N'MonetaryValue',		0,	N'Net To Receive',	0,0,0,NULL),
(9,1360,	N'ExternalReference',	0,	N'Check #',			5,5,0,NULL),
(10,1360,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0,NULL),
(11,1360,	N'PostingDate',			2,	N'Payment Date',	1,2,1,NULL),
(12,1360,	N'CenterId',			2,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''),
(13,1360,	N'AdditionalReference',	0,	N'CRV #',			5,5,0,NULL);
--1370:CashFromCustomerWithWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[CurrencyId0]		= [CurrencyId2],
		[CenterId1]			= [CenterId2],
		[CenterId0]			= COALESCE([CenterId0], [CenterId2]),
		[MonetaryValue2]	= ISNULL([MonetaryValue2], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue2], 0) + ISNULL([MonetaryValue1], 0),
		[NotedAmount1]		= ISNULL([MonetaryValue2], 0),
		[NotedRelationId1]	= [NotedRelationId2],
	--	[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId2]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1370;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId],								[EntryTypeId]) VALUES
(0,1370,+1,		@CashAndCashEquivalents,						@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,1370,-1,		@CurrentValueAddedTaxPayables,					NULL),
(2,1370,-1,		@IssuesAtPointInTimeToCustomersControlExtension,NULL); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1370,	N'Memo',				0,	N'Memo',			1,4,1,NULL),
(1,1370,	N'NotedRelationId',		2,	N'Customer',		1,4,1,NULL),
(2,1370,	N'CurrencyId',			2,	N'Currency',		1,2,1,NULL),
(3,1370,	N'MonetaryValue',		2,	N'Amount (VAT Excl)',1,2,0,NULL), -- 
(4,1370,	N'MonetaryValue',		1,	N'VAT',				0,0,0,NULL),
(5,1370,	N'ExternalReference',	1,	N'Invoice #',		1,4,0,NULL),
(8,1370,	N'MonetaryValue',		0,	N'Net To Receive',	0,0,0,NULL),
(9,1370,	N'ExternalReference',	0,	N'Check #',			5,5,0,NULL),
(10,1370,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0,NULL),
(11,1370,	N'PostingDate',			2,	N'Payment Date',	1,2,1,NULL),
(12,1370,	N'CenterId',			2,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''),
(13,1370,	N'AdditionalReference',	0,	N'CRV #',			5,5,0,NULL);
--1390:CashFromCustomerWithWTWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId2]		= [CurrencyId3],
		[CurrencyId1]		= [CurrencyId3],
		[CurrencyId0]		= [CurrencyId3],
		[CenterId2]			= [CenterId3],
		[CenterId1]			= [CenterId3],
		[CenterId0]			= COALESCE([CenterId0], [CenterId3]),
		[MonetaryValue3]	= ISNULL([MonetaryValue3], 0),
		[MonetaryValue2]	= 0.15 * [MonetaryValue3], --ISNULL([MonetaryValue2], 0),
		[MonetaryValue1]	= IIF(ISNUMERIC([ExternalReference1]) = 1 AND [ExternalReference1] <> N''-'', 0.02 * [MonetaryValue3], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue3], 0) + ISNULL([MonetaryValue2], 0) - 
								IIF(ISNUMERIC([ExternalReference1]) = 1 AND [ExternalReference1] <> N''-'', 0.02 * [MonetaryValue3], 0),
		--[ExternalReference1]= ISNULL([ExternalReference1], N''--''),
		[NotedAmount2]		= ISNULL([MonetaryValue3], 0),
		[NotedAmount1]		= ISNULL([MonetaryValue3], 0),
		[NotedRelationId2]	= [NotedRelationId3],
		[NotedRelationId1]	= [NotedRelationId3],
		-- Entry Type may change depending on nature of items
		[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId3]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1390;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId]) VALUES
(0,1390,+1,		@CashAndCashEquivalents),
(1,1390,+1,		@WithholdingTaxReceivablesExtension),
(2,1390,-1,		@CurrentValueAddedTaxPayables),
(3,1390,-1,		@IssuesAtPointInTimeToCustomersControlExtension); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1390,	N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1390,	N'NotedRelationId',		3,	N'Customer',		1,4,1,NULL),
(2,1390,	N'CurrencyId',			3,	N'Currency',		1,2,1,NULL),
(3,1390,	N'MonetaryValue',		3,	N'Amount (VAT Excl.)',1,2,0,NULL), -- 
(4,1390,	N'MonetaryValue',		2,	N'VAT',				0,0,0,NULL),
(5,1390,	N'ExternalReference',	2,	N'Invoice #',		1,4,0,NULL),
(6,1390,	N'MonetaryValue',		1,	N'Amount Withheld',	4,4,0,NULL),
(7,1390,	N'ExternalReference',	1,	N'WT Voucher #',	5,5,0,NULL),
(8,1390,	N'MonetaryValue',		0,	N'Net To Receive',	0,0,0,NULL),
(9,1390,	N'ExternalReference',	0,	N'Check #',			5,5,0,NULL),
(10,1390,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0,NULL),
(11,1390,	N'PostingDate',			3,	N'Payment Date',	1,2,1,NULL),
(12,1390,	N'CenterId',			3,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''),
(13,1390,	N'AdditionalReference',	0,	N'CRV #',			5,5,0,NULL);
--1490:CashExchange
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]),
		[CenterId0] = COALESCE([CenterId0], [CenterId2]),
		[CenterId1] = COALESCE([CenterId1], [CenterId2]),
		[CurrencyId2] = dbo.fn_FunctionalCurrencyId(),
		[MonetaryValue0] = IIF([CurrencyId0]=[CurrencyId1],[MonetaryValue1],[MonetaryValue0]),
		[MonetaryValue2] = wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId1], [MonetaryValue1])
							- wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId0], [MonetaryValue0]) 
'
WHERE [Index] = 1490;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [ParentAccountTypeId],[EntryTypeId]) VALUES
(0,1490,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,1490,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(2,1490,+1,	@GainsLossesOnExchangeDifferencesOnTranslationBeforeTax, NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1490,	N'CustodyId',			1,	N'From Account',	1,2,0,NULL),
(1,1490,	N'CustodyId',			0,	N'To Account',		1,2,0,NULL),
(2,1490,	N'CurrencyId',			1,	N'From Currency',	1,2,0,NULL),
(3,1490,	N'CurrencyId',			0,	N'To Currency',		1,2,0,NULL),
(4,1490,	N'MonetaryValue',		1,	N'From Amount',		1,3,0,NULL),
(5,1490,	N'MonetaryValue',		0,	N'To Amount',		1,3,0,NULL),
(6,1490,	N'CenterId',			2,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''),
(7,1490,	N'Memo',				0,	N'Memo',			1,2,1,NULL),
(8,1490,	N'PostingDate',			0,	N'Transfer Date',	1,4,1,NULL);
--1500:CashTransfer
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]),
		[MonetaryValue0] = ISNULL([MonetaryValue1],0)
		-- Currency and Center are set by custody
'
WHERE [Index] = 1500;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [ParentAccountTypeId],[EntryTypeId]) VALUES
(0,1500,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,1500,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1500,	N'CustodyId',			1,	N'From Account',	1,2,0),
(1,1500,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(3,1500,	N'CustodyId',			0,	N'To Account',		1,2,0),
(4,1500,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(6,1500,	N'MonetaryValue',		1,	N'Amount',			1,2,0),
(7,1500,	N'Memo',				0,	N'Memo',			1,3,1),
(8,1500,	N'PostingDate',			0,	N'Transfer Date',	0,4,1);
--1590:CashToSupplierWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[CurrencyId2]		= [CurrencyId0],
		[CenterId1]			= [CenterId0],
		[CenterId2]			= COALESCE([CenterId2], [CenterId0]),
		[MonetaryValue0]	= ISNULL([MonetaryValue0], 0),
		[MonetaryValue1]	= ISNULL([MonetaryValue1], 0),
		[MonetaryValue2]	= ISNULL([MonetaryValue0], 0) + ISNULL([MonetaryValue1], 0),
		[NotedAmount1]		= ISNULL([MonetaryValue0], 0),
		[NotedRelationId1]	= [NotedRelationId0],
		-- Entry Type may change depending on nature of items
		[EntryTypeId2]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''PaymentsToSuppliersForGoodsAndServices''),
		[NotedAgentName2]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId0]),
		[AdditionalReference2] = IIF(ISNUMERIC([AdditionalReference2]) = 1, N''CPV'' + [AdditionalReference2], [AdditionalReference2])
'
WHERE [Index] = 1590;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId]) VALUES
(0,1590,+1,		@ReceiptsAtPointInTimeFromSuppliersControlExtension),
(1,1590,+1,		@CurrentValueAddedTaxReceivables),
(2,1590,-1,		@CashAndCashEquivalents); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1590,N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1590,N'NotedRelationId',		0,	N'Supplier',		1,4,1,NULL),
(2,1590,N'CurrencyId',			0,	N'Currency',		1,2,1,NULL),
(3,1590,N'MonetaryValue',		0,	N'Amount (VAT Excl)',1,2,0,NULL),
(4,1590,N'MonetaryValue',		1,	N'VAT',				1,4,0,NULL),
(5,1590,N'ExternalReference',	1,	N'Invoice #',		1,4,0,NULL),
(6,1590,N'MonetaryValue',		2,	N'Net To Pay',		0,0,0,NULL),
(8,1590,N'ExternalReference',	2,	N'Check #',			5,5,0,NULL),
(9,1590,N'CustodyId',			2,	N'Cash/Bank Acct',	4,4,0,NULL),
(10,1590,N'PostingDate',		0,	N'Payment Date',	1,2,1,NULL),
(11,1590,N'CenterId',			0,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''),
(12,1590,N'AdditionalReference',2,	N'CPV #',			1,4,0,NULL);
--1610:CashToSupplierWithPointInvoiceWithWT CashPaymentToTradePayableWithWT: (basically, it is the VAT) -- assume all in same currency
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[CurrencyId2]		= [CurrencyId0],
		[CurrencyId3]		= [CurrencyId0],
		[CenterId1]			= [CenterId0],
		[CenterId2]			= [CenterId0],
		[CenterId3]			= COALESCE([CenterId3], [CenterId0]),
		[MonetaryValue0]	= ISNULL([MonetaryValue0], 0),
		[MonetaryValue1]	= ISNULL([MonetaryValue1], 0),
		[MonetaryValue2]	= IIF(ISNUMERIC([ExternalReference2]) = 1 AND [ExternalReference2] <> N''-'', 0.02 * [MonetaryValue0], 0),
		[MonetaryValue3]	= ISNULL([MonetaryValue0], 0) + ISNULL([MonetaryValue1], 0) - 
								IIF(ISNUMERIC([ExternalReference2]) = 1 AND [ExternalReference2] <> N''-'', 0.02 * [MonetaryValue0], 0),
		--[ExternalReference2]= ISNULL([ExternalReference2], N''--''),
		[NotedAmount1]		= ISNULL([MonetaryValue0], 0),
		[NotedAmount2]		= ISNULL([MonetaryValue0], 0),
		[NotedRelationId1]	= [NotedRelationId0],
		[NotedRelationId2]	= [NotedRelationId0],
		-- Entry Type may change depending on nature of items
		[EntryTypeId3]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''PaymentsToSuppliersForGoodsAndServices''),
		[NotedAgentName3]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId0]),
		[AdditionalReference3] = IIF(ISNUMERIC([AdditionalReference3]) = 1, N''CPV'' + [AdditionalReference3], [AdditionalReference3])
'
WHERE [Index] = 1610;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId]) VALUES
(0,1610,+1,		@ReceiptsAtPointInTimeFromSuppliersControlExtension), -- Item price
(1,1610,+1,		@CurrentValueAddedTaxReceivables), -- VAT, Taxamble Amount
(2,1610,-1,		@WithholdingTaxPayableExtension), -- Amount paid, Equivalent Actual amount to be paid. Noted Currency Id
(3,1610,-1,		@CashAndCashEquivalents); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1610,N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1610,N'NotedRelationId',		0,	N'Supplier',		1,4,1,NULL),
(2,1610,N'CurrencyId',			0,	N'Currency',		1,2,1,NULL),
(3,1610,N'MonetaryValue',		0,	N'Amount (VAT Excl)',1,2,0,NULL),
(4,1610,N'MonetaryValue',		1,	N'VAT',				1,4,0,NULL),
(5,1610,N'ExternalReference',	1,	N'Invoice #',		1,4,0,NULL),
(6,1610,N'MonetaryValue',		2,	N'Amount Withheld',	4,4,0,NULL),
(7,1610,N'ExternalReference',	2,	N'WT Voucher #',	5,5,0,NULL),
(8,1610,N'MonetaryValue',		3,	N'Net To Pay',		0,0,0,NULL),
(9,1610,N'ExternalReference',	3,	N'Check #',			5,5,0,NULL),
(10,1610,N'CustodyId',			3,	N'Cash/Bank Acct',	4,4,0,NULL),
(11,1610,N'PostingDate',		0,	N'Payment Date',	1,2,1,NULL),
(12,1610,N'CenterId',			0,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''),
(13,1610,N'AdditionalReference',3,	N'CPV #',			1,4,0,NULL);
--1630:CashToSupplier
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= COALESCE([CurrencyId1], [CurrencyId0]),
		[CenterId1]		= COALESCE([CenterId1], [CenterId0]),
		[MonetaryValue1]	= ISNULL([MonetaryValue1], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue1], 0),
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId0]),
		[AdditionalReference1] = IIF(ISNUMERIC([AdditionalReference1]) = 1, N''CPV'' + [AdditionalReference1], [AdditionalReference1]) 
'
WHERE [Index] = 1630;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId],							[EntryTypeId]) VALUES
(0,1630,+1,		@CashPaymentsToSuppliersControlExtension,	NULL),
(1,1630,-1,		@CashAndCashEquivalents,					@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1630,	N'Memo',				0,	N'Memo',			1,4,1,NULL),
(1,1630,	N'CustodyId',			1,	N'Cash/Bank Acct',	4,4,0,NULL),
(2,1630,	N'MonetaryValue',		0,	N'Amount',			1,2,0,NULL), 
(3,1630,	N'CurrencyId',			0,	N'Currency',		1,2,1,NULL),
(4,1630,	N'NotedRelationId',		0,	N'Supplier',		1,4,1,NULL),
(5,1630,	N'PostingDate',			0,	N'Paid On',			1,4,1,NULL),
(6,1630,	N'ExternalReference',	1,	N'Check #',			5,5,0,NULL),
(7,1630,	N'CenterId',			0,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit'''),
(8,1630,	N'AdditionalReference',	1,	N'CPV #',			5,5,0,NULL);
--1660:SupplierWT
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
 UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue1]	= ISNULL(0.02 * [NotedAmount1], 0),
		[MonetaryValue0]	= ISNULL(0.02 * [NotedAmount1], 0),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1]) 
'
WHERE [Index] = 1660;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],										[EntryTypeId]) VALUES
(0,1660,+1,	@CashPaymentsToSuppliersControlExtension,NULL),
(1,1660,-1,	@WithholdingTaxPayableExtension,NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1660,N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1660,N'NotedRelationId',		1,	N'Supplier',		3,4,1,NULL),
(2,1660,N'CurrencyId',			1,	N'Currency',		1,2,1,NULL),
(3,1660,N'NotedAmount',			1,	N'Amount (VAT Excl.)',3,3,0,NULL),
(4,1660,N'MonetaryValue',		1,	N'Amount Withheld',	0,0,0,NULL),
(9,1660,N'ExternalReference',	1,	N'Voucher #',		1,4,1,NULL),
(10,1660,N'PostingDate',		1,	N'Voucher Date',	1,4,1,NULL),
(11,1660,N'CenterId',			1,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit''');
--1700:PointExpenseFromInventory
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[ParentAccountTypeId] = ATC.[Id]
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node])  = 1
		WHERE ATP.[Concept] = N''Inventories''
	),
	ResourceCosts AS (
		SELECT
		PWL.PostingDate, PWL.[CustodyId1],  PWL.[ResourceId1],
			SUM(E.[AlgebraicMonetaryValue]) AS NetMonetaryValue,
			SUM(E.[AlgebraicValue]) AS NetValue,
			SUM(E.[AlgebraicQuantity]) AS NetQuantity
		FROM map.[DetailsEntries]() E
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		JOIN @ProcessedWideLines PWL ON PWL.[ResourceId1] = E.[ResourceId] AND PWL.[CustodyId1] = E.[CustodyId] AND L.PostingDate <= PWL.[PostingDate]
		WHERE E.[AccountId] IN (SELECT [Id] FROM InventoryAccounts)
		AND L.[State] = 4
		GROUP BY PWL.PostingDate, PWL.[CustodyId1],  PWL.[ResourceId1]
	)	
	UPDATE PWL
	SET
		[CurrencyId0]		= R.[CurrencyId],
		[EntryTypeId1]		= CASE
								WHEN C.[CenterType] = N''ProductionExpense'' THEN (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''IncreaseDecreaseThroughProductionExtension'')
								ELSE (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''IncreaseDecreaseThrougConsumptionExtension'')
							END,
		[CurrencyId1]		= R.[CurrencyId],
		[ResourceId0]		= PWL.[ResourceId1], [Quantity0] = PWL.[Quantity1], [UnitId0] = PWL.[UnitId1],
		[MonetaryValue0]	= IIF (
						ISNULL(RC.[NetQuantity],0) = 0,
						0,
						RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
					),
		-- Assuming that, for foreign currency, value is credit at same ratio as Monetary Value
		[Value0]		= IIF (
						ISNULL(RC.[NetQuantity],0) = 0,
						0,
						RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
					),
		[MonetaryValue1]	= IIF (
						ISNULL(RC.[NetQuantity],0) = 0,
						0,
						RC.NetMonetaryValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
					),
		[Value1]			= IIF (
						ISNULL(RC.[NetQuantity],0) = 0,
						0,
						RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
					),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = PWL.[CustodyId1]),
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = PWL.[CustodyId0])
	FROM @ProcessedWideLines PWL
	LEFT JOIN ResourceCosts RC ON PWL.[ResourceId1] = RC.[ResourceId1] AND PWL.[CustodyId1] = RC.[CustodyId1] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
	LEFT JOIN dbo.Centers C ON PWL.[CenterId0] = C.[Id]
'
WHERE [Index] = 1700;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],			[EntryTypeId]) VALUES
(0,1700,+1,	@ExpenseByNature,			NULL),
(1,1700,-1,	@Inventories,				@InventoriesUsedInOperationExtension);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1700,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1700,	N'Memo',				1,	N'Memo',			1,4,1,NULL),
(1,1700,	N'CustodyId'	,		1,	N'Warehouse',		3,4,1,NULL),
(2,1700,	N'ResourceId',			1,	N'Item',			1,2,0,NULL),
(3,1700,	N'Quantity',			1,	N'Qty',				1,2,0,NULL),
(4,1700,	N'UnitId',				1,	N'Unit',			1,2,0,NULL),
(5,1700,	N'CenterId',			0,	N'Cost Center',		1,2,0,NULL),
(6,1700,	N'PostingDate',			1,	N'Issued On',		1,4,1,NULL),
(7,1700,	N'CenterId',			1,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit''');
--1710:PointExpenseFromSupplier
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue0] = ISNULL([MonetaryValue1],0),
		[CurrencyId0] = [CurrencyId1],
		[NotedRelationId0] = [NotedRelationId1] 
'
WHERE [Index] = 1710;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],										[EntryTypeId]) VALUES
(0,1710,+1,	@ExpenseByNature,										NULL),
(1,1710,-1,	@ReceiptsAtPointInTimeFromSuppliersControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1710,	N'Memo',				0,	N'Memo',			1,5,1,NULL),
(1,1710,	N'NotedRelationId',		1,	N'Supplier',		2,3,1,NULL),
(2,1710,	N'CurrencyId',			1,	N'Currency',		1,2,1,NULL),
(3,1710,	N'MonetaryValue',		1,	N'Cost (VAT Excl.)',1,2,0,NULL),
(4,1710,	N'CenterId',			0,	N'Cost Center',		1,4,0,NULL),
(5,1710,	N'PostingDate',			1,	N'Received On',		1,4,1,NULL),
(6,1710,	N'CenterId',			1,	N'Business Unit',	1,4,1,N'CenterType=''BusinessUnit''');
--1730:ExpenseFromSupplierWithInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue0] = ISNULL([MonetaryValue0], 0),
		[MonetaryValue1] = ISNULL([MonetaryValue0], 0),
		[MonetaryValue2] = ISNULL([MonetaryValue0], 0) + ISNULL([MonetaryValue1], 0),
		[CurrencyId0] = [CurrencyId2],
		[NotedAmount1] =  ISNULL([MonetaryValue0], 0),
		[NotedRelationId1] = [NotedRelationId2],
		[NotedRelationId0] = [NotedRelationId2] 
'
WHERE [Index] = 1730;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],										[EntryTypeId]) VALUES
(0,1730,+1,	@ExpenseByNature,										NULL),
(1,1730,+1,	@CurrentValueAddedTaxReceivables,						NULL),
(2,1730,-1,	@CashPaymentsToSuppliersControlExtension,				NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1730,	N'Memo',				0,	N'Memo',			1,5,1,NULL),
(1,1730,	N'NotedRelationId',		2,	N'Supplier',		2,3,1,NULL),
(2,1730,	N'CurrencyId',			2,	N'Currency',		1,2,1,NULL),
(3,1730,	N'MonetaryValue',		0,	N'Cost (VAT Excl.)',1,2,0,NULL),
(4,1730,	N'MonetaryValue',		1,	N'VAT',				1,2,0,NULL),
(5,1730,	N'MonetaryValue',		2,	N'Line Total',		0,0,0,NULL),
(6,1730,	N'ExternalReference',	2,	N'Invoice #',		4,5,0,NULL),
(7,1730,	N'CenterId',			0,	N'Cost Center',		1,4,0,NULL),
(8,1730,	N'PostingDate',			2,	N'Received On',		1,4,1,NULL),
(9,1730,	N'CenterId',			2,	N'Business Unit',	0,4,1,N'CenterType=''BusinessUnit''');
--1780:DepreciationPPE
UPDATE @LineDefinitions
SET
	[GenerateScript] = N'
	DECLARE @PostingDate DATE;
	SELECT @PostingDate = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''PostingDate'') AS DATE);
	EXEC [wiz].[AssetsDepreciation__Populate] @PostingDate = @PostingDate
	',
	[PreprocessScript] = N'
-- Currency, Unit, Center and Custody are recalculated here again, in case the user chose some assets manually.
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N''PropertyPlantAndEquipment'');
	DECLARE @PPETypeIds IdList;
	WITH PPETypeIds AS (
		SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@PPENode) = 1
	),
	PPEBalancesPre AS (
		SELECT
				E.[ResourceId],
				E.[CustodyId],
				E.[CenterId],
				E.[CurrencyId]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.Accounts A ON E.AccountId = A.[Id]
		JOIN dbo.Resources R ON E.[ResourceId] = R.[Id] AND E.[UnitId] = R.[UnitId]
		JOIN @ProcessedWideLines PWL ON R.[Id] = PWL.[ResourceId1]
		WHERE A.[ParentAccountTypeId] IN (SELECT [Id] FROM PPETypeIds)
		AND L.[State] = 4
		AND (L.[PostingDate] < PWL.[PostingDate] OR L.[PostingDate] = PWL.[PostingDate] AND L.[Id] < PWL.Id)
		GROUP BY E.[ResourceId], E.[CustodyId], E.[CenterId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[Quantity]) > 0 OR SUM(E.[Direction] * E.[MonetaryValue]) > 0 OR  SUM(E.[Direction] * E.[Value]) > 0
	)
	UPDATE PWL
	SET
		PWL.[MonetaryValue0] = ISNULL(PWL.[MonetaryValue1], 0),
		PWL.[CustodyId1] = PPB.[CustodyId],
		PWL.[CenterId1] = PPB.[CenterId],
		PWL.[CurrencyId1] = PPB.[CurrencyId],
		PWL.[CurrencyId0] = PPB.[CurrencyId]
		-- Unit Id will be set in the Generic Preprocess, since it comes from Resources
	FROM @ProcessedWideLines PWL
	LEFT JOIN PPEBalancesPre PPB ON PWL.[ResourceId1] = PPB.[ResourceId] 
'
WHERE [Index] = 1780;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],			[EntryTypeId]) VALUES
(0,1780,+1,	@DepreciationExpense,		NULL),
(1,1780,-1,	@PropertyPlantAndEquipment,	@DepreciationPropertyPlantAndEquipment);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader],[Filter]) VALUES
(0,1780,N'PostingDate',			0,	N'Posting Date',	0,0,0,NULL),
(1,1780,N'ResourceId',			1,	N'FIxed Asset',		0,0,0,NULL),
(2,1780,N'Quantity',			1,	N'Usage/Period',	0,1,0,NULL),
(3,1780,N'UnitId',				1,	N'Unit',			0,0,0,NULL),
(4,1780,N'MonetaryValue',		1,	N'Depreciation',	0,4,0,NULL),
(5,1780,N'CurrencyId',			1,	N'Currency',		0,0,0,NULL),
(6,1780,N'CenterId',			0,	N'Cost Center',		1,4,0,NULL),
(7,1780,N'Memo',				0,	N'Memo',			0,5,1,NULL);
INSERT INTO @LineDefinitionGenerateParameters([Index], [HeaderIndex],
		[Key],			[Label],				[Visibility],	[DataType],	[Filter]) VALUES
(0,1,N'PostingDate',	N'Posting Date',		N'Required',	N'Date',	NULL);

EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryCustodyDefinitions = @LineDefinitionEntryCustodyDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
-- Declarations
DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
DECLARE @PPEFromIPCLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEFromIPC');
DECLARE @PPEFromInventoryLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEFromInventory');
DECLARE @PPEFromSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEFromSupplier');
DECLARE @PPEFromSupplierWithPointInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEFromSupplierWithPointInvoice');
DECLARE @CIPFromConstructionExpenseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CIPFromConstructionExpense');
DECLARE @IPUCDFromDevelopmentExpenseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IPUCDFromDevelopmentExpense');
DECLARE @InventoryFromPPELD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryFromPPE');
DECLARE @InventoryFromIPCLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryFromIPC');
DECLARE @InventoryTransferLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryTransfer');
DECLARE @InventoryConversionLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryConversion');
DECLARE @InventoryFromIITLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryFromIIT');
DECLARE @InventoryFromSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryFromSupplier');
DECLARE @InventoryFromSupplierWithPointInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryFromSupplierWithPointInvoice');
DECLARE @IITFromLCLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IITFromLC');
DECLARE @IITFromTransitExpenseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IITFromTransitExpense');
DECLARE @WIPFromProductionExpenseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'WIPFromProductionExpense');
DECLARE @RevenueFromInventoryLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RevenueFromInventory');
DECLARE @RevenueFromPeriodServiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RevenueFromPeriodService');
DECLARE @RevenueFromInventoryWithPointInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RevenueFromInventoryWithPointInvoice');
DECLARE @RevenueFromPointServiceWithPointInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RevenueFromPointServiceWithPointInvoice');
DECLARE @RevenueFromPeriodServiceWithPeriodInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RevenueFromPeriodServiceWithPeriodInvoice');
DECLARE @CashFromCustomerLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashFromCustomer');
DECLARE @CashFromCustomerWithWTLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashFromCustomerWithWT');
DECLARE @CashFromCustomerWithPointInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashFromCustomerWithPointInvoice');
DECLARE @CashFromCustomerWithPeriodInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashFromCustomerWithPeriodInvoice');
DECLARE @CashFromCustomerWithWTWithPointInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashFromCustomerWithWTWithPointInvoice');
DECLARE @CashFromCustomerWithWTWithPeriodInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashFromCustomerWithWTWithPeriodInvoice');
DECLARE @CashExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashExchange');
DECLARE @CashTransferLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransfer');
DECLARE @CashToSalariesLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashToSalaries');
DECLARE @CashToSupplierWithPointInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashToSupplierWithPointInvoice');
DECLARE @CashToSupplierWithPeriodInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashToSupplierWithPeriodInvoice');
DECLARE @CashToSupplierWithPointInvoiceWithWTLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashToSupplierWithPointInvoiceWithWT');
DECLARE @CashToSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashToSupplier');
DECLARE @SupplierWTLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'SupplierWT');
DECLARE @PointExpenseFromInventoryLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PointExpenseFromInventory');
DECLARE @PointExpenseFromSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PointExpenseFromSupplier');
DECLARE @PeriodExpenseFromSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PeriodExpenseFromSupplier');
DECLARE @ExpenseFromSupplierWithInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ExpenseFromSupplierWithInvoice');
DECLARE @SalaryTransportationOtherOvertimePensionLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'SalaryTransportationOtherOvertimePension');
DECLARE @SalaryTransportationOvertimePenaltyPensionProvidentCostSharingLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'SalaryTransportationOvertimePenaltyPensionProvidentCostSharing');
DECLARE @OvertimeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'Overtime');
DECLARE @DepreciationPPELD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'DepreciationPPE');
DECLARE @AnnualLeaveAllowanceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'AnnualLeaveAllowance');
DECLARE @AnnualLeaveUsageLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'AnnualLeaveUsage');
DECLARE @UnpaidLeaveLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'UnpaidLeave');
DECLARE @TradeSettlementLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'TradeSettlement');




/*
DECLARE @TranslationsLD TABLE (
	[Word] NVARCHAR (50),
	[Lang] NVARCHAR (5),
	PRIMARY KEY ([Word], [Lang]),
	[Translated] NVARCHAR (50)
)
INSERT INTO @TranslationsLD 
([Word],				[Lang], [Translated]) VALUES
(N'Adjustment',			N'ar',	N'تسوية'),
(N'Adjustments',		N'ar',	N'تسويات'),
(N'Other Payment',		N'ar',	N'دفعية أخرى'),
(N'Other Payments',		N'ar',	N'دفعيات أخرى'),
(N'Transfer/Exchange',	N'ar',	N'تحويل\صرف'),		
(N'Transfers/Exchanges',N'ar',	N'تحويلات\صرف'),
(N'Duplicate Line',		N'ar',	N'تسوية'),
(N'Incorrect Analysis', N'ar',	N'تسوية'),
(N'Other reasons',		N'ar',	N'تسوية'),
(N'Payment to Supplier',N'ar',	N'دفعية لمورد'),
(N'Payments to Suppliers',N'ar',N'دفعيات لموردين'),
(N'Memo',				N'ar',	N'البيان'),
(N'Supplier',			N'ar',	N'المورد'),
(N'Employee',			N'ar',	N'الموظف'),
(N'Beneficiary',		N'ar',	N'المستفيد'),
(N'Due Currency',		N'ar',	N'عملة الاستحقاق'),
(N'Total Due',			N'ar',	N'جملة الاستحقاق'),
(N'From Account',		N'ar',	N'من حساب'),
(N'To Account',			N'ar',	N'إلى حساب'),
(N'From Currency',		N'ar',	N'من عملة'),
(N'To Currency',		N'ar',	N'إلى عملة'),
(N'From Amount',		N'ar',	N'من مبلغ'),
(N'To Amount',			N'ar',	N'إلى مبلغ'),
(N'Due Amount',			N'ar',	N'القسط الحالي'),
(N'Pay Currency',		N'ar',	N'عملة الدفع'),
(N'Pay Amount',			N'ar',	N'المبلغ المدفوع'),
(N'Bank/Cashier',		N'ar',	N'البنك\الخزنة'),
(N'Check/Receipt #',	N'ar',	N'رقم الشيك\الإيصال'),
(N'Check Date',			N'ar',	N'تاريخ الشيك'),
(N'Inv. Ctr',			N'ar',	N'مركز الاستثمار');
--(0,9,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
--(1,9,-3,	N'Other reasons',		N'أسباب أخرى');
--N'Payment Employee Benefit',N'دفعية لصالح موظف',	

DECLARE @Lang2 NVARCHAR (5), @Lang3 NVARCHAR (5);
SELECT @Lang2 = SecondaryLanguageId, @Lang3 = TernaryLanguageId FROM dbo.Settings

UPDATE LD
SET LD.[TitleSingular2] = T.[Translated]
FROM @LineDefinitions LD JOIN @TranslationsLD T ON LD.[TitleSingular] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LD
SET	LD.[TitlePlural2] = T.[Translated]
FROM @LineDefinitions LD JOIN @TranslationsLD T ON LD.[TitlePlural] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LDSR
SET LDSR.[Name2] = T.[Translated]
FROM @LineDefinitionStateReasons LDSR JOIN @TranslationsLD T ON LDSR.[Name] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LDC
SET	LDC.[Label2] = T.[Translated]
FROM @LineDefinitionColumns LDC JOIN @TranslationsLD T ON LDC.[Label] = T.[Word] WHERE T.[Lang] = @Lang2
*/
