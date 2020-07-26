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
(0,N'SDG', N'2019-12-31', 100,1)

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
--(@VehicleMakeLKD),
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
--(@LandMemberRD),
--(@BuildingsMemberRD),
(@MachineryMemberRD),
--(@MotorVehiclesMemberRD),
--(@FixturesAndFittingsMemberRD),
(@OfficeEquipmentMemberRD),
(@ComputerEquipmentMemberRD),
(@CommunicationAndNetworkEquipmentMemberRD),
(@NetworkInfrastructureMemberRD),
--(@BearerPlantsMemberRD),
--(@TangibleExplorationAndEvaluationAssetsMemberRD),
--(@MiningAssetsMemberRD),
--(@OilAndGasAssetsMemberRD),
(@PowerGeneratingAssetsMemberRD),
--(@LeaseholdImprovementsMemberRD),
--(@ConstructionInProgressMemberRD),
--(@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD),
(@OtherPropertyPlantAndEquipmentMemberRD),
--(@InvestmentPropertyCompletedMemberRD),
--(@InvestmentPropertyUnderConstructionOrDevelopmentMemberRD),
--(@RawGrainRD),
--(@FinishedGrainRD),
--(@ByproductGrainRD),
--(@RawVehicleRD),
--(@FinishedVehicleRD),
--(@FinishedOilRD),
--(@ByproductOilRD),
(@WorkInProgressRD),
--(@TradeMedicineRD),
--(@TradeConstructionMaterialRD),
--(@TradeSparePartRD),
(@RevenueServiceRD),
(@EmployeeBenefitRD);
--(@CheckReceivedRD);

EXEC [dal].[ResourceDefinitions__UpdateState]
	@Ids = @ResourceDefinitionIds,
	@State = N'Visible'

DELETE FROM LineDefinitionEntryResourceDefinitions
WHERE ResourceDefinitionId IN (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [State] <> N'Visible')

-- Activate needed resource definitions
DELETE FROM @RelationDefinitionIds;
INSERT INTO @RelationDefinitionIds([Id]) VALUES
(@CreditorCD),
(@DebtorCD),
--(@OwnerCD),
(@PartnerCD),
(@SupplierCD),
(@CustomerCD),
(@EmployeeCD);
--(@BankCD),
--(@EmployeeCD)
--(@WarehouseCD);
--(@ShipperCD);

EXEC [dal].[RelationDefinitions__UpdateState]
	@Ids = @RelationDefinitionIds,
	@State = N'Visible'

DELETE FROM [LineDefinitionEntryCustodianDefinitions]
WHERE [CustodianDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible')

DELETE FROM [LineDefinitionEntryNotedRelationDefinitions]
WHERE [NotedRelationDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible')