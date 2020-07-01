IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],	[IdentifierLabel], [IdentifierVisibility]) VALUES
	( N'raw-materials',	N'Raw Materials',	N'Raw Material',	N'Roll #',			N'Optional');

	EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		

	DECLARE @RawMaterialsDescendantsTemp TABLE ([Code] NVARCHAR(255), [Name] NVARCHAR(255), [Node] HIERARCHYID, [IsAssignable] BIT DEFAULT 1, [Index] INT, [ResourceDefinitionId] NVARCHAR (50))
	INSERT INTO @RawMaterialsDescendantsTemp ([Index],
		[Code],					[Name],			[Node],			[IsAssignable]) VALUES
--		(N'RawMaterials',					N'Raw materials',								N'/1/11/1/1/',	1,29),
	(0, N'HotRollExtension',	N'Hot Roll',	N'/1/11/1/1/1/',	1),
	(1, N'ColdRollExtension',	N'Cold Roll',	N'/1/11/1/1/2/',	1);
	
	DECLARE @RawMaterialsDescendants AccountTypeList;
	SET @PId = (SELECT [Id] FROM dbo.[AccountTypes] WHERE [Concept] = N'RawMaterials');
	INSERT INTO @RawMaterialsDescendants ([ParentId],[Code], [Name], [ParentIndex], [IsAssignable], [Index])
	SELECT @PId, [Code], [Name], (SELECT [Index] FROM @RawMaterialsDescendantsTemp WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex, [IsAssignable], [Index]
	FROM @RawMaterialsDescendantsTemp RC

	EXEC [api].[AccountTypes__Save]
		@Entities = @RawMaterialsDescendants,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Raw Materials: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		
	
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],				[Code],			[Identifier], 	[Lookup1Id]) VALUES
	(0, N'HR 1000MMx1.9MM',	N'HR1000x1.9',	N'1001',		dbo.fn_Lookup(N'steel-thicknesses', N'1.9')),
	(1, N'CR 1000MMx1.4MM',	N'CR1000x1.4',	N'1002',		dbo.fn_Lookup(N'steel-thicknesses', N'1.4'));
	-- For RM, we use the descriptor - if any - in Entries

	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'mt'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'mt'),	1);

	EXEC [api].[Resources__Save]
		@DefinitionId = N'raw-materials',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting raw materials: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END