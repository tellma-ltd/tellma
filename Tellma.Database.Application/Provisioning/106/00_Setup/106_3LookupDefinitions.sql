-- Activate needed Lookup definitions
DELETE FROM @LookupDefinitionIds;
INSERT INTO @LookupDefinitionIds([Id]) VALUES
(@ITEquipmentManufacturerLKD),
(@OperatingSystemLKD),
(@BodyColorLKD),
(@VehicleMakeLKD),
--(@SteelThicknessLKD),
--(@PaperOriginLKD),
--(@PaperGroupLKD),
--(@PaperTypeLKD),
(@GrainClassificationLKD),
(@GrainTypeLKD),
(@QualityLKD),
(@BankAccountTypeLKD),
(@MarketSegmentLKD),
(@BankLKD);

EXEC [dal].[LookupDefinitions__UpdateState]
	@Ids = @LookupDefinitionIds,
	@State = N'Visible';