
	INSERT INTO dbo.ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],	[ResourceTypeParentList]) VALUES
	( N'raw-materials',	N'Raw Materials',	N'Raw Material',	N'RawMaterials');
	
	DECLARE @RawMaterials dbo.ResourceList;
	INSERT INTO @RawMaterials ([Index],
	[ResourceTypeId],	[Name],				[Code],			[MassUnitId],				[CountUnitId]) VALUES
	(0, N'RawMaterials',N'HR 1000MMx1.9MM',	N'HR1000x1.9',	dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(1, N'RawMaterials',N'CR 1000MMx1.4MM',	N'CR1000x1.4',	dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs'));
	-- For RM, we use the descriptor - if any - in Entries

	EXEC [api].[Resources__Save]
		@DefinitionId = N'raw-materials',
		@Entities = @RawMaterials,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting raw materials'
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT N'raw-materials' AS [Resource Definition]
		DECLARE @RawMaterialsIds dbo.IdList;
		INSERT INTO @RawMaterialsIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'raw-materials';

		SELECT ResourceTypeId, [Name] AS 'Raw Material', [MassUnit] AS 'Weight In', [CountUnit] AS 'Count In'
		FROM rpt.Resources(@RawMaterialsIds);
	END