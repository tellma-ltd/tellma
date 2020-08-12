-- De-Activate Unneeded Entry Types


-- Activate Currencies in Use
INSERT INTO @IndexedCurrencyIds
([Index],	[Id]) VALUES
(0,			@SDG),
(1,			@USD);

EXEC [api].[Currencies__Activate]
	@IndexedIds = @IndexedCurrencyIds,
	@IsActive = 1,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies Activating: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- Add exchange Rates
INSERT INTO @ExchangeRates([Index],[CurrencyId], [ValidAsOf], [AmountInCurrency], [AmountInFunctional]) VAlUES
(0,N'SDG', N'2016-12-31', 150,1),
(1,N'SDG', N'2017-12-31', 200,1);
--(2,N'SDG', N'2016-07-14', 109,1),
--(3,N'SDG', N'2016-07-21', 108,1),
--(4,N'SDG', N'2016-07-28', 111,1),
--(5,N'SDG', N'2016-08-04', 110,1),
--(6,N'SDG', N'2016-08-11', 108,1),
--(7,N'SDG', N'2016-08-18', 106,1),
--(8,N'SDG', N'2016-08-25', 112,1),
--(9,N'SDG', N'2016-09-01', 110,1),
--(10,N'SDG', N'2016-09-08', 113,1),
--(11,N'SDG', N'2016-09-15', 117,1),
--(12,N'SDG', N'2016-09-22', 116,1),
--(13,N'SDG', N'2016-09-29', 118,1),
--(14,N'SDG', N'2016-10-06', 121,1),
--(15,N'SDG', N'2016-10-13', 125,1),
--(16,N'SDG', N'2016-10-20', 127,1),
--(17,N'SDG', N'2016-10-27', 133,1),
--(18,N'SDG', N'2016-11-03', 134,1),
--(19,N'SDG', N'2016-11-10', 139,1),
--(20,N'SDG', N'2016-11-17', 144,1),
--(21,N'SDG', N'2016-11-24', 150,1),
--(22,N'SDG', N'2016-12-01', 149,1),
--(23,N'SDG', N'2016-12-08', 151,1),
--(24,N'SDG', N'2016-12-15', 152,1),
--(25,N'SDG', N'2016-12-22', 155,1),
--(26,N'SDG', N'2016-12-29', 158,1),
--(27,N'SDG', N'2017-01-05', 162,1),
--(28,N'SDG', N'2017-01-12', 167,1),
--(29,N'SDG', N'2017-01-19', 166,1),
--(30,N'SDG', N'2017-01-26', 171,1),
--(31,N'SDG', N'2017-02-02', 173,1),
--(32,N'SDG', N'2017-02-09', 179,1),
--(33,N'SDG', N'2017-02-16', 182,1),
--(34,N'SDG', N'2017-02-23', 185,1),
--(35,N'SDG', N'2017-03-02', 190,1),
--(36,N'SDG', N'2017-03-09', 195,1),
--(37,N'SDG', N'2017-03-16', 193,1),
--(38,N'SDG', N'2017-03-23', 194,1),
--(39,N'SDG', N'2017-03-30', 196,1),
--(40,N'SDG', N'2017-04-06', 199,1),
--(41,N'SDG', N'2017-04-13', 200,1),
--(42,N'SDG', N'2017-04-20', 201,1),
--(43,N'SDG', N'2017-04-27', 206,1),
--(44,N'SDG', N'2017-05-04', 204,1),
--(45,N'SDG', N'2017-05-11', 207,1),
--(46,N'SDG', N'2017-05-18', 207,1),
--(47,N'SDG', N'2017-05-25', 213,1),
--(48,N'SDG', N'2017-06-01', 216,1),
--(49,N'SDG', N'2017-06-08', 221,1),
--(50,N'SDG', N'2017-06-15', 223,1),
--(51,N'SDG', N'2017-06-22', 224,1),
--(52,N'SDG', N'2017-06-29', 226,1),
--(53,N'SDG', N'2017-07-06', 225,1),
--(54,N'SDG', N'2017-07-13', 229,1),
--(55,N'SDG', N'2017-07-20', 230,1),
--(56,N'SDG', N'2017-07-27', 235,1),
--(57,N'SDG', N'2017-08-03', 238,1),
--(58,N'SDG', N'2017-08-10', 236,1),
--(59,N'SDG', N'2017-08-17', 235,1),
--(60,N'SDG', N'2017-08-24', 240,1),
--(61,N'SDG', N'2017-08-31', 240,1),
--(62,N'SDG', N'2017-09-07', 238,1),
--(63,N'SDG', N'2017-09-14', 244,1),
--(64,N'SDG', N'2017-09-21', 247,1),
--(65,N'SDG', N'2017-09-28', 246,1),
--(66,N'SDG', N'2017-10-05', 245,1),
--(67,N'SDG', N'2017-10-12', 251,1),
--(68,N'SDG', N'2017-10-19', 253,1),
--(69,N'SDG', N'2017-10-26', 253,1),
--(70,N'SDG', N'2017-11-02', 251,1),
--(71,N'SDG', N'2017-11-09', 254,1),
--(72,N'SDG', N'2017-11-16', 259,1),
--(73,N'SDG', N'2017-11-23', 261,1),
--(74,N'SDG', N'2017-11-30', 267,1),
--(75,N'SDG', N'2017-12-07', 267,1),
--(76,N'SDG', N'2017-12-14', 273,1),
--(77,N'SDG', N'2017-12-21', 275,1),
--(78,N'SDG', N'2017-12-28', 281,1),
--(79,N'SDG', N'2018-01-04', 285,1),
--(80,N'SDG', N'2018-01-11', 290,1),
--(81,N'SDG', N'2018-01-18', 291,1),
--(82,N'SDG', N'2018-01-25', 292,1),
--(83,N'SDG', N'2018-02-01', 290,1),
--(84,N'SDG', N'2018-02-08', 292,1),
--(85,N'SDG', N'2018-02-15', 294,1),
--(86,N'SDG', N'2018-02-22', 300,1),
--(87,N'SDG', N'2018-03-01', 301,1),
--(88,N'SDG', N'2018-03-08', 304,1),
--(89,N'SDG', N'2018-03-15', 304,1),
--(90,N'SDG', N'2018-03-22', 305,1),
--(91,N'SDG', N'2018-03-29', 307,1),
--(92,N'SDG', N'2018-04-05', 310,1),
--(93,N'SDG', N'2018-04-12', 309,1),
--(94,N'SDG', N'2018-04-19', 315,1),
--(95,N'SDG', N'2018-04-26', 320,1),
--(96,N'SDG', N'2018-05-03', 324,1),
--(97,N'SDG', N'2018-05-10', 329,1),
--(98,N'SDG', N'2018-05-17', 330,1),
--(99,N'SDG', N'2018-05-24', 333,1),
--(100,N'SDG', N'2018-05-31', 332,1),
--(101,N'SDG', N'2018-06-07', 334,1),
--(102,N'SDG', N'2018-06-14', 335,1),
--(103,N'SDG', N'2018-06-21', 338,1),
--(104,N'SDG', N'2018-06-28', 341,1),
--(105,N'SDG', N'2018-07-05', 342,1),
--(106,N'SDG', N'2018-07-12', 344,1),
--(107,N'SDG', N'2018-07-19', 343,1),
--(108,N'SDG', N'2018-07-26', 346,1),
--(109,N'SDG', N'2018-08-02', 347,1),
--(110,N'SDG', N'2018-08-09', 346,1),
--(111,N'SDG', N'2018-08-16', 352,1),
--(112,N'SDG', N'2018-08-23', 358,1),
--(113,N'SDG', N'2018-08-30', 363,1),
--(114,N'SDG', N'2018-09-06', 367,1),
--(115,N'SDG', N'2018-09-13', 365,1),
--(116,N'SDG', N'2018-09-20', 364,1),
--(117,N'SDG', N'2018-09-27', 366,1),
--(118,N'SDG', N'2018-10-04', 367,1),
--(119,N'SDG', N'2018-10-11', 370,1),
--(120,N'SDG', N'2018-10-18', 376,1),
--(121,N'SDG', N'2018-10-25', 374,1),
--(122,N'SDG', N'2018-11-01', 376,1),
--(123,N'SDG', N'2018-11-08', 378,1),
--(124,N'SDG', N'2018-11-15', 384,1),
--(125,N'SDG', N'2018-11-22', 389,1),
--(126,N'SDG', N'2018-11-29', 387,1),
--(127,N'SDG', N'2018-12-06', 393,1),
--(128,N'SDG', N'2018-12-13', 394,1),
--(129,N'SDG', N'2018-12-20', 399,1),
--(130,N'SDG', N'2018-12-27', 398,1),
--(131,N'SDG', N'2019-01-03', 403,1),
--(132,N'SDG', N'2019-01-10', 402,1),
--(133,N'SDG', N'2019-01-17', 404,1),
--(134,N'SDG', N'2019-01-24', 403,1),
--(135,N'SDG', N'2019-01-31', 402,1),
--(136,N'SDG', N'2019-02-07', 406,1),
--(137,N'SDG', N'2019-02-14', 408,1),
--(138,N'SDG', N'2019-02-21', 412,1),
--(139,N'SDG', N'2019-02-28', 410,1),
--(140,N'SDG', N'2019-03-07', 409,1),
--(141,N'SDG', N'2019-03-14', 413,1),
--(142,N'SDG', N'2019-03-21', 416,1),
--(143,N'SDG', N'2019-03-28', 415,1),
--(144,N'SDG', N'2019-04-04', 418,1),
--(145,N'SDG', N'2019-04-11', 417,1),
--(146,N'SDG', N'2019-04-18', 421,1),
--(147,N'SDG', N'2019-04-25', 425,1),
--(148,N'SDG', N'2019-05-02', 427,1),
--(149,N'SDG', N'2019-05-09', 433,1),
--(150,N'SDG', N'2019-05-16', 434,1),
--(151,N'SDG', N'2019-05-23', 435,1),
--(152,N'SDG', N'2019-05-30', 434,1),
--(153,N'SDG', N'2019-06-06', 437,1),
--(154,N'SDG', N'2019-06-13', 442,1),
--(155,N'SDG', N'2019-06-20', 442,1),
--(156,N'SDG', N'2019-06-27', 441,1);

