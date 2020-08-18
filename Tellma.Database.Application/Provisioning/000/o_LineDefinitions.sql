INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(1000, N'ManualLine', N'Making any accounting adjustment', N'Adjustment', N'Adjustments', 0, 0),
(1060, N'PPEFromSupplier', N'Receiving property, plant and equipment from supplier, invoiced separately', N'PPE Purchase', N'PPE Purchases', 0, 1),
(1200, N'InventoryTransfer', N'Inventory transfer between warehouses (1-1)', N'Stock Transfer', N'Stock Transfers + Conversions', 0, 0),
(1260, N'InventoryFromSupplier', N'Receiving inventory from supplier/contractor', N'Stock Purchase', N'Stock Purchases', 0, 0),
(1330, N'RevenueFromInventory', N'Issuing inventory to customer, invoiced separately', N'Inventory (Sale)', N'Inventories (Sales)', 0, 0),
--(1350, N'RevenueFromPeriodService', N'Rendering period services to customer, invoiced separately', N'Lease Out', N'Leases Out', 0, 1),
(1360, N'RevenueFromInventoryWithPointInvoice', N'Issuing inventory to customer + point invoice', N'Inventory (Sale) + Invoice', N'Inventories (Sale) + Invoices', 0, 0),
--(1370, N'RevenueFromPointServiceWithPointInvoice', N'Rendering point services to customer + point invoice', N'Service (Sale) + Invoice (Point)', N'Services (Sales) + Invoices (Point)', 0, 1),
--(1380, N'RevenueFromPeriodServiceWithPeriodInvoice', N'Rendering period services to customer + period invoice', N'Service (Sale) + Invoice (Period)', N'Services (Sale) + Invoices (Period)', 0, 1),
(1410, N'CashFromCustomer', N'Collecting cash from customer/lessee, Invoiced separately', N'Cash Receipt (Sale)', N'Cash Receipts (Sale)', 0, 1),
(1420, N'CashFromCustomerWithWT', N'Collecting cash from customer/lessee with WT, Invoiced separately', N'Cash Receipt + WT', N'Cash Receipts + WT', 0, 1),
(1430, N'CashFromCustomerWithPointInvoice', N'Collecting cash from customer + point invoice', N'Cash Receipt + Point Invoice', N'Cash Receipts + Point Invoices', 0, 1),
--(1440, N'CashFromCustomerWithPeriodInvoice', N'Collecting cash from lessee + period invoice', N'Cash Receipt + Period Invoice', N'Cash Receipts + Period Invoices', 0, 1),
(1450, N'CashFromCustomerWithWTWithPointInvoice', N'Collecting cash from customer + WT + point invoice', N'Cash Receipt + WT + Point Invoice', N'Cash Receipts + WT + Point Invoices', 0, 1),
--(1460, N'CashFromCustomerWithWTWithPeriodInvoice', N'Collecting cash from lessee + WT + period invoice', N'Cash Receipt + WT + Period Invoice', N'Cash Receipts + WT + Period Invoices', 0, 1),
(1550, N'CashTransferExchange', N'Cash transfer and currency exchange', N'Cash Transfer & Exchange', N'Cash Transfers & Exchanges', 0, 1),
(1560, N'CashTransfer', N'Cash transfer, same currency', N'Cash Transfer', N'Cash Transfers', 0, 1),
(1570, N'CashExchange', N'Currency exchange, same account', N'Cash Exchange', N'Cash Exchanges', 0, 1),
(1660, N'CashToSupplierWithPointInvoice', N'Paying cash to supplier/lessor/.. + point invoice', N'Cash Payment + Point Invoice', N'Cash Payments + Point Invoices', 0, 1),
(1680, N'CashToSupplierWithPointInvoiceWithWT', N'Paying cash to supplier/lessor/.. + point invoice + WT', N'Cash Payment + Point Invoice + WT', N'Cash Payments + Point Invoices + WT', 0, 1),
(1730, N'SupplierWT', N'WT from supplier', N'WT (Purchase)', N'WT (Purchases)', 0, 1),
(1770, N'PointExpenseFromInventory', N'Issuing inventory to cost center (maintenance, job order, production line, construction project..)', N'Stock Consumption', N'Stock Consumptions', 0, 0),
(1780, N'PointExpenseFromSupplier', N'Receiving consumables and point services from supplier, invoiced separately', N'C/S Purchase', N'C/S Purchases', 0, 0)
--1000: ManualLine
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Direction], [AccountTypeId]) VALUES (0,1000,+1, @StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,1000,	N'AccountId',	0,			N'Account',		4,4,0), -- together with properties
(1,1000,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,1000,	N'Value',		0,			N'Credit',		4,4,0),
(3,1000,	N'Memo',		0,			N'Memo',		4,4,1);
--1060: PPEFromSupplier
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId2],
		[CurrencyId1]		= [CurrencyId2],
		[CenterId0]			= COALESCE([CenterId0], [CenterId2]),
		[CenterId1]			= COALESCE([CenterId1], [CenterId2]),
		[CustodyId1]		= [CustodyId0],
		[MonetaryValue1]	= ISNULL([MonetaryValue2],0) - ISNULL([MonetaryValue0],0),
		[ResourceId1]		= [ResourceId0],
		[Quantity0]			= 1,
		[UnitId0]			= (SELECT [Id] FROM dbo.Units WHERE Code = N''pure''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1])
'
WHERE [Index] = 1060;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],			[EntryTypeId]) VALUES
(0,1060,+1,	@PropertyPlantAndEquipment,	@AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment),
(1,1060,+1,	@PropertyPlantAndEquipment,	@AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment),
(2,1060,-1,	@ReceiptsAtPointInTimeFromSuppliersControlExtension,NULL);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,1060,@PPECustodyCD),
(0,1,1060,@PPECustodyCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1060,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1060,	N'NotedRelationId',		2,	N'Supplier',		3,4,1),
(2,1060,	N'CustodyId',			0,	N'Custody',			5,5,0),
(3,1060,	N'ResourceId',			0,	N'Fixed Asset',		2,4,0),
(4,1060,	N'Quantity',			1,	N'Life/Usage',		2,4,0),
(5,1060,	N'UnitId',				1,	N'Unit',			2,4,0),
(6,1060,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(7,1060,	N'MonetaryValue',		2,	N'Cost (VAT Excl.)',1,2,0),
(8,1060,	N'MonetaryValue',		0,	N'Residual Value',	1,2,0),
(10,1060,	N'PostingDate',			1,	N'Acquired On',		1,4,1),
(11,1060,	N'CenterId',			2,	N'Business Unit',	1,4,1);
--1200:InventoryTransfer
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
 WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[AccountTypeId] = ATC.[Id]
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
'
WHERE [Index] = 1200;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],	[EntryTypeId]) VALUES
(0,1200,+1,	@Inventories,		@InternalInventoryTransferExtension),
(1,1200,-1,	@Inventories,		NULL);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,1200,@WarehouseCD),
(0,1,1200,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1200,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1200,	N'NotedRelationId',		1,	N'Supplier',		3,4,1),
(2,1200,	N'CustodyId',			0,	N'Warehouse',		3,4,1),
(3,1200,	N'ResourceId',			0,	N'Item',			2,4,0),
(4,1200,	N'Quantity',			0,	N'Qty',				2,4,0),
(5,1200,	N'UnitId',				0,	N'Unit',			2,4,0),
(6,1200,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(7,1200,	N'MonetaryValue',		1,	N'Cost (VAT Excl.)',1,2,0),
(10,1200,	N'PostingDate',			1,	N'Received On',		1,4,1),
(11,1200,	N'CenterId',			1,	N'Business Unit',	1,4,1);
--1260:InventoryFromSupplier
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue0]	= [MonetaryValue1],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1])
'
WHERE [Index] = 1260;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],										[EntryTypeId]) VALUES
(0,1260,+1,	@Inventories,											@ReceiptsReturnsThroughPurchaseExtension),
(1,1260,-1,	@ReceiptsAtPointInTimeFromSuppliersControlExtension,	NULL);
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,0,1260,@MerchandiseRD),
(1,0,1260,@CurrentFoodAndBeverageRD),
(2,0,1260,@CurrentAgriculturalProduceRD),
(3,0,1260,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(4,0,1260,@RawMaterialsRD),
(5,0,1260,@ProductionSuppliesRD),
(6,0,1260,@CurrentPackagingAndStorageMaterialsRD),
(7,0,1260,@SparePartsRD),
(8,0,1260,@CurrentFuelRD),
(9,0,1260,@OtherInventoriesRD),
(10,0,1260,@TradeMedicineRD),
(11,0,1260,@TradeConstructionMaterialRD),
(12,0,1260,@TradeSparePartRD),
(13,0,1260,@RawGrainRD),
(14,0,1260,@RawVehicleRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,1260,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1260,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1260,	N'NotedRelationId',		1,	N'Supplier',		3,4,1),
(2,1260,	N'CustodyId',			0,	N'Warehouse',		3,4,1),
(3,1260,	N'ResourceId',			0,	N'Item',			2,4,0),
(4,1260,	N'Quantity',			0,	N'Qty',				2,4,0),
(5,1260,	N'UnitId',				0,	N'Unit',			2,4,0),
(6,1260,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(7,1260,	N'MonetaryValue',		1,	N'Cost (VAT Excl.)',1,2,0),
(10,1260,	N'PostingDate',			1,	N'Received On',		1,4,1),
(11,1260,	N'CenterId',			1,	N'Business Unit',	1,4,1);
--1330:RevenueFromInventory
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[AccountTypeId] = ATC.[Id]
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
		[CustodyId0]		= PWL.[CustodyId1],
		[CustodyId3]		= PWL.[CustodyId1],
		[CurrencyId0]		= R.[CurrencyId],
		[CurrencyId1]		= R.[CurrencyId],
		[CurrencyId3]		= PWL.[CurrencyId2],
		[CenterId1]			= COALESCE(PWL.[CenterId1], PWL.[CenterId2]),
		[CenterId0]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[CenterId3]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
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
		[NotedRelationId0]	= [NotedRelationId3],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId3]),
		[NotedRelationId2]	= [NotedRelationId3]
	FROM @ProcessedWideLines PWL
	LEFT JOIN ResourceCosts RC ON PWL.[ResourceId1] = RC.[ResourceId1] AND PWL.[CustodyId1] = RC.[CustodyId1] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
