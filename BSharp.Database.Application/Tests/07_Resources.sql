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
	DECLARE @R1 [dbo].ResourceList, @R2 [dbo].ResourceList, @R3 [dbo].ResourceList, @R4 [dbo].ResourceList, @R5 [dbo].ResourceList, @R6 [dbo].ResourceList, @R7 [dbo].ResourceList;
	DECLARE @RP1 [dbo].ResourcePickList, @RP2 [dbo].ResourcePickList, @RP3 [dbo].ResourcePickList, @RP4 [dbo].ResourcePickList, @RP5 [dbo].ResourcePickList, @RP6 [dbo].ResourcePickList, @RP7 [dbo].ResourcePickList;
	DECLARE @R1Ids dbo.[IdList], @R2Ids dbo.[IdList], @R3Ids dbo.[idList];
	DECLARE @R1IndexedIds dbo.IndexedIdList, @R2IndexedIds dbo.IndexedIdList, @R3IndexedIds dbo.IndexedIdList;

END
	INSERT INTO dbo.ResourceDefinitions
	([Id],								[Name],									[IfrsResourceClassificationId]) VALUES
	(N'property-plant-and-equipment',	N'Property, plant and equipment',		N'PropertyPlantAndEquipment'),
	(N'motor-vehicles',					N'Motor vehicles',						N'MotorVehicles'),
	(N'computer-equipment',				N'Computer equipment',					N'ComputerEquipment'),
	(N'investment-property',			N'Investment property',					N'InvestmentProperty'),
	(N'intangible-assets',				N'Intangible assets other than goodwill',N'IntangibleAssets'),
	(N'financial-assets',				N'Financial assets',					N'FinancialAssets'),
	(N'received-checks',				N'Checks (received)',					N'FinancialAssets'),
	(N'investments',					N'Investments',							N'Investments'),	
	(N'biological-assets',				N'Biological assets',					N'BiologicalAssets'),
	(N'inventories',					N'Inventories',							N'Inventories'),
	(N'unfinished-goods',				N'Work in progress',					N'WorkInProgress'),
	(N'raw-materials',					N'Raw Materials',						N'RawMaterials'),
	(N'steel-products',					N'Steel products',						N'FinishedGoods'),
	(N'plastic-products',				N'Plastic products',					N'FinishedGoods'),
	(N'vehicles',						N'Vehicles',							N'FinishedGoods'),
	(N'spare-parts',					N'Spare parts',							N'Merchandise'),
	(N'cash-and-cash-equivalents',		N'Cash and cash equivalents',			N'CashAndCashEquivalents'),
	(N'trade-and-other-receivables',	N'Trade and other receivables',			N'TradeAndOtherReceivables'),
	(N'financial-liabilities',			N'Financial liabilities',				N'FinancialLiabilities'),
	(N'issued-checks',					N'Checks (issued)',						N'FinancialLiabilities'),
	(N'issued-letters-of-credit',		N'Letters of credit (issued)',			N'FinancialLiabilities')
	;
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'property-plant-and-equipment'
										[Name],								[IsLeaf],	[Node]) VALUES
	(N'property-plant-and-equipment',	N'Land and buildings',					0,			N'/1/'),
	(N'property-plant-and-equipment',	N'Land',								1,			N'/1/1/'),
	(N'property-plant-and-equipment',	N'Buildings',							1,			N'/1/2/'),
	(N'property-plant-and-equipment',	N'Machinery',							1,			N'/2/'),
	(N'property-plant-and-equipment',	N'Vehicles',							0,			N'/3/'),
	(N'property-plant-and-equipment',	N'Ships',								1,			N'/3/1/'),
	(N'property-plant-and-equipment',	N'Aircraft',							1,			N'/3/2/'),
	(N'property-plant-and-equipment',	N'Motor vehicles',						1,			N'/3/3/'),
	(N'property-plant-and-equipment',	N'Fixture and fittings',				1,			N'/4/'),
	(N'property-plant-and-equipment',	N'Office equipment',					1,			N'/5/'),