EXEC [api].[ExchangeRates__Save]
	@Entities = @ExchangeRates,
	@ValidationErrorsJson = @ValidationErrorsJson
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Exchange Rates Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Activate needed resource definitions
DELETE FROM @LookupDefinitionIds;
INSERT INTO @LookupDefinitionIds([Id]) VALUES
(@ITEquipmentManufacturerLKD),
(@OperatingSystemLKD),
--(@BodyColorLKD),
(@VehicleMakeLKD),
--(@SteelThicknessLKD),
--(@PaperOriginLKD),
--(@PaperGroupLKD),
--(@PaperTypeLKD),
--(@GrainClassificationLKD),
--(@GrainTypeLKD),
--(@QualityLKD),
(@BankAccountTypeLKD),
(@MarketSegmentLKD);

EXEC [dal].[LookupDefinitions__UpdateState]
	@Ids = @LookupDefinitionIds,
	@State = N'Visible'

-- Activate needed resource definitions
DELETE FROM @ResourceDefinitionIds;
INSERT INTO @ResourceDefinitionIds([Id]) VALUES
(@LandMemberRD),
(@BuildingsMemberRD),
(@MachineryMemberRD),
(@MotorVehiclesMemberRD),
(@FixturesAndFittingsMemberRD),
(@OfficeEquipmentMemberRD),
(@ComputerEquipmentMemberRD),
(@CommunicationAndNetworkEquipmentMemberRD),
(@NetworkInfrastructureMemberRD),
--(@BearerPlantsMemberRD),
--(@TangibleExplorationAndEvaluationAssetsMemberRD),
--(@MiningAssetsMemberRD),
--(@OilAndGasAssetsMemberRD),
(@PowerGeneratingAssetsMemberRD),
(@LeaseholdImprovementsMemberRD),
(@ConstructionInProgressMemberRD),
(@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD),
(@OtherPropertyPlantAndEquipmentMemberRD),
(@InvestmentPropertyCompletedMemberRD),
(@InvestmentPropertyUnderConstructionOrDevelopmentMemberRD),
--(@MerchandiseRD),
--(@CurrentFoodAndBeverageRD),
--(@CurrentAgriculturalProduceRD),
--(@FinishedGoodsRD),
(@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
--(@WorkInProgressRD),
(@RawMaterialsRD),
--(@ProductionSuppliesRD),
--(@SparePartsRD),
--(@CurrentFuelRD),
--(@OtherInventoriesRD),
--(@TradeMedicineRD),
--(@TradeConstructionMaterialRD),
--(@TradeSparePartRD),
--(@FinishedGrainRD),
--(@ByproductGrainRD),
--(@FinishedVehicleRD),
--(@FinishedOilRD),
--(@ByproductOilRD),
--(@RawGrainRD),
--(@RawVehicleRD),
(@RevenueServiceRD),
(@EmployeeBenefitRD),
(@CheckReceivedRD);

EXEC [dal].[ResourceDefinitions__UpdateState]
	@Ids = @ResourceDefinitionIds,
	@State = N'Visible'

DELETE FROM LineDefinitionEntryResourceDefinitions
WHERE ResourceDefinitionId IN (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [State] <> N'Visible')

-- Activate needed resource definitions
DELETE FROM @RelationDefinitionIds;
INSERT INTO @RelationDefinitionIds([Id]) VALUES
(@CreditorRLD),
(@DebtorRLD),
(@OwnerRLD),
(@PartnerRLD),
(@SupplierRLD),
(@CustomerRLD),
(@EmployeeRLD),
(@BankAccountCD),
(@SafeCD),
(@WarehouseCD);
--(@TransitCustodyCD);

EXEC [dal].[RelationDefinitions__UpdateState]
	@Ids = @RelationDefinitionIds,
	@State = N'Visible'

DELETE FROM [LineDefinitionEntryCustodyDefinitions]
WHERE [CustodyDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible')

DELETE FROM [LineDefinitionEntryNotedRelationDefinitions]
WHERE [NotedRelationDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible')