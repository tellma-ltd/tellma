INSERT INTO @Accounts([Index],[Code],[Name], [AccountTypeId], [ContractDefinitionId],[ResourceDefinitionId],[CurrencyId],[CenterId],[NotedContractDefinitionId]) VALUES
(4, N'1101-101', N'Petty Cash - Bizunesh Birhanu', @CashOnHand, @CashOnHandAccountCD, NULL, @ETB,@106C_BoleHQSGNA,NULL),
(6, N'1102-101', N'Petty Cash - Amanuel Bayissa', @CashOnHand, @CashOnHandAccountCD, NULL, @ETB,@106C_SoretiMall,NULL),
(8, N'1103-001', N'AIB Bole OD', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(9, N'1103-002', N'AIB Bole Special OD', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(10, N'1103-003', N'ECX AIB Bole Payout', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(11, N'1103-004', N'ECX AIB Bole Payin', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(12, N'1103-005', N'AIB Bole Current Account', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(13, N'1103-006', N'AIB Main', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(14, N'1103-007', N'AIB Dembela', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(15, N'1103-008', N'AIB Derartu Adebabay', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(16, N'1103-009', N'AIB 10% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @USD,@106C_SoretiUnspecified,NULL),
(17, N'1103-010', N'AIB 90% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(18, N'1103-011', N'OIB Hora (Import)72035/1070502', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(19, N'1103-012', N'OIB Hora Current Account', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(20, N'1103-013', N'OIB Boset 23833/2010101/1/0', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(21, N'1103-014', N'OIB ECX Commission', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(22, N'1103-015', N'OIB ECX Payin', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(23, N'1103-016', N'OIB 10% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @USD,@106C_SoretiUnspecified,NULL),
(24, N'1103-017', N'OIB 90% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(25, N'1103-018', N'OIBAdama 23833/2010101/16/0', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(26, N'1103-019', N'OIB Biftu 692369', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(27, N'1103-020', N'OIB Boset', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(28, N'1103-021', N'OIB Boset 10% 2011001/1/0', @BalancesWithBanks, @BankAccountCD, NULL, @USD,@106C_SoretiUnspecified,NULL),
(29, N'1103-022', N'OIB Boset 90% 2011201/1/0', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(30, N'1103-023', N'OIB Interest free Adama', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(31, N'1103-024', N'CBE A/Avenu', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(32, N'1103-025', N'CBE OUTSTANDING BALANCE', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(33, N'1103-026', N'OIB 82397', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(34, N'1103-027', N'CBE A/Avenu Pay out', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(35, N'1103-028', N'CBE A/Avenu Payin', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(36, N'1103-029', N'CBE 10% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @USD,@106C_SoretiUnspecified,NULL),
(37, N'1103-030', N'CBE 90% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(38, N'1103-031', N'CBE 90% Old Retention', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(39, N'1103-032', N'CBE A.A', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(40, N'1103-033', N'CBE Andnet', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(41, N'1103-034', N'CBE Abageda', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(42, N'1103-035', N'Cooprativ Bank Finfine', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(43, N'1103-036', N'Cooprativ Bank Adama', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(44, N'1103-037', N'Cooprativ Bank Hawass', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(45, N'1103-038', N'Cooprativ Bank 10% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @USD,@106C_SoretiUnspecified,NULL),
(46, N'1103-039', N'Cooprativ Bank 90% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(47, N'1103-040', N'Nib Bole', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(48, N'1103-041', N'Nib 10% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @USD,@106C_SoretiUnspecified,NULL),
(49, N'1103-042', N'Nib 90% Retention', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(50, N'1103-043', N'Dashen Bank', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(51, N'1103-044', N'Wegagen Bank A/Avenu', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(52, N'1103-045', N'Wegagen Bank Adama', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(53, N'1103-046', N'United Bank', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(54, N'1103-047', N'Zemen Bank', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(55, N'1103-048', N'OIB Loan Hor 72035/1070402/1/0', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(56, N'1103-049', N'OIB Hora 72035/2010101/1/0', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(57, N'1103-050', N'OibInt.fr Hora 27827/2010101/1', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(58, N'1103-051', N'Cooperative Finfinie Br/3747', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(59, N'1103-052', N'Cooperativ Bole Rwanda53532644', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(60, N'1103-053', N'Debub Global', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(61, N'1103-054', N'Abyssinia Bank Air Port Branch', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(62, N'1103-055', N'Abyssinya Bank of Abageda', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(63, N'1103-056', N'Aib Denbela', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(64, N'1103-057', N'CBE Nathret Branch', @BalancesWithBanks, @BankAccountCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(67, N'1121-010', N'Trade Receivables', @CurrentTradeReceivables, @CustomerCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(68, N'1121-020', N'Receivables due from related parties', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @CustomerCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(69, N'1121-030', N'Prepayments', @CurrentPrepayments, @SupplierCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(70, N'1121-040', N'Accrued Income', @CurrentAccruedIncome, @CustomerCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(71, N'1121-050', N'Rent Receivables', @CurrentReceivablesFromRentalOfProperties, @CustomerCD, NULL, @ETB,@106C_SoretiMall,NULL),
(73, N'1206-001', N'VAT Receivables', @CurrentValueAddedTaxReceivables, NULL, NULL, @ETB,@106C_SoretiUnspecified,@CustomerCD),
(74, N'1206-002', N'Withholding tax Receivables', @WithholdingTaxReceivablesExtension, NULL, NULL, @ETB,@106C_SoretiUnspecified,@SupplierCD),
(76, N'1204-010', N'Deposit Bid Bond', @OtherCurrentReceivables, NULL, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(78, N'1202-010', N'Staff Debtors', @OtherCurrentFinancialAssets, @EmployeeCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(79, N'1205-010', N'Sundry Debtors', @OtherCurrentFinancialAssets, @DebtorCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(81, N'1209-001', N'Allowance for expected Credit Loss, receivables', @OtherCurrentReceivables, NULL, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(82, N'1209-002', N'Allowance for expected Credit Loss, loans', @OtherCurrentReceivables, NULL, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(86, N'1401-001', N'Fava beans', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(87, N'1401-002', N'White Pea beans', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(88, N'1401-003', N'Red Kidney beans', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(89, N'1401-004', N'Cream beans', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(90, N'1401-005', N'Light Speckled beans', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(91, N'1401-006', N'Black beans', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(92, N'1401-007', N'Light brown', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(93, N'1401-013', N'Green Mung beans', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(94, N'1401-016', N'Chickpeas', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(95, N'1401-017', N'Kabuli Chickpeas', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(96, N'1401-020', N'Soya beans', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(98, N'1401-021', N'Sesame Seeds', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(99, N'1401-022', N'Sunflower Seeds', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(100, N'1401-023', N'Linseeds', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(103, N'1401-030', N'Maize', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(104, N'1401-032', N'Wheat', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(105, N'1401-033', N'Barley', @RawMaterials, @WarehouseCD, @RawGrainRD, @ETB,@106C_AdamaFactory,NULL),
(108, N'1402-001', N'White Pea beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(109, N'1402-002', N'Red Kidney beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(110, N'1402-003', N'Chickpeas', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(111, N'1402-004', N'Cream (Kidney) beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(112, N'1402-005', N'Soya beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(113, N'1402-006', N'Light Speckled (Kidney) beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(114, N'1402-007', N'Chick beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(115, N'1402-008', N'Black (Kidney) beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(116, N'1402-009', N'Light Brown (Kidney) beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(117, N'1402-010', N'Green Mung beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(118, N'1402-011', N'Kabuli Chickpeas', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(120, N'1402-021', N'Sesame Seeds', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(121, N'1402-022', N'Sunflower Seeds', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(122, N'1402-023', N'Linseeds', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(123, N'1402-024', N'Fava beans', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(125, N'1402-031', N'Maize', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(126, N'1402-032', N'Wheat', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(127, N'1402-033', N'Barley', @FinishedGoods, @WarehouseCD, @FinishedGrainRD, @ETB,@106C_AdamaFactory,NULL),
(129, N'1403-001', N'White Pea beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(130, N'1403-002', N'Red Kidney beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(131, N'1403-003', N'Chickpeas', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(132, N'1403-004', N'Cream beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(133, N'1403-005', N'Soya beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(134, N'1403-006', N'Light Speckled beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(135, N'1403-007', N'Chick beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(136, N'1403-008', N'Black beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(137, N'1403-009', N'Light brown', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(138, N'1403-010', N'Green Mung beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(139, N'1403-011', N'Kabuli Chickpeas', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(140, N'1403-012', N'Sesame Seeds', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(141, N'1403-013', N'Sunflower Seeds', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(142, N'1403-014', N'Linseeds', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(143, N'1403-015', N'Fava beans', @OtherInventories, @WarehouseCD, @ByproductGrainRD, @ETB,@106C_AdamaFactory,NULL),
(145, N'1404-001', N'Minidor Assembled', @FinishedGoods, @WarehouseCD, @FinishedVehicleRD, @ETB,@106C_MinidorFactory,NULL),
(147, N'1405-001', N'Oli Oil', @FinishedGoods, @WarehouseCD, @FinishedOilRD, @ETB,@106C_AdamaFactory,NULL),
(149, N'1406-001', N'Oil cake', @OtherInventories, @WarehouseCD, @ByproductOilRD, @ETB,@106C_AdamaFactory,NULL),
(155, N'1409-001,-002,etc', N'By major groups', @RawMaterials, @WarehouseCD, NULL, @ETB,@106C_MinidorFactory,NULL),
(157, N'1410-001', N'Sunflower', @RawMaterials, @WarehouseCD, @RawOilRD, @ETB,@106C_AdamaFactory,NULL),
(159, N'1411-001', N'Minidor (SKD)', @Merchandise, @WarehouseCD, NULL, @ETB,@106C_MinidorFactory,NULL),
(161, N'1412-001', N'Oil imported', @Merchandise, @WarehouseCD, NULL, @ETB,@106C_ImportedMerchandise,NULL),
(163, N'1413-001,002, etc', N'By major type of Spare parts', @Merchandise, @WarehouseCD, NULL, @ETB,@106C_ImportedMerchandise,NULL),
(165, N'14014-001,002, etc', N'By major type of Medicine', @Merchandise, @WarehouseCD, @MedicineRD, @ETB,@106C_ImportedMerchandise,NULL),
(167, N'1415-001,002, etc', N'By major type of Construction materials', @Merchandise, @WarehouseCD, @ConstructionMaterialRD, @ETB,@106C_ImportedMerchandise,NULL),
(169, N'1416-001', N'PP bags & thread', @CurrentPackagingAndStorageMaterials, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(170, N'1416-002', N'Plastic Bottle of Oil', @CurrentPackagingAndStorageMaterials, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(171, N'1416-003', N'Containers & Other packing materials', @CurrentPackagingAndStorageMaterials, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(173, N'1417-001,002, etc', N'By type of Oil, Fuel & Lubricants', @CurrentFuel, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(175, N'1418-001', N'Other consumable goods', @OtherInventories, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(176, N'1418-002', N'Other Fumigation materials', @OtherInventories, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(177, N'1418-003', N'Uniform & Outfit', @OtherInventories, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(178, N'1418-004', N'Electrical Materials', @OtherInventories, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(179, N'1418-005', N'Other mateirals', @OtherInventories, @WarehouseCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(181, N'1419-001,002, etc ', N'By type of medicine', @OtherInventories, @WarehouseCD, @MedicineRD, @ETB,@106C_ImportedMerchandise,NULL),
(185, N'1430-001', N'Vehcile /Minidor L/C # …..', @CurrentInventoriesInTransit, @ImportShipmentCD, NULL, @ETB,@106C_MinidorFactory,NULL),
(186, N'1430-002', N'Vehcile Spare parts L/C # …..', @CurrentInventoriesInTransit, @ImportShipmentCD, NULL, @ETB,@106C_MinidorFactory,NULL),
(187, N'1430-003', N'Oil L/C #', @CurrentInventoriesInTransit, @ImportShipmentCD, NULL, @ETB,@106C_ImportedMerchandise,NULL),
(188, N'1430-004', N'Machinery L/C #', @CurrentInventoriesInTransit, @ImportShipmentCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(189, N'1430-005', N'Machinery spare parts L/C #', @CurrentInventoriesInTransit, @ImportShipmentCD, NULL, @ETB,@106C_AdamaFactory,NULL),
(190, N'1430-006', N'Stationery materials L/C #', @CurrentInventoriesInTransit, @ImportShipmentCD, NULL, @ETB,@106C_SoretiUnspecified,NULL),
(191, N'1430-007', N'Sanitation materials L/C #', @CurrentInventoriesInTransit, @ImportShipmentCD, NULL, @ETB,@106C_ImportedMerchandise,NULL),
(192, N'1430-008', N'Medicine', @CurrentInventoriesInTransit, @ImportShipmentCD, @MedicineRD, @ETB,@106C_ImportedMerchandise,NULL)



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
DECLARE @1601_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1601-001');
DECLARE @1601_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1601-002');
DECLARE @1619_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1619-001');
DECLARE @1619_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1619-002');
DECLARE @1619_099 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1619-099');
DECLARE @1801_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1801-010');
DECLARE @1801_020 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1801-020');
DECLARE @1802_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1802-001');
DECLARE @1802_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1802-002');
DECLARE @1802_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1802-003');
DECLARE @1802_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1802-004');
DECLARE @1803_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1803-010');
DECLARE @1805_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1805-001');
DECLARE @2201_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2201-001');
DECLARE @2201_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2201-002');
DECLARE @2201_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2201-003');
DECLARE @2201_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2201-004');
DECLARE @2301 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301');
DECLARE @2301_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-001');
DECLARE @2301_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-002');
DECLARE @2301_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-003');
DECLARE @2301_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-004');
DECLARE @2301_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-005');
DECLARE @2301_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-006');
DECLARE @2301_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-007');
DECLARE @2401 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2401');
DECLARE @2402 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402');
DECLARE @2402_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-001');
DECLARE @2402_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-002');
DECLARE @2402_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-003');
DECLARE @2402_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-004');
DECLARE @2402_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-005');
DECLARE @2402_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-006');
DECLARE @2402_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-007');
DECLARE @2402_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-008');
DECLARE @2402_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-009');
DECLARE @2402_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-010');
DECLARE @2402_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-011');
DECLARE @2402_099 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-099');
DECLARE @2501 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2501');
DECLARE @2502 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2502');
DECLARE @2503 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2503');
DECLARE @2504 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2504');
DECLARE @2601 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2601');
DECLARE @2701 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2701');
DECLARE @2702 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2702');
DECLARE @2703  INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2703 ');
DECLARE @2801 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2801');
DECLARE @2802 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2802');
DECLARE @2803 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2803');
DECLARE @2901 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2901');
DECLARE @3101_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'3101-001');
DECLARE @3102_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'3102-001');
DECLARE @3103_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'3103-001');
DECLARE @3909_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'3909-001');
DECLARE @4101_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4101-001');
DECLARE @4101_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4101-002');
DECLARE @4101_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4101-003');
DECLARE @4102_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-001');
DECLARE @4102_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-002');
DECLARE @4102_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-003');
DECLARE @4102_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-004');
DECLARE @4102_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-005');
DECLARE @4102_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-006');
DECLARE @4102_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-007');
DECLARE @4102_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-008');
DECLARE @4102_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-009');
DECLARE @4102_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-010');
DECLARE @4102_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-011');
DECLARE @4103_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4103-001');
DECLARE @4103_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4103-002');
DECLARE @4103_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4103-003');
DECLARE @4103_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4103-004');
DECLARE @4104_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4104-001');
DECLARE @4104_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4104-002');
DECLARE @4105_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4105-001');
DECLARE @4105_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4105-002');
DECLARE @4201_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4201-001');
DECLARE @4201_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4201-002');
DECLARE @4909_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-001');
DECLARE @4909_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-002');
DECLARE @4909_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-003');
DECLARE @4909_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-004');
DECLARE @4909_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-005');
DECLARE @5101_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-001');
DECLARE @5101_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-002');
DECLARE @5101_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-003');
DECLARE @5101_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-004');
DECLARE @5101_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-005');
DECLARE @5101_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-006');
DECLARE @5011_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5011-007');
DECLARE @5101_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-008');
DECLARE @5101_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-009');
DECLARE @5101_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-010');
DECLARE @5101_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-011');
DECLARE @5102_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5102-021');
DECLARE @5102_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5102-022');
DECLARE @5102_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5102-023');
DECLARE @5102_024 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5102-024');
DECLARE @5103_031 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5103-031');
DECLARE @5103_032 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5103-032');
DECLARE @5103_033 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5103-033');
DECLARE @5120_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-001');
DECLARE @5120_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-002');
DECLARE @5120_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-003');
DECLARE @5120_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-004');
DECLARE @5120_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-005');
DECLARE @5120_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-006');
DECLARE @5120_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-007');
DECLARE @5120_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-008');
DECLARE @5120_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-009');
DECLARE @5120_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-010');
DECLARE @5120_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-011');
DECLARE @5202_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-001');
DECLARE @5202_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-002');
DECLARE @5202_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-003');
DECLARE @5202_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-004');
DECLARE @5202_099 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-099');
DECLARE @5302_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-001');
DECLARE @5302_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-002');
DECLARE @5302_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-003');
DECLARE @5302_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-004');
DECLARE @5302_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-005');
DECLARE @5302_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-006');
DECLARE @5302_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-007');
DECLARE @5302_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-008');
DECLARE @5302_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-009');
DECLARE @5302_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-010');
DECLARE @5302_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-011');
DECLARE @5302_012 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-012');
DECLARE @5302_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-013');
DECLARE @5302_014 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-014');
DECLARE @5303_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-001');
DECLARE @5303_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-002');
DECLARE @5303_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-003');
DECLARE @5303_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-004');
DECLARE @5303_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-005');
DECLARE @5303_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-006');
DECLARE @5303_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-007');
DECLARE @5303_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-008');
DECLARE @5402_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-001');
DECLARE @5402_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-002');
DECLARE @5402_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-003');
DECLARE @5402_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-004');
DECLARE @5402_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-005');
DECLARE @5402_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-006');
DECLARE @5402_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-007');
DECLARE @5402_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-008');
DECLARE @5402_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-009');
DECLARE @5402_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-010');
DECLARE @5402_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-011');
DECLARE @5402_012 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-012');
DECLARE @5402_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-013');
DECLARE @5402_014 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-014');
DECLARE @5403_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-001');
DECLARE @5403_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-002');
DECLARE @5403_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-003');
DECLARE @5403_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-004');
DECLARE @5403_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-005');
DECLARE @5403_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-006');

END