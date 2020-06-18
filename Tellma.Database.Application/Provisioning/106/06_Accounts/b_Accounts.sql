INSERT INTO @Accounts([Index], [Name], [Code], [AccountTypeId]) VALUES
(1111001, N'Cash Account 1', N'1111-001', @CashOnHand),
(1112001, N'Bank Account 1', N'1112-001', @BalancesWithBanks),
(1112002, N'Bank Account 2', N'1112-002', @BalancesWithBanks),
(1122000, N'Current receivable from related parties - SP', N'1122-000', @TradeAndOtherCurrentReceivablesDueFromRelatedParties),
(1122001, N'Related Party 1', N'1122-001', @TradeAndOtherCurrentReceivablesDueFromRelatedParties),
(1122002, N'Related Party 2', N'1122-002', @TradeAndOtherCurrentReceivablesDueFromRelatedParties),
(1128001, N'ECL Allowance, Current trade receivable', N'1128-001', @AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension),
(1128002, N'ECL Allowance, Related Parties receivable', N'1128-002', @AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension),
(1128003, N'ECL Allowance, prepayments', N'1128-003', @AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension),
(1128004, N'ECL Allowance, accrued income', N'1128-004', @AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension),
(1171100, N'Raw Grains - SP', N'1171-100', @RawMaterials),
(1171101, N'Raw Grains - Type 1', N'1171-101', @RawMaterials),
(1171102, N'Raw Grains - Type 2', N'1171-102', @RawMaterials),
(1171200, N'Vehicle Components - SP', N'1171-200', @RawMaterials),
(1171201, N'Vehicle Components - Type 1', N'1171-201', @RawMaterials),
(1171202, N'Vehicle Components - Type 2', N'1171-202', @RawMaterials),
(1172000, N'Merchandise - SP', N'1172-000', @Merchandise),
(1172001, N'Medicine', N'1172-001', @Merchandise),
(1172002, N'Construction Materials', N'1172-002', @Merchandise),
(1172003, N'Spare Parts (for Sale)', N'1172-003', @Merchandise),
(1174000, N'Finished Goods - SP', N'1174-000', @FinishedGoods),
(1174001, N'Cleaned Grains', N'1174-001', @FinishedGoods),
(1174002, N'Processed Oil', N'1174-002', @FinishedGoods),
(1174003, N'Assembled Vehicles', N'1174-003', @FinishedGoods),
(1175000, N'Packaging and Storage Materials - SP', N'1175-000', @CurrentPackagingAndStorageMaterials),
(1176000, N'Spare Parts - SP', N'1176-000', @SpareParts),
(1176001, N'Spare Parts (Not for sale)', N'1176-001', @SpareParts),
(1177100, N'Import shipments - SP', N'1177-100', @CurrentInventoriesInTransit),
(1177101, N'Import shipment - LC#1', N'1177-101', @CurrentInventoriesInTransit),
(1177102, N'Import shipment - LC#2', N'1177-102', @CurrentInventoriesInTransit),
(1177200, N'Export shipments - SP', N'1177-200', @CurrentInventoriesInTransit),
(1177201, N'Export Shipment - Permit #1', N'1177-201', @CurrentInventoriesInTransit),
(1177202, N'Export Shipment - Permit #2', N'1177-202', @CurrentInventoriesInTransit),
(2126001, N'Current value added tax payables', N'2126-001', @CurrentValueAddedTaxPayables),
(2126002, N'Current excise tax payables', N'2126-002', @CurrentExciseTaxPayables),
(2126003, N'Current Social Security payables', N'2126-003', @CurrentSocialSecurityPayablesExtension),
(2126005, N'Employee Income Tax Payable', N'2126-005', @CurrentEmployeeIncomeTaxPayablesExtension)



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