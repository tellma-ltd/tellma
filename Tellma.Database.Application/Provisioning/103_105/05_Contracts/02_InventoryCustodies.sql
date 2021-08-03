DECLARE @Warehouses dbo.[CustodyList];

IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @Warehouses
	([Index], [Name]) VALUES
	(0,		N'RM Warehouse'),
	(1,		N'FG Warehouse');
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	INSERT INTO @Warehouses
	([Index], [Name]) VALUES
	(0,		N'RM Warehouse'),
	(1,		N'FG Warehouse'),
	(2,		N'Cashier');
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Warehouses
	([Index], [Name]) VALUES
	(0,		N'RM Warehouse'),
	(1,		N'FG Warehouse'),
	(2,		N'Cashier');
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	INSERT INTO @Warehouses
	([Index], [Name],				[Name2]) VALUES
	(0,		N'Jeddah Sales',	N'جده - مبيعات'),
	(1,		N'Riyadh Sales',	N'الرياض - مبيعات'),
	(2,		N'Dammam Sales',	N'الدمام - مبيعات');

	EXEC [api].[Relations__Save]
		@DefinitionId = @WarehouseCD,
		@Entities = @Warehouses,
		@UserId = @AdminUserId;

	DECLARE	@4WH_RM INT = (SELECT [Id] FROM [dbo].[fi_Custodies](@WarehouseCD, NULL) WHERE [Name] = N'RM Warehouse');
	DECLARE	@4WH_FG INT = (SELECT [Id] FROM [dbo].[fi_Custodies](@WarehouseCD, NULL) WHERE [Name] = N'FG Warehouse');
	DECLARE	@5WH_JED INT = (SELECT [Id] FROM [dbo].[fi_Custodies](@WarehouseCD, NULL) WHERE [Name] = N'Jeddah Sales');
	DECLARE	@5WH_RUH INT = (SELECT [Id] FROM [dbo].[fi_Custodies](@WarehouseCD, NULL) WHERE [Name] = N'Riyadh Sales');
	DECLARE	@6WH_DAM INT = (SELECT [Id] FROM [dbo].[fi_Custodies](@WarehouseCD, NULL) WHERE [Name] = N'Dammam Sales');
