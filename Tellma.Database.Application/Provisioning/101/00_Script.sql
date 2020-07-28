-- De-Activate Unneeded Entry Types


-- Activate Currencies in Use
INSERT INTO @IndexedCurrencyIds
([Index],	[Id]) VALUES
(0,			@SDG),
(1,			@USD),
(2,			@SAR),
(3,			@AED);

EXEC [api].[Currencies__Activate]
	@IndexedIds = @IndexedCurrencyIds,
	@IsActive = 1,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies Activating: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DELETE FROM @Currencies
INSERT INTO @Currencies([Index],			[NumericCode], [Id], [Name], [Description],[E]) 
 
SELECT  ROW_NUMBER() OVER(ORDER BY [Id]),	[NumericCode], [Id], [Name], [Description],[E]
FROM dbo.[Currencies]
WHERE [Id] IN (SELECT[Id] FROM @IndexedCurrencyIds);

UPDATE @Currencies SET [Name] = N'Pound', [Name2] = N'جنيه' WHERE [Id] = @SDG
UPDATE @Currencies SET [Name] = N'Dollar', [Name2] = N'دولار' WHERE [Id] = @USD
UPDATE @Currencies SET [Name] = N'Riyal', [Name2] = N'ريال' WHERE [Id] = @SAR
UPDATE @Currencies SET [Name] = N'Dirham', [Name2] = N'درهم' WHERE [Id] = @AED

EXEC [api].[Currencies__Save]
	@Entities = @Currencies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies Updating: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Add exchange Rates
INSERT INTO @ExchangeRates([Index],[CurrencyId], [ValidAsOf], [AmountInCurrency], [AmountInFunctional]) VAlUES
(0,N'USD', N'2019-12-31', 1,45.1463),
(1,N'SAR', N'2019-12-31', 1,12.039),
(2,N'AED', N'2019-12-31', 1,12.2931);

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
--(@BankAccountTypeLKD),
(@MarketSegmentLKD),
(@BankLKD);

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
--(@MerchandiseRD),
--(@CurrentFoodAndBeverageRD),
--(@CurrentAgriculturalProduceRD),
--(@FinishedGoodsRD),
--(@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(@WorkInProgressRD),
--(@RawMaterialsRD),
--(@ProductionSuppliesRD),
--(@CurrentPackagingAndStorageMaterialsRD),
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
--(@CheckReceivedRD),
(@AccrualsRD),
(@AccruedIncomeRD),
(@EmployeeLoanRD),
(@CurrentBilledButNotIssuedRD),
(@DeferredIncomeRD),
(@PrepaymentsRD),
(@SalaryAdvanceRD),
(@ReceivablesFromRentalOfPropertiesRD),
--(@ReceivablesFromSaleOfPropertiesRD),
--(@RefundsProvisionRD),
(@RentDeferredIncomeRD),
--(@RestructuringProvisionRD),
--(@RetentionPayableRD),
(@TradePayableRD),
(@TradeReceivableRD);
--(@WarrantyProvisionRD);

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
(@EmployeeCD),
(@BankAccountCD),
(@SafeCD);
--(@WarehouseCD);
--(@ShipperCD);

EXEC [dal].[RelationDefinitions__UpdateState]
	@Ids = @RelationDefinitionIds,
	@State = N'Visible'

DELETE FROM [LineDefinitionEntryCustodianDefinitions]
WHERE [CustodianDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible')

DELETE FROM [LineDefinitionEntryNotedRelationDefinitions]
WHERE [NotedRelationDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible')
