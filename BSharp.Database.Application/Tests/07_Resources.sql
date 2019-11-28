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
	DECLARE @R9 [dbo].ResourceList, @R10 [dbo].ResourceList;
	DECLARE @RP9 [dbo].ResourceInstanceList, @RP10 [dbo].ResourceInstanceList;
	DECLARE @R1Ids dbo.[IdList], @R2Ids dbo.[IdList], @R3Ids dbo.[idList];
	DECLARE @R1IndexedIds dbo.IndexedIdList, @R2IndexedIds dbo.IndexedIdList, @R3IndexedIds dbo.IndexedIdList;

END

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
	:r .\07_Resources_EmployeeBenefits.sql
	
	IF @DebugResources = 1
	BEGIN
	--	SELECT * FROM dbo.[ResourceDefinitions];
		SELECT	RD.[Id] AS [ResourceDefinitionId], RC.[Id], RC.[ParentId], RC.[Node].ToString() As [Path],
				REPLICATE(N'    ', RC.[Node].GetLevel() - 1) + RC.[Name] AS [Name],
				RC.[Code], RC.[IsActive], RC.[IsLeaf]
		FROM dbo.ResourceClassifications RC
		RIGHT JOIN dbo.ResourceDefinitions RD ON RC.[DefinitionId] = RD.Id
		ORDER BY RD.[SortKey], [ResourceDefinitionId], [Node];
		INSERT INTO @R2Ids SELECT [Id] FROM dbo.Resources;
		EXEC rpt.[sp_Resources] @R2Ids;
	END

		--(N'general-resources',				N'General items',				N'General item',	NULL),
	--(N'sdks',							N'SDKs',						N'SDK',				N'FinishedGoods')
	--(N'steel-products',				N'Steel Products',				N'Steel product'), --
	--(N'steel-rolls',					N'Steel Rolls',					N'Steel roll'), --
	--(N'received-checks',				N'Checks (received)',			N'Check received'), --
	--(N'strips',						N'Strips',						N'Strip'),	
	--(N'plastic-products',				N'Plastic products',			N'Plastic product'),
	--(N'issued-checks',				N'Checks (issued)',				N'Check (Issued)'),
	--(N'issued-letters-of-credit',		N'Letters of credit (issued)',	N'LC (Issued)')
	--(6, N'general-goods',		N'Teddy bear',			NULL,		NULL,			@pcsUnit),


--INSERT INTO @R1 ([Index],
--[IfrsResourceClassificationId],	[Name],		[Code],	[SystemCode], [UnitId]) VALUES
--	(11, N'general-goods',		N'Cotton',	NULL,	NULL,		@KgUnit);




--	(20, N'PPEServices',		N'Girgi Office',	N'Goff',	NULL,			@moUnit),
--	(21, N'PPEServices',		N'Car 101 - Svc',	N'101D',	NULL,			@moUnit),
--	(22, N'PPEServices',		N'Car 102 - Svc',	N'102D',	NULL,			@dayUnit);

--	EXEC [api].[Resources__Save]
--		@Resources = @R1,
--		@Instances = @RP1,
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
--		EXEC rpt.[sp_ResourcesInstances] @R1Ids;
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


--	UPDATE @R2
--	SET 
--		[Name] = [Name] + N' - (New)';

--	DELETE FROM @RP2 WHERE [Code] = N'199';

--	EXEC [api].[Resources__Save]
--		@Resources = @R2,
--		@Instances = @RP2,
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
--		EXEC rpt.[sp_ResourcesInstances] @R2Ids;
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
--		EXEC rpt.[sp_ResourcesInstances] @R3Ids;
--	END
--END 

DECLARE @ETB int, @USD int, @CommonStock int;
DECLARE @Camry2018 int, @Cotton int, @TeddyBear int, @Car1 int, @Car2 int;
DECLARE @HOvertime int, @ROvertime int, @Basic int, @Transportation int, 
		@LaborHourly int, @LaborDaily int;--, @Car1Svc int, @GOff int;
DECLARE @HR1000x1_9 INT, @CR1000x1_4 INT;
DECLARE @Oil INT, @Diesel INT;

SELECT 
	@ETB = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'ETB'), 
	@USD = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'USD'),
	@Camry2018 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Toyota Camry 2018'),
	--@Car1 = (SELECT [Id] FROM [dbo].[ResourceInstances] WHERE [Code] = N'101'),
	--@Car2 = (SELECT [Id] FROM [dbo].[ResourceInstances] WHERE [Code] = N'102'),
	--@Car1Svc = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'101D'),
	--@GOff = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'Goff'),
	@Cotton = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Cotton'),
	@TeddyBear = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Teddy bear'),
	@CommonStock = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Common Stock'),
	@HOvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Holiday Overtime'),
	@ROvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Rest Overtime'),
	@Basic = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Basic'),
	@Transportation = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Transportation'),
	@LaborHourly = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Labor (Hourly)'),
	@LaborDaily = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Labor (Daily)'),
	@HR1000x1_9 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'HR1000x1.9'),
	@CR1000x1_4 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'CR1000x1.4'),
	@Oil = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Oil'),
	@Diesel = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Diesel');