	DECLARE @Custodies dbo.[AgentList];
	DECLARE @Warehouse_RM int, @Warehouse_FG int;
	INSERT INTO @Custodies
	([Index], [Name],								[StartDate],	[OperatingSegmentId]) VALUES
	(0,		N'RM Warehouse',						'2017.09.15',	@OS_Steel),
	(1,		N'FG Warehouse',						'2018.01.05',	@OS_Steel),
	(4,		N'Admin department',					'2019.05.09',	@OS_Steel);

	EXEC [api].[Agents__Save]
		@DefinitionId = N'custodies',
		@Entities = @Custodies,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'custodies: Inserting'
		GOTO Err_Label;
	END;
	SELECT
		@Warehouse_RM = (SELECT [Id] FROM [dbo].fi_Agents(N'custodies', NULL) WHERE [Name] = N'RM Warehouse'),
		@Warehouse_FG = (SELECT [Id] FROM [dbo].fi_Agents(N'custodies', NULL) WHERE [Name] = N'FG Warehouse');

	IF @DebugCustodies = 1
		SELECT A.[Code], A.[Name], A.[StartDate] AS 'Custody Since', A.[IsActive],
		--AR.[SupplierRating], AR.[PaymentTerms], 
		RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'custodies', NULL) A
		JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;