'
WHERE [Index] = 1330;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],									[EntryTypeId]) VALUES
(0,1330,+1,	@CostOfMerchandiseSold,								NULL),
(1,1330,-1,	@Inventories,										@ReturnsIssuesThroughSaleExtension),
(2,1330,+1,	@IssuesAtPointInTimeToCustomersControlExtension,	NULL),
(3,1330,-1,	@Revenue,											NULL)
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,1,1330,@MerchandiseRD),
(1,1,1330,@CurrentFoodAndBeverageRD),
(2,1,1330,@CurrentAgriculturalProduceRD),
(3,1,1330,@FinishedGoodsRD),
(4,1,1330,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(5,1,1330,@TradeMedicineRD),
(6,1,1330,@TradeConstructionMaterialRD),
(7,1,1330,@TradeSparePartRD),
(8,1,1330,@FinishedGrainRD),
(9,1,1330,@ByproductGrainRD),
(10,1,1330,@FinishedVehicleRD),
(11,1,1330,@FinishedOilRD),
(12,1,1330,@ByproductOilRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1330,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1330,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1330,	N'NotedRelationId',		3,	N'Customer',		3,4,1),
(2,1330,	N'CustodyId',			1,	N'Warehouse',		3,4,1),
(3,1330,	N'ResourceId',			1,	N'Item',			2,4,0),
(4,1330,	N'Quantity',			1,	N'Qty',				2,4,0),
(5,1330,	N'UnitId',				1,	N'Unit',			2,4,0),
(6,1330,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(7,1330,	N'MonetaryValue',		3,	N'Cost (VAT Excl.)',1,2,0),
(10,1330,	N'PostingDate',			1,	N'Issued On',		1,4,1),
(11,1330,	N'CenterId',			2,	N'Business Unit',	1,4,1);
--1360:RevenueFromInventoryWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[AccountTypeId] = ATC.[Id]
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
		[MonetaryValue2]	= ISNULL([MonetaryValue3],0) + ISNULL([MonetaryValue4],0),
		[NotedAmount3]		= ISNULL([MonetaryValue3],0) + ISNULL([MonetaryValue4],0),
		[NotedRelationId0]	= [NotedRelationId4],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId4]),
		[NotedRelationId2]	= [NotedRelationId4]
	FROM @ProcessedWideLines PWL
	LEFT JOIN ResourceCosts RC ON PWL.[ResourceId1] = RC.[ResourceId1] AND PWL.[CustodyId1] = RC.[CustodyId1] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
'
WHERE [Index] = 1360;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],									[EntryTypeId]) VALUES
(0,1360,+1,	@CostOfMerchandiseSold,								NULL),
(1,1360,-1,	@Inventories,										@ReturnsIssuesThroughSaleExtension),
(2,1360,+1,	@CashReceiptsFromCustomersControlExtension,			NULL),
(3,1360,-1,	@CurrentValueAddedTaxPayables,						NULL),
(4,1360,-1,	@Revenue,											NULL)
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,1,1360,@MerchandiseRD),
(1,1,1360,@CurrentFoodAndBeverageRD),
(2,1,1360,@CurrentAgriculturalProduceRD),
(3,1,1360,@FinishedGoodsRD),
(4,1,1360,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(5,1,1360,@TradeMedicineRD),
(6,1,1360,@TradeConstructionMaterialRD),
(7,1,1360,@TradeSparePartRD),
(8,1,1360,@FinishedGrainRD),
(9,1,1360,@ByproductGrainRD),
(10,1,1360,@FinishedVehicleRD),
(11,1,1360,@FinishedOilRD),
(12,1,1360,@ByproductOilRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1360,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1360,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1360,	N'NotedRelationId',		4,	N'Customer',		3,4,1),
(2,1360,	N'CustodyId',			1,	N'Warehouse',		3,4,1),
(3,1360,	N'ResourceId',			1,	N'Item',			2,4,0),
(4,1360,	N'Quantity',			1,	N'Qty',				2,4,0),
(5,1360,	N'UnitId',				1,	N'Unit',			2,4,0),
(6,1360,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(7,1360,	N'MonetaryValue',		4,	N'Cost (VAT Excl.)',1,2,0),
(8,1360,	N'MonetaryValue',		3,	N'VAT',				1,2,0),
(9,1360,	N'MonetaryValue',		2,	N'Line Total',		1,2,0),
(10,1360,	N'ExternalReference',	2,	N'Invoice #',		1,4,0),
(11,1360,	N'PostingDate',			1,	N'Issued On',		1,4,1),
(12,1360,	N'CenterId',			2,	N'Business Unit',	1,4,1);
--1410:CashFromCustomer
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue1]	= ISNULL([MonetaryValue1], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue1], 0),
		[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1410;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId]) VALUES
(0,1410,+1,		@CashAndCashEquivalents),
(1,1410,-1,		@CashReceiptsFromCustomersControlExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1410,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1410,	N'NotedRelationId',		1,	N'Customer',		1,4,1),
(2,1410,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(3,1410,	N'MonetaryValue',		1,	N'Amount',			1,2,0), 
(9,1410,	N'ExternalReference',	0,	N'Check #',			5,5,0),
(10,1410,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0),
(11,1410,	N'PostingDate',			1,	N'Received On',		1,2,1),
(12,1410,	N'CenterId',			1,	N'Business Unit',	1,4,1),
(13,1410,	N'AdditionalReference',	0,	N'CRV #',			5,5,0);
--1420:CashFromCustomerWithWT
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[CurrencyId0]		= [CurrencyId2],
		[CenterId1]			= [CenterId2],
		[CenterId0]			= COALESCE([CenterId0], [CenterId2]),
		[MonetaryValue2]	= ISNULL([MonetaryValue2], 0),
		[MonetaryValue1]	= IIF(ISNUMERIC([ExternalReference1]) = 1 AND [ExternalReference1] <> N''-'', 0.02 * ISNULL([NotedAmount1],0), 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue2], 0) - 
								IIF(ISNUMERIC([ExternalReference1]) = 1 AND [ExternalReference1] <> N''-'', 0.02 * ISNULL([NotedAmount1],0), 0),
		[NotedRelationId1]	= [NotedRelationId2],
		--[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId2]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1420;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId],							[EntryTypeId]) VALUES
(0,1420,+1,		@CashAndCashEquivalents,					@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,1420,+1,		@WithholdingTaxReceivablesExtension,		NULL),
(2,1420,-1,		@CashReceiptsFromCustomersControlExtension,	NULL); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1420,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1420,	N'NotedRelationId',		2,	N'Customer',		1,4,1),
(2,1420,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(3,1420,	N'MonetaryValue',		2,	N'Due Amount',		1,2,0), -- 
(6,1420,	N'MonetaryValue',		1,	N'Amount Withheld',	4,4,0),
(7,1420,	N'ExternalReference',	1,	N'WT Voucher #',	5,5,0),
(8,1420,	N'MonetaryValue',		0,	N'Net To Receive',	1,1,0),
(9,1420,	N'ExternalReference',	0,	N'Check #',			5,5,0),
(10,1420,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0),
(11,1420,	N'PostingDate',			2,	N'Payment Date',	1,2,1),
(12,1420,	N'CenterId',			2,	N'Business Unit',	1,4,1),
(13,1420,	N'AdditionalReference',	0,	N'CRV #',			5,5,0);
--1430:CashFromCustomerWithWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[CurrencyId0]		= [CurrencyId2],
		[CenterId1]			= [CenterId2],
		[CenterId0]			= COALESCE([CenterId0], [CenterId2]),
		[MonetaryValue2]	= ISNULL([MonetaryValue2], 0),
		[MonetaryValue1]	= 0.15 * [MonetaryValue2],
		[MonetaryValue0]	= ISNULL([MonetaryValue2], 0) + ISNULL([MonetaryValue1], 0),
		[NotedAmount1]		= ISNULL([MonetaryValue2], 0),
		[NotedRelationId1]	= [NotedRelationId2],
	--	[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId2]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1430;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId],								[EntryTypeId]) VALUES
(0,1430,+1,		@CashAndCashEquivalents,						@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,1430,-1,		@CurrentValueAddedTaxPayables,					NULL),
(2,1430,-1,		@IssuesAtPointInTimeToCustomersControlExtension,NULL); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1430,	N'Memo',				0,	N'Memo',			1,4,1),
(1,1430,	N'NotedRelationId',		2,	N'Customer',		1,4,1),
(2,1430,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(3,1430,	N'MonetaryValue',		2,	N'Amount (VAT Excl)',1,2,0), -- 
(4,1430,	N'MonetaryValue',		1,	N'VAT',				1,4,0),
(5,1430,	N'ExternalReference',	1,	N'Invoice #',		1,4,0),
(8,1430,	N'MonetaryValue',		0,	N'Net To Receive',	1,1,0),
(9,1430,	N'ExternalReference',	0,	N'Check #',			5,5,0),
(10,1430,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0),
(11,1430,	N'PostingDate',			2,	N'Payment Date',	1,2,1),
(12,1430,	N'CenterId',			2,	N'Business Unit',	1,4,1),
(13,1430,	N'AdditionalReference',	0,	N'CRV #',			5,5,0);
--1450:CashFromCustomerWithWTWithPointInvoice
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
WHERE [Index] = 1450;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId]) VALUES
(0,1450,+1,		@CashAndCashEquivalents),
(1,1450,+1,		@WithholdingTaxReceivablesExtension),
(2,1450,-1,		@CurrentValueAddedTaxPayables),
(3,1450,-1,		@IssuesAtPointInTimeToCustomersControlExtension); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1450,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1450,	N'NotedRelationId',		3,	N'Customer',		1,4,1),
(2,1450,	N'CurrencyId',			3,	N'Currency',		1,2,1),
(3,1450,	N'MonetaryValue',		3,	N'Amount (VAT Excl)',1,2,0), -- 
(4,1450,	N'MonetaryValue',		2,	N'VAT',				1,4,0),
(5,1450,	N'ExternalReference',	2,	N'Invoice #',		1,4,0),
(6,1450,	N'MonetaryValue',		1,	N'Amount Withheld',	4,4,0),
(7,1450,	N'ExternalReference',	1,	N'WT Voucher #',	5,5,0),
(8,1450,	N'MonetaryValue',		0,	N'Net To Receive',	1,1,0),
(9,1450,	N'ExternalReference',	0,	N'Check #',			5,5,0),
(10,1450,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0),
(11,1450,	N'PostingDate',			3,	N'Payment Date',	1,2,1),
(12,1450,	N'CenterId',			3,	N'Business Unit',	1,4,1),
(13,1450,	N'AdditionalReference',	0,	N'CRV #',			5,5,0);
--1550:CashTransferExchange
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
WHERE [Index] = 1550;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,1550,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,1550,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(2,1550,+1,	@GainsLossesOnExchangeDifferencesOnTranslationBeforeTax, NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1550,	N'CustodyId',			1,	N'From Account',	1,2,0),
(1,1550,	N'CustodyId',			0,	N'To Account',		1,2,0),
(2,1550,	N'CurrencyId',			1,	N'From Currency',	1,2,0),
(3,1550,	N'CurrencyId',			0,	N'To Currency',		1,2,0),
(4,1550,	N'MonetaryValue',		1,	N'From Amount',		1,3,0),
(5,1550,	N'MonetaryValue',		0,	N'To Amount',		1,3,0),
(6,1550,	N'CenterId',			2,	N'Business Unit',	1,4,1),
(7,1550,	N'Memo',				0,	N'Memo',			1,2,1),
(8,1550,	N'PostingDate',			0,	N'Transfer Date',	1,2,1);
--1560:CashTransfer
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]),
		[MonetaryValue0] = ISNULL([MonetaryValue1],0)
'
WHERE [Index] = 1560;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,1560,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,1560,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1560,	N'CustodyId',			1,	N'From Account',	1,2,0),
(1,1560,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(2,1560,	N'CustodyId',			0,	N'To Account',		1,2,0),
(3,1560,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,1560,	N'MonetaryValue',		1,	N'Amount',			1,3,0),
(5,1560,	N'Memo',				0,	N'Memo',			1,2,1),
(6,1560,	N'PostingDate',			0,	N'Transfer Date',	1,2,1);
--1570:CashExchange
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]),
		[CustodyId0] = [CustodyId1],
		[CenterId0] = COALESCE((SELECT [CenterId] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]), [CenterId2]),
		[CenterId1] = COALESCE((SELECT [CenterId] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]), [CenterId2]),
		[CurrencyId2] = dbo.fn_FunctionalCurrencyId(),
		[MonetaryValue0] = IIF([CurrencyId0]=[CurrencyId1],[MonetaryValue1],[MonetaryValue0]),
		[MonetaryValue2] = wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId1], [MonetaryValue1])
							- wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId0], [MonetaryValue0]) 
'
WHERE [Index] = 1570;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,1570,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,1570,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(2,1570,+1,	@GainsLossesOnExchangeDifferencesOnTranslationBeforeTax, NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1570,	N'CustodyId',			1,	N'Account',			1,2,0),
(2,1570,	N'CurrencyId',			1,	N'From Currency',	1,2,0),
(3,1570,	N'CurrencyId',			0,	N'To Currency',		1,2,0),
(4,1570,	N'MonetaryValue',		1,	N'From Amount',		1,3,0),
(5,1570,	N'MonetaryValue',		0,	N'To Amount',		1,3,0),
(6,1570,	N'CenterId',			2,	N'Business Unit',	1,4,1),
(7,1570,	N'Memo',				0,	N'Memo',			1,2,1),
(8,1570,	N'PostingDate',			0,	N'Exchange Date',	1,2,1);
--1660:CashToSupplierWithPointInvoice
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
WHERE [Index] = 1660;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId]) VALUES
(0,1660,+1,		@ReceiptsAtPointInTimeFromSuppliersControlExtension),
(1,1660,+1,		@CurrentValueAddedTaxReceivables),
(2,1660,-1,		@CashAndCashEquivalents); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1660,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1660,	N'NotedRelationId',		0,	N'Supplier',		1,4,1),
(2,1660,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,1660,	N'MonetaryValue',		0,	N'Amount (VAT Excl)',1,2,0),
(4,1660,	N'MonetaryValue',		1,	N'VAT',				1,4,0),
(5,1660,	N'ExternalReference',	1,	N'Invoice #',		1,4,0),
(6,1660,	N'MonetaryValue',		2,	N'Net To Pay',		1,1,0),
(8,1660,	N'ExternalReference',	2,	N'Check #',			5,5,0),
(9,1660,	N'CustodyId',			2,	N'Cash/Bank Acct',	4,4,0),
(10,1660,N'PostingDate',			0,	N'Payment Date',	1,2,1),
(11,1660, N'CenterId',			0,	N'Business Unit',	1,4,1),
(12,1660, N'AdditionalReference',2,	N'CPV #',			1,4,0);
--1680:CashToSupplierWithPointInvoiceWithWT CashPaymentToTradePayableWithWT: (basically, it is the VAT) -- assume all in same currency
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
WHERE [Index] = 1680;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId]) VALUES
(0,1680,+1,		@ReceiptsAtPointInTimeFromSuppliersControlExtension), -- Item price
(1,1680,+1,		@CurrentValueAddedTaxReceivables), -- VAT, Taxamble Amount
(2,1680,-1,		@WithholdingTaxPayableExtension), -- Amount paid, Equivalent Actual amount to be paid. Noted Currency Id
(3,1680,-1,		@CashAndCashEquivalents); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1680,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1680,	N'NotedRelationId',		0,	N'Supplier',		1,4,1),
(2,1680,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,1680,	N'MonetaryValue',		0,	N'Amount (VAT Excl)',1,2,0),
(4,1680,	N'MonetaryValue',		1,	N'VAT',				1,4,0),
(5,1680,	N'ExternalReference',	1,	N'Invoice #',		1,4,0),
(6,1680,	N'MonetaryValue',		2,	N'Amount Withheld',	4,4,0),
(7,1680,	N'ExternalReference',	2,	N'WT Voucher #',	5,5,0),
(8,1680,	N'MonetaryValue',		3,	N'Net To Pay',		1,1,0),
(9,1680,	N'ExternalReference',	3,	N'Check #',			5,5,0),
(10,1680,N'CustodyId',			3,	N'Cash/Bank Acct',	4,4,0),
(11,1680,N'PostingDate',			0,	N'Payment Date',	1,2,1),
(12,1680, N'CenterId',			0,	N'Business Unit',	1,4,1),
(13,1680, N'AdditionalReference',3,	N'CPV #',			1,4,0);
--1730:SupplierWT
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue0]	= [MonetaryValue1],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1])
'
WHERE [Index] = 1730;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],										[EntryTypeId]) VALUES
(0,1730,+1,	@CashPaymentsToSuppliersControlExtension,NULL),
(1,1730,-1,	@WithholdingTaxPayableExtension,NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1730,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1730,	N'NotedRelationId',		1,	N'Supplier',		3,4,1),
(2,1730,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(3,1730,	N'NotedAmount',			1,	N'Amount (VAT Excl.)',3,3,0),
(4,1730,	N'MonetaryValue',		1,	N'Amount Withheld',	1,2,0),
(9,1730,N'ExternalReference',	1,	N'Voucher #',		1,4,1),
(10,1730,N'PostingDate',			1,	N'Voucher Date',	1,4,1),
(11,1730,N'CenterId',			1,	N'Business Unit',	1,4,1);
--1770:PointExpenseFromInventory
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[AccountTypeId] = ATC.[Id]
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
WHERE [Index] = 1770;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],			[EntryTypeId]) VALUES
(0,1770,+1,	@ExpenseByNature,			NULL),
(1,1770,-1,	@Inventories,				@IncreaseDecreaseThrougConsumptionExtension);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1770,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1770,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1770,	N'CustodyId'	,		1,	N'Warehouse',		3,4,1),
(2,1770,	N'ResourceId',			1,	N'Item',			1,2,0),
(3,1770,	N'Quantity',			1,	N'Qty',				1,2,0),
(4,1770,	N'UnitId',				1,	N'Unit',			1,2,0),
(5,1770,	N'CenterId',			0,	N'Cost Center',		1,2,0),
(6,1770,	N'PostingDate',			1,	N'Issued On',		1,4,1),
(7,1770,	N'CenterId',			1,	N'Business Unit',	1,4,1);
--1780:PointExpenseFromSupplier
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue0] = ISNULL([MonetaryValue1],0),
		[CurrencyId0] = [CurrencyId1],
		[NotedRelationId0] = [NotedRelationId1] 
