/* Use Cases
Completed
	- Inserting
	- Updating
	- Deleting
Missing	
	- Activating
	- Deactivating
*/
BEGIN -- Cleanup & Declarations
	DECLARE @R5 [dbo].ResourceList, @R8 [dbo].ResourceList, @R9 [dbo].ResourceList, @R10 [dbo].ResourceList;
	DECLARE  @RP5 [dbo].ResourcePickList,  @RP8 [dbo].ResourcePickList, @RP9 [dbo].ResourcePickList, @RP10 [dbo].ResourcePickList;
	DECLARE @R1Ids dbo.[IdList], @R2Ids dbo.[IdList], @R3Ids dbo.[idList];
	DECLARE @R1IndexedIds dbo.IndexedIdList, @R2IndexedIds dbo.IndexedIdList, @R3IndexedIds dbo.IndexedIdList;

END
	INSERT INTO dbo.ResourceDefinitions ([SortKey],
		[Id],								[Name],									[IfrsResourceClassificationId]) VALUES
	(1,N'property-plant-and-equipment',		N'Property, plant and equipment',		N'PropertyPlantAndEquipment'),
	(2,N'motor-vehicles',					N'Motor vehicles',						N'MotorVehicles'), --
	(3,N'computer-equipment',				N'Computer equipment',					N'ComputerEquipment'), --
	(4,N'investment-property',				N'Investment property',					N'InvestmentProperty'),
	(5,N'intangible-assets',				N'Intangible assets other than goodwill',N'IntangibleAssets'),
	(6,N'financial-assets',					N'Financial assets',					N'FinancialAssets'),
	(7,N'received-checks',					N'Checks (received)',					N'FinancialAssets'), --
	(8,N'investments',						N'Investments',							N'Investments'),	
	(9,N'biological-assets',				N'Biological assets',					N'BiologicalAssets'),
	(10,N'inventories',						N'Inventories',							N'Inventories'),
	(11,N'raw-materials',					N'Raw Materials',						N'RawMaterials'),
	(12,N'production-supplies',				N'Production Supplies',					N'ProductionSupplies'),
	(13,N'unfinished-goods',				N'Work in progress',					N'WorkInProgress'),
	(14,N'steel-products',					N'Steel products',						N'FinishedGoods'),
	(15,N'plastic-products',				N'Plastic products',					N'FinishedGoods'),
	(16,N'vehicles',						N'Vehicles',							N'FinishedGoods'),
	(17,N'spare-parts',						N'Spare parts',							N'SpareParts'),
	(18,N'cash-and-cash-equivalents',		N'Cash and cash equivalents',			N'CashAndCashEquivalents'),
	(19,N'financial-liabilities',			N'Financial liabilities',				N'FinancialLiabilities'),
	(20,N'issued-checks',					N'Checks (issued)',						N'FinancialLiabilities'),
	(21,N'issued-letters-of-credit',		N'Letters of credit (issued)',			N'FinancialLiabilities')
	;

	UPDATE RC_Child -- Fix Parent Id
	SET RC_Child.ParentId = RC_Parent.Id
	FROM dbo.ResourceClassifications RC_Child
	JOIN dbo.ResourceClassifications RC_Parent 
	ON RC_Child.[ParentNode] = RC_Parent.[Node]
	AND RC_Child.[ResourceDefinitionId] = RC_Parent.[ResourceDefinitionId];
BEGIN -- Inserting

	:r .\07_Resources_PropertyPlantAndEquipment.sql
	:r .\07_Resources_ComputerEquipment.sql
	:r .\07_Resources_FinancialAssets.sql
	:r .\07_Resources_ReceivedChecks.sql
	:r .\07_Resources_RawMaterials.sql
	:r .\07_Resources_ProductionSupplies.sql
	:r .\07_Resources_SteelProducts.sql
	:r .\07_Resources_Vehicles.sql
	:r .\07_Resources_Cash.sql
	:r .\07_Resources_FinancialLiabilities.sql
	
	IF @DebugResources = 1
	BEGIN
	--	SELECT * FROM dbo.[ResourceDefinitions];
		SELECT	RD.[Id] AS [ResourceDefinitionId], RC.[Id], RC.[ParentId], RC.[Node].ToString() As [Path],
				REPLICATE(N'    ', RC.[Node].GetLevel() - 1) + RC.[Name] AS [Name],
				RC.[Code], RC.[IsActive], RC.[IsLeaf]
		FROM dbo.ResourceClassifications RC
		RIGHT JOIN dbo.ResourceDefinitions RD ON RC.ResourceDefinitionId = RD.Id ORDER BY RD.[SortKey], [ResourceDefinitionId], [Node];
		INSERT INTO @R2Ids SELECT [Id] FROM dbo.Resources;
	--	EXEC rpt.[sp_ResourcesPicks] @R2Ids;
	END

	--(6, N'general-goods',		N'Teddy bear',			NULL,		NULL,			@pcsUnit),


--INSERT INTO @R1 ([Index],
--[IfrsResourceClassificationId],	[Name],		[Code],	[SystemCode], [UnitId]) VALUES
--	(11, N'general-goods',		N'Cotton',	NULL,	NULL,		@KgUnit);



--INSERT INTO @R1 ([Index],
--[IfrsResourceClassificationId],	[Name],					[Code],		[SystemCode],	[UnitId]) VALUES
--	(14, N'wages-and-salaries',	N'Basic',			NULL,		N'Basic',		@moUnit),
--	(15, N'wages-and-salaries',	N'Transportation',	NULL,		N'Transportation',@moUnit),
--	(16, N'wages-and-salaries',	N'Holiday Overtime',NULL,		N'HolidayOvertime',@hrUnit),
--	(17, N'wages-and-salaries',	N'Rest Overtime',	NULL,		N'RestOvertime',@hrUnit),
--	(18, N'wages-and-salaries',	N'Labor (hourly)',	NULL,		N'LaborHourly',	@hrUnit),
--	(19, N'wages-and-salaries',	N'Labor (daily)',	NULL,		N'LaborDaily',	@dayUnit),
--	(20, N'PPEServices',		N'Girgi Office',	N'Goff',	NULL,			@moUnit),
--	(21, N'PPEServices',		N'Car 101 - Svc',	N'101D',	NULL,			@moUnit),
--	(22, N'PPEServices',		N'Car 102 - Svc',	N'102D',	NULL,			@dayUnit);

--	EXEC [api].[Resources__Save]
--		@Resources = @R1,
--		@Picks = @RP1,
--		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--	IF @ValidationErrorsJson IS NOT NULL 
--	BEGIN
--		Print 'Inserting Resources'
--		GOTO Err_Label;
--	END;
--	IF @DebugResources = 1
--	BEGIN
--		--SELECT * FROM dbo.Resources;
--		INSERT INTO @R1Ids SELECT [Id] FROM dbo.Resources;
--		EXEC rpt.[sp_ResourcesPicks] @R1Ids;
--	END
END

--BEGIN -- Updating
--	INSERT INTO @R2 ([Index],
--		[Id], [UnitId], [IfrsResourceClassificationId], [Name], [Code], [SystemCode],
--		[CurrencyId], [MassUnitId], [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
--	)
--	SELECT ROW_NUMBER() OVER (ORDER BY [Id]),
--		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode],
--		[CurrencyId], [MassUnitId], [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
--	FROM [dbo].Resources
--	WHERE [Name] IN (N'Toyota Camry 2018')
--	INSERT INTO @RP2 ( [ResourceIndex], [Id], [ResourceId],	[Code], [ProductionDate])
--	SELECT				R2.[Index], RI.[Id], RI.[ResourceId],  RI.[Code], RI.[ProductionDate]
--	FROM [dbo].[ResourcePicks] RI
--	JOIN @R2 R2 ON RI.ResourceId = R2.[Id]
--	WHERE ResourceId IN (SELECT [Id] FROM @R2);

--	UPDATE @R2
--	SET 
--		[Name] = [Name] + N' - (New)';

--	DELETE FROM @RP2 WHERE [Code] = N'199';

--	EXEC [api].[Resources__Save]
--		@Resources = @R2,
--		@Picks = @RP2,
--		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--	IF @ValidationErrorsJson IS NOT NULL 
--	BEGIN
--		Print 'Updating Resources'
--		GOTO Err_Label;
--	END
--	IF @DebugResources = 1
--	BEGIN
--		--SELECT * FROM @R2;
--		--SELECT * FROM @RP2;
--		INSERT INTO @R2Ids SELECT [Id] FROM dbo.Resources;
--		EXEC rpt.[sp_ResourcesPicks] @R2Ids;
--	END
--END

--BEGIN -- Deleting
--	INSERT INTO @R3 ([Index],
--		[Id], [UnitId], [IfrsResourceClassificationId], [Name], [Code], [SystemCode],
--		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
--	)
--	SELECT ROW_NUMBER() OVER (ORDER BY [Id]),
--		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode],
--		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
--	FROM [dbo].Resources
--	WHERE [Name] LIKE N'Fake%';

--	INSERT INTO @R3IndexedIds SELECT [Index], [Id] FROM @R3

--	EXEC [api].[Resources__Delete]
--		@IndexedIds = @R3IndexedIds,
--		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--	IF @ValidationErrorsJson IS NOT NULL 
--	BEGIN
--		Print 'Deleting Resources'
--		GOTO Err_Label;
--	END
--	IF @DebugResources = 1
--	BEGIN
--		INSERT INTO @R3Ids SELECT [Id] FROM dbo.Resources;
--		EXEC rpt.[sp_ResourcesPicks] @R3Ids;
--	END
--END 

DECLARE @ETB int, @USD int, @CommonStock int;
DECLARE @Camry2018 int, @Cotton int, @TeddyBear int, @Car1 int, @Car2 int;
--DECLARE @HOvertime int, @ROvertime int, @Basic int, @Transportation int, 
--		@LaborHourly int, @LaborDaily int, @Car1Svc int, @GOff int;
DECLARE @HR1000x1_9 INT, @CR1000x1_4 INT;
DECLARE @Oil INT, @Diesel INT;

SELECT 
	@ETB = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'ETB'), 
	@USD = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'USD'),
	@Camry2018 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Toyota Camry 2018'),
	@Car1 = (SELECT [Id] FROM [dbo].[ResourcePicks] WHERE [Code] = N'101'),
	@Car2 = (SELECT [Id] FROM [dbo].[ResourcePicks] WHERE [Code] = N'102'),
	--@Car1Svc = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'101D'),
	--@GOff = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'Goff'),
	@Cotton = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Cotton'),
	@TeddyBear = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Teddy bear'),
	@CommonStock = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Common Stock'),
	--@HOvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'HolidayOvertime'),
	--@ROvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'RestOvertime'),
	--@Basic = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Basic'),
	--@Transportation = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Transportation'),
	--@LaborHourly = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'LaborHourly'),
	--@LaborDaily = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'LaborDaily'),
	@HR1000x1_9 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'HR1000x1.9'),
	@CR1000x1_4 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'CR1000x1.4'),
	@Oil = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Oil'),
	@Diesel = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Diesel');