--	(N'property-plant-and-equipment',	N'Computer equipment',					1,			N'/6/'),
	(N'property-plant-and-equipment',	N'Communication and network equipment',	1,			N'/7/'),
	(N'property-plant-and-equipment',	N'Nework infrastructure',				1,			N'/8/'),
	(N'property-plant-and-equipment',	N'Bearer plants',						1,			N'/9/'),
	(N'property-plant-and-equipment',	N'Bearer plants',						1,			N'/10/'),
	(N'property-plant-and-equipment',	N'Tangible exploration and evaluation assets',1,	N'/11/'),
	(N'property-plant-and-equipment',	N'Mining assets',						1,			N'/12/'),
	(N'property-plant-and-equipment',	N'Oil and gas assets',					1,			N'/13/'),
	(N'property-plant-and-equipment',	N'Power generating assets',				1,			N'/14/'),
	(N'property-plant-and-equipment',	N'Leashold improvements',				1,			N'/15/'),
	(N'property-plant-and-equipment',	N'Construction in progress',			0,			N'/16/'),
	(N'property-plant-and-equipment',	N'Affordable complexes',				1,			N'/16/1/'),
	(N'property-plant-and-equipment',	N'Luxury Complexes',					1,			N'/16/2/');
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'computer-equipment'
							[Name],				[IsLeaf],	[Node]) VALUES
	(N'computer-equipment',	N'Servers',			1,			N'/1/'),
	(N'computer-equipment',	N'Desktops',		1,			N'/2/'),
	(N'computer-equipment',	N'Laptops',			1,			N'/3/'),
	(N'computer-equipment',	N'Mobiles',			1,			N'/4/'),
	(N'computer-equipment',	N'Printers',		0,			N'/5/'),
	(N'computer-equipment',	N'Color printers',	1,			N'/5/1/'),
	(N'computer-equipment',	N'B/W printers',	1,			N'/5/2/');
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'steel-products'
						[Name],	[IsLeaf],	[Node]) VALUES
	(N'steel-products',	N'D',	1,			N'/1/'),
	(N'steel-products',	N'HSP',	0,			N'/2/'),
	(N'steel-products',	N'CHS',	1,			N'/2/1/'),
	(N'steel-products',	N'RHS',	1,			N'/2/2/'),
	(N'steel-products',	N'SHS',	1,			N'/2/3/'),
	(N'steel-products',	N'LTZ',	0,			N'/3/'),
	(N'steel-products',	N'L',	1,			N'/3/1/'),
	(N'steel-products',	N'T',	1,			N'/3/2/'),
	(N'steel-products',	N'Z',	1,			N'/3/3/'),
	(N'steel-products',	N'SM',	1,			N'/4/'),
	(N'steel-products',	N'CP',	1,			N'/5/'),
	(N'steel-products',	N'X',	1,			N'/6/');
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'vehicles'
					[Name],		[IsLeaf],	[Node]) VALUES
	(N'vehicles',	N'Cars',	1,			N'/1/'),
	(N'vehicles',	N'Sedan',	1,			N'/1/1/'),
	(N'vehicles',	N'4xDrive',	1,			N'/1/2/'),
	(N'vehicles',	N'Sports',	1,			N'/1/3/'),
	(N'vehicles',	N'Trucks',	0,			N'/2/');
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'financial-assets
							[Name],				[IsLeaf],	[Node]) VALUES
	(N'financial-assets',	N'Checks (received)',1,			N'/1/'),
	(N'financial-assets',	N'CPO (received)',	0,			N'/2/'),
	(N'financial-assets',	N'L/C (received)',	1,			N'/3/'),
	(N'financial-assets',	N'L/G (received)',	1,			N'/4/');
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'financial-liabilities
								[Name],				[IsLeaf],	[Node]) VALUES
	(N'financial-liabilities',	N'Checks (issued)',	1,			N'/1/'),
	(N'financial-liabilities',	N'L/G (issued)',	1,			N'/2/'),
	(N'financial-liabilities',	N'L/C (issued)',	1,			N'/3/');
	UPDATE RC_Child -- Fix Parent Id
	SET RC_Child.ParentId = RC_Parent.Id
	FROM dbo.ResourceClassifications RC_Child
	JOIN dbo.ResourceClassifications RC_Parent 
	ON RC_Child.[ParentNode] = RC_Parent.[Node]
	AND RC_Child.[ResourceDefinitionId] = RC_Parent.[ResourceDefinitionId];
BEGIN -- Inserting
	INSERT INTO @R1 ([Index],
		[Name],			[Code],		[CurrencyId]) VALUES
	(0, N'Cash/ETB',	N'ETB',		N'ETB'), -- may not be needed. Implicit in Account
	(1, N'Cash/USD',	N'USD',		N'USD'); -- may not be needed. Implicit in Account
	EXEC [api].[Resources__Save] --  N'cash-and-cash-equivalents',
		@ResourceDefinitionId =  N'cash-and-cash-equivalents',
		@Resources = @R1,
		@Picks = @RP1,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Cash and cash equivalents'
		GOTO Err_Label;
	END;

	INSERT INTO @R2 ([Index],
		[Name],						[Code],		[CurrencyId]) VALUES
	(0,	N'Checks (received)/ETB',	N'RCKETB',	N'ETB'); -- may not be needed. Implicit in Account
	INSERT INTO @RP2([Index], [ResourceIndex],
		[ProductionDate],	[Code],		[MonetaryValue], [IssuingBankId]) VALUES
	(0,0,	N'2017.10.01',	N'101009',	6900,			@CBE),
	(1,0,	N'2017.10.15',	N'2308',	17550,			@AWB);	
	EXEC [api].[Resources__Save] -- N'received-checks'
	@ResourceDefinitionId =  N'received-checks',
	@Resources = @R2,
	@Picks = @RP2,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting received checks'
		GOTO Err_Label;
	END;
	
	INSERT INTO @R3 ([Index],
		[Name],				[Code],			[MassUnitId], [CountUnitId]) VALUES
	(0,	N'HR 1000MMx1.9MM',	N'HR1000x1.9',	@KgUnit, @pcsUnit),
	(1,	N'CR 1000MMx1.4MM',	N'CR1000x1.4',	@KgUnit, @pcsUnit);
	INSERT INTO @RP3([Index], [ResourceIndex],
	[ProductionDate],	[Code],		[Mass]) VALUES
	(4,0,N'2017.10.01',	N'54001',	7891),
	(5,0,N'2017.10.15',	N'54002',	6985),
	(6,0,N'2017.10.15',	N'60032',	7320),
	(7,0,N'2017.10.01',	N'60342',	7100);
	EXEC [api].[Resources__Save] --  N'raw-materials'
		@ResourceDefinitionId = N'raw-materials',
		@Resources = @R3,
		@Picks = @RP3,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting raw materials'
		GOTO Err_Label;
	END;

	DECLARE @RCVS INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceDefinitionId] = N'vehicles' AND [Node] = N'/1/1/');
	INSERT INTO @R6 ([Index],
	[ResourceClassificationId],	[Name],					[CountUnitId]) VALUES
	(0, @RCVS,					N'Toyota Camry 2018',	@pcsUnit),--1
	(1, @RCVS,					N'Fake',				@pcsUnit),--1
	(2, @RCVS,					N'Toyota Yaris 2018',	@pcsUnit);--1
	INSERT INTO @RP6([Index], [ResourceIndex],
			[ProductionDate],	[Code]) VALUES
	(0,0,	N'2017.10.01',		N'101'),
	(1,0,	N'2017.10.15',		N'102'),
	(2,0,	N'2017.10.15',		N'199'),
	(3,2,	N'2017.10.01',		N'201');
	EXEC [api].[Resources__Save] -- N'vehicles'
		@ResourceDefinitionId = N'vehicles',
		@Resources = @R6,
		@Picks = @RP6,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting vehicles'
		GOTO Err_Label;
	END;

	INSERT INTO @R7 ([Index],
		[Name],				[Code],			[CountUnitId]) VALUES
	(0, N'Common Stock',	N'CMNSTCK',		@shareUnit),
	(1, N'Premium Stock',	N'PRMMSTCK',	@shareUnit);
	EXEC [api].[Resources__Save] -- N'financial-liabilities'
		@ResourceDefinitionId = N'financial-liabilities',
		@Resources = @R7,
		@Picks = @RP7,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting financial liabilities'
		GOTO Err_Label;
	END;

	IF @DebugResources = 1
	BEGIN
		SELECT * FROM dbo.[ResourceDefinitions];
		SELECT	[ResourceDefinitionId], [Id], [ParentId], [Node].ToString() As [Path], REPLICATE(N'    ', [Node].GetLevel() - 1) + [Name] AS [Name],
				[Code], [IsActive], [IsLeaf]
		FROM dbo.ResourceClassifications ORDER BY [ResourceDefinitionId], [Node];
		INSERT INTO @R2Ids SELECT [Id] FROM dbo.Resources;
		EXEC rpt.[sp_ResourcesPicks] @R2Ids;
	END

	--(6, N'general-goods',		N'Teddy bear',			NULL,		NULL,			@pcsUnit),



--INSERT INTO @R1 ([Index],
--[IfrsResourceClassificationId],	[Name],		[Code],	[SystemCode], [UnitId]) VALUES
--	(11, N'general-goods',		N'Cotton',	NULL,	NULL,		@KgUnit);

--INSERT INTO @R1 ([Index],
--[IfrsResourceClassificationId],	[Name],		[Code],	[SystemCode], [UnitId]) VALUES
--	(12, N'general-goods',		N'Oil',		NULL,	NULL,		@LiterUnit),
--	(13, N'general-goods',		N'Diesel',	NULL,	NULL,		@LiterUnit);

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
DECLARE @HOvertime int, @ROvertime int, @Basic int, @Transportation int, 
		@LaborHourly int, @LaborDaily int, @Car1Svc int, @GOff int;
DECLARE @HR1000x1_9 INT, @CR1000x1_4 INT;
DECLARE @Oil INT, @Diesel INT;

SELECT 
	@ETB = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'ETB'), 
	@USD = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'USD'),
	@Camry2018 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Toyota Camry 2018'),
	@Car1 = (SELECT [Id] FROM [dbo].[ResourcePicks] WHERE [Code] = N'101'),
	@Car2 = (SELECT [Id] FROM [dbo].[ResourcePicks] WHERE [Code] = N'102'),
	--@Car1Svc = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'101D'),
	@GOff = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'Goff'),
	@Cotton = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Cotton'),
	@TeddyBear = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Teddy bear'),
	@CommonStock = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Common Stock'),
	@HOvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'HolidayOvertime'),
	@ROvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'RestOvertime'),
	@Basic = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Basic'),
	@Transportation = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Transportation'),
	@LaborHourly = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'LaborHourly'),
	@LaborDaily = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'LaborDaily'),
	@HR1000x1_9 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'HR1000x1.9'),
	@CR1000x1_4 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'CR1000x1.4'),
	@Oil = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Oil'),
	@Diesel = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Diesel');