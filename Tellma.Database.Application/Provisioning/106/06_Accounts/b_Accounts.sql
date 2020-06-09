INSERT INTO @Accounts([Index],[Code],[Name], [AccountTypeId], [ContractDefinitionId],[ResourceDefinitionId],[NotedContractDefinitionId]) VALUES
(4, N'1101-101', N'Cash on Hand -', @CashOnHand, @cash_registersCD, NULL, NULL),
(6, N'1102-101', N'Petty Cash-', @CashOnHand, @petty_cash_fundsCD, NULL, NULL),
(8, N'1103-001', N'AIB Bole OD', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(9, N'1103-002', N'AIB Bole Special OD', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(10, N'1103-003', N'ECX AIB Bole Payout', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(11, N'1103-004', N'ECX AIB Bole Payin', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(12, N'1103-005', N'AIB Bole Current Account', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(13, N'1103-006', N'AIB Main', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(14, N'1103-007', N'AIB Dembela', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(15, N'1103-008', N'AIB Derartu Adebabay', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(16, N'1103-009', N'AIB 10% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(17, N'1103-010', N'AIB 90% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(18, N'1103-011', N'OIB Hora (Import)72035/1070502', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(19, N'1103-012', N'OIB Hora Current Account', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(20, N'1103-013', N'OIB Boset 23833/2010101/1/0', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(21, N'1103-014', N'OIB ECX Commission', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(22, N'1103-015', N'OIB ECX Payin', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(23, N'1103-016', N'OIB 10% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(24, N'1103-017', N'OIB 90% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(25, N'1103-018', N'OIBAdama 23833/2010101/16/0', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(26, N'1103-019', N'OIB Biftu 692369', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(27, N'1103-020', N'OIB Boset', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(28, N'1103-021', N'OIB Boset 10% 2011001/1/0', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(29, N'1103-022', N'OIB Boset 90% 2011201/1/0', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(30, N'1103-023', N'OIB Interest free Adama', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(31, N'1103-024', N'CBE A/Avenu', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(32, N'1103-025', N'CBE OUTSTANDING BALANCE', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(33, N'1103-026', N'OIB 82397', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(34, N'1103-027', N'CBE A/Avenu Pay out', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(35, N'1103-028', N'CBE A/Avenu Payin', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(36, N'1103-029', N'CBE 10% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(37, N'1103-030', N'CBE 90% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(38, N'1103-031', N'CBE 90% Old Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(39, N'1103-032', N'CBE A.A', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(40, N'1103-033', N'CBE Andnet', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(41, N'1103-034', N'CBE Abageda', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(42, N'1103-035', N'Cooprativ Bank Finfine', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(43, N'1103-036', N'Cooprativ Bank Adama', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(44, N'1103-037', N'Cooprativ Bank Hawass', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(45, N'1103-038', N'Cooprativ Bank 10% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(46, N'1103-039', N'Cooprativ Bank 90% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(47, N'1103-040', N'Nib Bole', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(48, N'1103-041', N'Nib 10% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(49, N'1103-042', N'Nib 90% Retention', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(50, N'1103-043', N'Dashen Bank', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(51, N'1103-044', N'Wegagen Bank A/Avenu', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(52, N'1103-045', N'Wegagen Bank Adama', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(53, N'1103-046', N'United Bank', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(54, N'1103-047', N'Zemen Bank', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(55, N'1103-048', N'OIB Loan Hor 72035/1070402/1/0', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(56, N'1103-049', N'OIB Hora 72035/2010101/1/0', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(57, N'1103-050', N'OibInt.fr Hora 27827/2010101/1', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(58, N'1103-051', N'Cooperative Finfinie Br/3747', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(59, N'1103-052', N'Cooperativ Bole Rwanda53532644', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(60, N'1103-053', N'Debub Global', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(61, N'1103-054', N'Abyssinia Bank Air Port Branch', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(62, N'1103-055', N'Abyssinya Bank of Abageda', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(63, N'1103-056', N'Aib Denbela', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(64, N'1103-057', N'CBE Nathret Branch', @BalancesWithBanks, @bank_accountsCD, NULL, NULL),
(67, N'1121-010', N'Trade Receivables', @CurrentTradeReceivables, @customersCD, NULL, NULL),
(68, N'1121-020', N'Receivables due from related parties', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @customersCD, NULL, NULL),
(69, N'1121-030', N'Prepayments', @CurrentPrepayments, @suppliersCD, NULL, NULL),
(70, N'1121-040', N'Accrued Income', @CurrentAccruedIncome, @customersCD, NULL, NULL),
(71, N'1121-050', N'Rent Receivables', @CurrentReceivablesFromRentalOfProperties, @customersCD, NULL, NULL),
(73, N'1206-001', N'VAT Receivables', @CurrentValueAddedTaxReceivables, NULL, NULL, @customersCD),
(74, N'1206-002', N'Withholding tax Receivables', @WithholdingTaxReceivablesExtension, NULL, NULL, @suppliersCD),
(76, N'1204-010', N'Deposit Bid Bond', @OtherCurrentReceivables, NULL, NULL, NULL),
(78, N'1202-010', N'Staff Debtors', @OtherCurrentFinancialAssets, @employeesCD, NULL, NULL),
(79, N'1205-010', N'Sundry Debtors', @OtherCurrentFinancialAssets, @debtorsCD, NULL, NULL),
(81, N'1209-001', N'Allowance for expected Credit Loss, receivables', @OtherCurrentReceivables, NULL, NULL, NULL),
(82, N'1209-002', N'Allowance for expected Credit Loss, loans', @OtherCurrentReceivables, NULL, NULL, NULL),
(86, N'1401-001', N'Fava beans', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(87, N'1401-002', N'White Pea beans', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(88, N'1401-003', N'Red Kidney beans', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(89, N'1401-004', N'Cream beans', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(90, N'1401-005', N'Light Speckled beans', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(91, N'1401-006', N'Black beans', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(92, N'1401-007', N'Light brown', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(93, N'1401-013', N'Green Mung beans', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(94, N'1401-016', N'Chickpeas', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(95, N'1401-017', N'Kabuli Chickpeas', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(96, N'1401-020', N'Soya beans', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(98, N'1401-021', N'Sesame Seeds', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(99, N'1401-022', N'Sunflower Seeds', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(100, N'1401-023', N'Linseeds', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(103, N'1401-030', N'Maize', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(104, N'1401-032', N'Wheat', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(105, N'1401-033', N'Barley', @RawMaterials, @warehousesCD, @raw_grainsRD, NULL),
(108, N'1402-001', N'White Pea beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(109, N'1402-002', N'Red Kidney beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(110, N'1402-003', N'Chickpeas', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(111, N'1402-004', N'Cream (Kidney) beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(112, N'1402-005', N'Soya beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(113, N'1402-006', N'Light Speckled (Kidney) beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(114, N'1402-007', N'Chick beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(115, N'1402-008', N'Black (Kidney) beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(116, N'1402-009', N'Light Brown (Kidney) beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(117, N'1402-010', N'Green Mung beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(118, N'1402-011', N'Kabuli Chickpeas', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(120, N'1402-021', N'Sesame Seeds', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(121, N'1402-022', N'Sunflower Seeds', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(122, N'1402-023', N'Linseeds', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(123, N'1402-024', N'Fava beans', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(125, N'1402-031', N'Maize', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(126, N'1402-032', N'Wheat', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(127, N'1402-033', N'Barley', @FinishedGoods, @warehousesCD, @finished_grainsRD, NULL),
(129, N'1403-001', N'White Pea beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(130, N'1403-002', N'Red Kidney beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(131, N'1403-003', N'Chickpeas', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(132, N'1403-004', N'Cream beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(133, N'1403-005', N'Soya beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(134, N'1403-006', N'Light Speckled beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(135, N'1403-007', N'Chick beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(136, N'1403-008', N'Black beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(137, N'1403-009', N'Light brown', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(138, N'1403-010', N'Green Mung beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(139, N'1403-011', N'Kabuli Chickpeas', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(140, N'1403-012', N'Sesame Seeds', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(141, N'1403-013', N'Sunflower Seeds', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(142, N'1403-014', N'Linseeds', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(143, N'1403-015', N'Fava beans', @OtherInventories, @warehousesCD, @byproducts_grainsRD, NULL),
(145, N'1404-001', N'Minidor Assembled', @FinishedGoods, @warehousesCD, @finished_vehiclesRD, NULL),
(147, N'1405-001', N'Oli Oil', @FinishedGoods, @warehousesCD, @finished_oilsRD, NULL),
(149, N'1406-001', N'Oil cake', @OtherInventories, @warehousesCD, @byproducts_oilsRD, NULL),
(155, N'1409-001,-002,etc', N'By major groups', @RawMaterials, @warehousesCD, NULL, NULL),
(157, N'1410-001', N'Sunflower', @RawMaterials, @warehousesCD, @raw_oilsRD, NULL),
(159, N'1411-001', N'Minidor (SKD)', @Merchandise, @warehousesCD, NULL, NULL),
(161, N'1412-001', N'Oil imported', @Merchandise, @warehousesCD, NULL, NULL),
(163, N'1413-001,002, etc', N'By major type of Spare parts', @Merchandise, @warehousesCD, NULL, NULL),
(165, N'14014-001,002, etc', N'By major type of Medicine', @Merchandise, @warehousesCD, @medicinesRD, NULL),
(167, N'1415-001,002, etc', N'By major type of Construction materials', @Merchandise, @warehousesCD, @construction_materialsRD, NULL),
(169, N'1416-001', N'PP bags & thread', @CurrentPackagingAndStorageMaterials, @warehousesCD, NULL, NULL),
(170, N'1416-002', N'Plastic Bottle of Oil', @CurrentPackagingAndStorageMaterials, @warehousesCD, NULL, NULL),
(171, N'1416-003', N'Containers & Other packing materials', @CurrentPackagingAndStorageMaterials, @warehousesCD, NULL, NULL),
(173, N'1417-001,002, etc', N'By type of Oil, Fuel & Lubricants', @CurrentFuel, @warehousesCD, NULL, NULL),
(175, N'1418-001', N'Other consumable goods', @OtherInventories, @warehousesCD, NULL, NULL),
(176, N'1418-002', N'Other Fumigation materials', @OtherInventories, @warehousesCD, NULL, NULL),
(177, N'1418-003', N'Uniform & Outfit', @OtherInventories, @warehousesCD, NULL, NULL),
(178, N'1418-004', N'Electrical Materials', @OtherInventories, @warehousesCD, NULL, NULL),
(179, N'1418-005', N'Other mateirals', @OtherInventories, @warehousesCD, NULL, NULL),
(181, N'1419-001,002, etc ', N'By type of medicine', @OtherInventories, @warehousesCD, @medicinesRD, NULL),
(185, N'1430-001', N'Vehcile /Minidor L/C # …..', @CurrentInventoriesInTransit, @foreign_importsCD, NULL, NULL),
(186, N'1430-002', N'Vehcile Spare parts L/C # …..', @CurrentInventoriesInTransit, @foreign_importsCD, NULL, NULL),
(187, N'1430-003', N'Oil L/C #', @CurrentInventoriesInTransit, @foreign_importsCD, NULL, NULL),
(188, N'1430-004', N'Machinery L/C #', @CurrentInventoriesInTransit, @foreign_importsCD, NULL, NULL),
(189, N'1430-005', N'Machinery spare parts L/C #', @CurrentInventoriesInTransit, @foreign_importsCD, NULL, NULL),
(190, N'1430-006', N'Stationery materials L/C #', @CurrentInventoriesInTransit, @foreign_importsCD, NULL, NULL),
(191, N'1430-007', N'Sanitation materials L/C #', @CurrentInventoriesInTransit, @foreign_importsCD, NULL, NULL),
(192, N'1430-008', N'Medicine', @CurrentInventoriesInTransit, @foreign_importsCD, @medicinesRD, NULL),
(193, N'1430-009', N'Other Imported Goods L/C #', @CurrentInventoriesInTransit, @foreign_importsCD, NULL, NULL),
(196, N'1431-001', N'White Pea beans,  permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(197, N'1431-002', N'Red Kidney beans , Permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(198, N'1431-003', N'Chickpeas, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(199, N'1431-004', N'Cream beans, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(200, N'1431-005', N'Soya beans, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(201, N'1431-006', N'Light Speckled beans, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(202, N'1431-007', N'Chick beans, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(203, N'1431-008', N'Black beans, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(204, N'1431-009', N'Light brown, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(205, N'1431-010', N'Green Mung beans, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(206, N'1431-011', N'Kabuli Chickpeas, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(208, N'1431-021', N'Sesame Seeds, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(209, N'1431-022', N'Sunflower Seeds, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(210, N'1431-023', N'Linseeds, permit #', @CurrentInventoriesInTransit, @foreign_exportsCD, @finished_grainsRD, NULL),
(212, N'1501-001,002,etc', N'By type of asset held forsale', @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners, NULL, NULL, NULL),
(215, N'1601-001', N'Hulling Machine', @Machinery, NULL, NULL, NULL),
(216, N'1601-002', N'Oil Mill Factory', @Machinery, NULL, NULL, NULL),
(217, N'1601.1', N'Acc.Dep.-Plant & Machinery', @Machinery, NULL, NULL, NULL),
(219, N'1602-001,002, etc', N'By type of factory equipmets', @Machinery, NULL, NULL, NULL),
(220, N'1602.2', N'Acc.Dep.-Factory Equipments', @Machinery, NULL, NULL, NULL),
(222, N'1603-001,-002,etc', N'By type of Motor Vehicles', @Vehicles, NULL, NULL, NULL),
(223, N'1603.3', N'Acc.Dep.- Motor Vehicles', @Vehicles, NULL, NULL, NULL),
(225, N'1604-001,002, etc', N'By type Of Furniture & Equipments', @OfficeEquipment, NULL, NULL, NULL),
(226, N'1604.4', N'Acc.Dep.-Off.Furniture&Equip.', @OfficeEquipment, NULL, NULL, NULL),
(228, N'1605-001,002,etc', N'By type of Computers & Accessories', @OfficeEquipment, NULL, NULL, NULL),
(229, N'1605.5', N'Acc.Dep.-Computers & Accesories', @OfficeEquipment, NULL, NULL, NULL);

EXEC [api].[Accounts__Save]
	@Entities = @Accounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting Accounts: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

IF (1=1) -- Declarations
BEGIN
	DECLARE @1101_101 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1101-101');
	DECLARE @1102_101 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1102-101');
	DECLARE @1103_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-001');
	DECLARE @1103_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-002');
	DECLARE @1103_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-003');
	DECLARE @1103_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-004');
	DECLARE @1103_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-005');
	DECLARE @1103_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-006');
	DECLARE @1103_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-007');
	DECLARE @1103_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-008');
	DECLARE @1103_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-009');
	DECLARE @1103_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-010');
	DECLARE @1103_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-011');
	DECLARE @1103_012 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-012');
	DECLARE @1103_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-013');
	DECLARE @1103_014 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-014');
	DECLARE @1103_015 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-015');
	DECLARE @1103_016 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-016');
	DECLARE @1103_017 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-017');
	DECLARE @1103_018 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-018');
	DECLARE @1103_019 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-019');
	DECLARE @1103_020 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-020');
	DECLARE @1103_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-021');
	DECLARE @1103_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-022');
	DECLARE @1103_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-023');
	DECLARE @1103_024 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-024');
	DECLARE @1103_025 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-025');
	DECLARE @1103_026 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-026');
	DECLARE @1103_027 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-027');
	DECLARE @1103_028 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-028');
	DECLARE @1103_029 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-029');
	DECLARE @1103_030 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-030');
	DECLARE @1103_031 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-031');
	DECLARE @1103_032 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-032');
	DECLARE @1103_033 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-033');
	DECLARE @1103_034 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-034');
	DECLARE @1103_035 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-035');
	DECLARE @1103_036 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-036');
	DECLARE @1103_037 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-037');
	DECLARE @1103_038 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-038');
	DECLARE @1103_039 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-039');
	DECLARE @1103_040 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-040');
	DECLARE @1103_041 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-041');
	DECLARE @1103_042 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-042');
	DECLARE @1103_043 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-043');
	DECLARE @1103_044 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-044');
	DECLARE @1103_045 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-045');
	DECLARE @1103_046 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-046');
	DECLARE @1103_047 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-047');
	DECLARE @1103_048 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-048');
	DECLARE @1103_049 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-049');
	DECLARE @1103_050 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-050');
	DECLARE @1103_051 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-051');
	DECLARE @1103_052 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-052');
	DECLARE @1103_053 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-053');
	DECLARE @1103_054 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-054');
	DECLARE @1103_055 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-055');
	DECLARE @1103_056 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-056');
	DECLARE @1103_057 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-057');
	DECLARE @1121_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-010');
	DECLARE @1121_020 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-020');
	DECLARE @1121_030 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-030');
	DECLARE @1121_040 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-040');
	DECLARE @1121_050 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-050');
	DECLARE @1206_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1206-001');
	DECLARE @1206_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1206-002');
	DECLARE @1204_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1204-010');
	DECLARE @1202_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1202-010');
	DECLARE @1205_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1205-010');
	DECLARE @1209_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1209-001');
	DECLARE @1209_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1209-002');
	DECLARE @1401_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-001');
	DECLARE @1401_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-002');
	DECLARE @1401_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-003');
	DECLARE @1401_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-004');
	DECLARE @1401_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-005');
	DECLARE @1401_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-006');
	DECLARE @1401_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-007');
	DECLARE @1401_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-013');
	DECLARE @1401_016 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-016');
	DECLARE @1401_017 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-017');
	DECLARE @1401_020 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-020');
	DECLARE @1401_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-021');
	DECLARE @1401_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-022');
	DECLARE @1401_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-023');
	DECLARE @1401_030 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-030');
	DECLARE @1401_032 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-032');
	DECLARE @1401_033 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-033');
	DECLARE @1402_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-001');
	DECLARE @1402_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-002');
	DECLARE @1402_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-003');
	DECLARE @1402_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-004');
	DECLARE @1402_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-005');
	DECLARE @1402_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-006');
	DECLARE @1402_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-007');
	DECLARE @1402_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-008');
	DECLARE @1402_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-009');
	DECLARE @1402_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-010');
	DECLARE @1402_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-011');
	DECLARE @1402_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-021');
	DECLARE @1402_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-022');
	DECLARE @1402_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-023');
	DECLARE @1402_024 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-024');
	DECLARE @1402_031 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-031');
	DECLARE @1402_032 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-032');
	DECLARE @1402_033 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-033');
	DECLARE @1403_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-001');
	DECLARE @1403_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-002');
	DECLARE @1403_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-003');
	DECLARE @1403_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-004');
	DECLARE @1403_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-005');
	DECLARE @1403_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-006');
	DECLARE @1403_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-007');
	DECLARE @1403_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-008');
	DECLARE @1403_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-009');
	DECLARE @1403_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-010');
	DECLARE @1403_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-011');
	DECLARE @1403_012 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-012');
	DECLARE @1403_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-013');
	DECLARE @1403_014 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-014');
	DECLARE @1403_015 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-015');
	DECLARE @1404_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1404-001');
	DECLARE @1405_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1405-001');
	DECLARE @1406_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1406-001');
	DECLARE @1410_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1410-001');
	DECLARE @1411_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1411-001');
	DECLARE @1412_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1412-001');
	DECLARE @1416_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1416-001');
	DECLARE @1416_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1416-002');
	DECLARE @1416_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1416-003');
	DECLARE @1418_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-001');
	DECLARE @1418_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-002');
	DECLARE @1418_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-003');
	DECLARE @1418_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-004');
	DECLARE @1418_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-005');
	DECLARE @1430_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-001');
	DECLARE @1430_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-002');
	DECLARE @1430_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-003');
	DECLARE @1430_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-004');
	DECLARE @1430_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-005');
	DECLARE @1430_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-006');
	DECLARE @1430_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-007');
	DECLARE @1430_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-008');
	DECLARE @1430_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-009');
	DECLARE @1431_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-001');
	DECLARE @1431_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-002');
	DECLARE @1431_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-003');
	DECLARE @1431_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-004');
	DECLARE @1431_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-005');
	DECLARE @1431_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-006');
	DECLARE @1431_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-007');
	DECLARE @1431_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-008');
	DECLARE @1431_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-009');
	DECLARE @1431_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-010');
	DECLARE @1431_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-011');
	DECLARE @1431_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-021');
	DECLARE @1431_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-022');
	DECLARE @1431_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-023');
END