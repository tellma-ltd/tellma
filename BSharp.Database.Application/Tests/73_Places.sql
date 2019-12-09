BEGIN -- Cleanup & Declarations
	DECLARE @PlacesDTO [dbo].[PlaceList];
	DECLARE @System int, @RawMaterialsWarehouse int, @FinishedGoodsWarehouse int, @MiscWarehouse int, 
			@BA_CBEUSD int, @BA_CBEETB int, @TigistSafe int; 
END;

BEGIN -- Insert 
	INSERT INTO @PlacesDTO
	([PlaceType], [Name],					[Address], [BirthDateTime], [CustodianId]) VALUES
	(N'Warehouse',	N'System',					NULL,		NULL,			NULL),
	(N'Warehouse',	N'Raw Materials Warehouse', NULL,		NULL,			NULL),
	(N'Warehouse',	N'Fake Warehouse',		N'Far away',	NULL,			NULL),
	(N'Warehouse',	N'Finished Goods Warehouse', NULL,		NULL,			NULL),
	(N'Warehouse',	N'Misc Warehouse',			NULL,		NULL,			NULL),
	(N'BankAccount',N'CBE - USD',			N'144-1200',	NULL,			@CBE),
	(N'BankAccount',N'CBE - ETB',			N'144-1299',	NULL,			@CBE),
	(N'CashSafe',	N'Tigist - Safe',		N'Cashier Office',NULL,			@TigistNegash);

	EXEC [dbo].[api_Places__Save]
		@Entities = @PlacesDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ResultsJson = @ResultsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Places: Place 1'
		GOTO Err_Label;
	END;

	IF @DebugPlaces = 1
		SELECT * FROM [dbo].[fr_Places__Json](@ResultsJson);
END
BEGIN -- Updating RM Warehouse address
	DELETE FROM @PlacesDTO;
	INSERT INTO @PlacesDTO (
		[Id], [PlaceType], [Name], [Code], [Address], [BirthDateTime], [EntityState], [CustodianId]
	)
	SELECT
		[Id], [PlaceType], [Name], [Code], [Address], [BirthDateTime], N'Unchanged', [CustodianId]
	FROM [dbo].[Agents]
	WHERE [Name] IN (N'Raw Materials Warehouse', N'Fake Warehouse');

	UPDATE @PlacesDTO
	SET 
		[Address] = N'Alemgena, Oromia',
		[EntityState] = N'Updated'
	WHERE [Name] = N'Raw Materials Warehouse';

	UPDATE @PlacesDTO
	SET 
		[EntityState] = N'Deleted'
	WHERE [Name] = N'Fake Warehouse';

	EXEC [dbo].[api_Places__Save]
		@Entities = @PlacesDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ResultsJson = @ResultsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Places: Place 2'
		GOTO Err_Label;
	END;

	IF @DebugPlaces = 1
		SELECT * FROM [dbo].[fr_Places__Json](@ResultsJson);

	DECLARE @Locs dbo.IntegerList;
	INSERT INTO @Locs([Id]) VALUES 
		(29),
		(31);

	EXEC [dbo].[api_Places__Deactivate]
		@Ids = @Locs,
		@ResultsJson = @ResultsJson OUTPUT;

	IF @DebugPlaces = 1
		SELECT * FROM [dbo].[fr_Places__Json](@ResultsJson);
END

IF @DebugPlaces = 1
	SELECT * FROM [dbo].[Agents];

SELECT
	@System = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'System'), 
	@RawMaterialsWarehouse = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Raw Materials Warehouse'), 
	@FinishedGoodsWarehouse = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Finished Goods Warehouse'),
	@MiscWarehouse = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Misc Warehouse'),
	@BA_CBEUSD = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'CBE - USD'),
	@BA_CBEETB = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'CBE - ETB'),
	@TigistSafe = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Tigist - Safe');

	INSERT INTO dbo.AgentsResources([AgentId],	[RelationType],	[ResourceId], CreatedAt, CreatedById, ModifiedAt, ModifiedById) VALUES
	(@BA_CBEETB, N'BankAccount', @ETB, @Now, @UserId, @Now, @UserId),
	(@BA_CBEUSD, N'BankAccount', @USD, @Now, @UserId, @Now, @UserId);