'
WHERE [Index] = 1780;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],										[EntryTypeId]) VALUES
(0,1780,+1,	@ExpenseByNature,										NULL),
(1,1780,-1,	@ReceiptsAtPointInTimeFromSuppliersControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1780,	N'Memo',				0,	N'Memo',			1,4,1),
(1,1780,	N'NotedRelationId',		1,	N'Supplier',		3,4,1),
(2,1780,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(3,1780,	N'MonetaryValue',		1,	N'Cost (VAT Excl.)',1,2,0),
(4,1780,	N'CenterId',			0,	N'Cost Center',		1,2,0),
(5,1780,	N'PostingDate',			1,	N'Received On',		1,4,1),
(6,1780,	N'CenterId',			1,	N'Business Unit',	1,4,1);

EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryCustodyDefinitions = @LineDefinitionEntryCustodyDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionEntryNotedRelationDefinitions = @LineDefinitionEntryNotedRelationDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
-- Declarations
DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
DECLARE @PPEFromSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEFromSupplier');
DECLARE @InventoryTransferLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryTransfer');
DECLARE @InventoryFromSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InventoryFromSupplier');
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
DECLARE @CashTransferExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransferExchange');
DECLARE @CashTransferLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransfer');
DECLARE @CashExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashExchange');
DECLARE @CashToSupplierWithPointInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashToSupplierWithPointInvoice');
DECLARE @CashToSupplierWithPointInvoiceWithWTLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashToSupplierWithPointInvoiceWithWT');
DECLARE @SupplierWTLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'SupplierWT');
DECLARE @PointExpenseFromInventoryLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PointExpenseFromInventory');
DECLARE @PointExpenseFromSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PointExpenseFromSupplier');

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
