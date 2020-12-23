-- De-Activate Unneeded Entry Types


-- Activate Currencies in Use
INSERT INTO @IndexedCurrencyIds
([Index],	[Id]) VALUES
(0,			@ETB),
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
(0,N'USD', N'2020-07-07', 1,35.140030),
(1,N'USD', N'2020-08-05', 1,35.349200),
(2,N'USD', N'2020-08-07', 1,35.376800),
(3,N'USD', N'2020-08-11', 1,35.494400);


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
(@BearerPlantsMemberRD),
(@TangibleExplorationAndEvaluationAssetsMemberRD),
(@MiningAssetsMemberRD),
(@OilAndGasAssetsMemberRD),
(@PowerGeneratingAssetsMemberRD),
(@LeaseholdImprovementsMemberRD),
(@ConstructionInProgressMemberRD),
(@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD),
(@OtherPropertyPlantAndEquipmentMemberRD),
(@InvestmentPropertyCompletedMemberRD),
(@InvestmentPropertyUnderConstructionOrDevelopmentMemberRD),
(@MerchandiseRD),
(@CurrentFoodAndBeverageRD),
(@CurrentAgriculturalProduceRD),
(@FinishedGoodsRD),
(@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
--(@WorkInProgressRD),
(@RawMaterialsRD),
(@ProductionSuppliesRD),
(@SparePartsRD),
(@CurrentFuelRD),
(@OtherInventoriesRD),
(@TradeMedicineRD),
(@TradeConstructionMaterialRD),
(@TradeSparePartRD),
(@FinishedGrainRD),
(@ByproductGrainRD),
(@FinishedVehicleRD),
(@FinishedOilRD),
(@ByproductOilRD),
(@RawGrainRD),
(@RawVehicleRD),
(@CustomerPointServiceRD),
(@CustomerPeriodServiceRD),
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
(@BankBranchRLD);

EXEC [dal].[RelationDefinitions__UpdateState]
	@Ids = @RelationDefinitionIds,
	@State = N'Visible'

DELETE FROM [LineDefinitionEntryCustodyDefinitions]
WHERE [CustodyDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible');