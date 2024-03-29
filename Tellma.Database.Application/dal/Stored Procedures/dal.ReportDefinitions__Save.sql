﻿CREATE PROCEDURE [dal].[ReportDefinitions__Save]
	@Entities [dbo].[ReportDefinitionList] READONLY,
	@Parameters [dbo].[ReportDefinitionParameterList] READONLY,
	@Select [dbo].[ReportDefinitionSelectList] READONLY,
	@Rows [dbo].[ReportDefinitionDimensionList] READONLY,
	@RowsAttributes [dbo].[ReportDefinitionDimensionAttributeList] READONLY,
	@Columns [dbo].[ReportDefinitionDimensionList] READONLY,	
	@ColumnsAttributes [dbo].[ReportDefinitionDimensionAttributeList] READONLY,
	@Measures [dbo].[ReportDefinitionMeasureList] READONLY,
	@Roles [dbo].[ReportDefinitionRoleList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList], @RowsIndexedIds [dbo].[IndexIdWithHeaderList], @ColumnsIndexedIds [dbo].[IndexIdWithHeaderList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- Update all users whose report definitions have changed
	IF EXISTS (
		SELECT * FROM dbo.[Roles] R
		JOIN dbo.[ReportDefinitionRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[ReportDefinitions] D ON D.[Id] = DR.[ReportDefinitionId]
		WHERE
			D.[Id] IN (SELECT [Id] FROM @Entities) AND
			R.[IsActive] = 1 AND
			R.[IsPublic] = 1
	)
	BEGIN
		 -- If a public role is mentioned invalidate the cache for all users
		UPDATE dbo.[Users] SET [PermissionsVersion] = NEWID();
	END
	ELSE
	BEGIN
		-- Invalidate the cache for affected users only
		UPDATE U
		SET U.[PermissionsVersion] = NEWID()
		FROM dbo.[Users] U
		JOIN dbo.[RoleMemberships] RM ON U.[Id] = RM.[UserId]
		JOIN dbo.[Roles] R ON RM.[RoleId] = R.[Id]
		JOIN dbo.[ReportDefinitionRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[ReportDefinitions] D ON D.[Id] = DR.[ReportDefinitionId]
		WHERE 
			D.[Id] IN (SELECT [Id] FROM @Entities) AND 
			R.[IsActive] = 1
	END

	-- Report Definitions
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[ReportDefinitions] AS t
		USING (
			SELECT 
				[Index], [Id], [Code], [Title], [Title2], [Title3], [Description], [Description2], [Description3],
				[Type], [Chart], [DefaultsToChart], [ChartOptions], [Collection], [DefinitionId], [Filter], [Having], 
				[OrderBy], [Top], [ShowColumnsTotal], [ColumnsTotalLabel], [ColumnsTotalLabel2], [ColumnsTotalLabel3], 
				[ShowRowsTotal], [RowsTotalLabel], [RowsTotalLabel2], [RowsTotalLabel3], [IsCustomDrilldown],
				IIF(EXISTS (SELECT 1 FROM @Roles R WHERE R.[HeaderIndex] = [Index]), 1, 0) AS [ShowInMainMenu],
				[MainMenuSection], [MainMenuIcon], [MainMenuSortKey]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Code]				= s.[Code],
				t.[Title]				= s.[Title],
				t.[Title2]				= s.[Title2],
				t.[Title3]				= s.[Title3],
				t.[Description]			= s.[Description],
				t.[Description2]		= s.[Description2],
				t.[Description3]		= s.[Description3],
				t.[Type]				= s.[Type],
				t.[Chart]				= s.[Chart],
				t.[DefaultsToChart]		= s.[DefaultsToChart],
				t.[ChartOptions]		= s.[ChartOptions],
				t.[Collection]			= s.[Collection],
				t.[DefinitionId]		= s.[DefinitionId],
				t.[Filter]				= s.[Filter],
				t.[Having]				= s.[Having],
				t.[OrderBy]				= s.[OrderBy],
				t.[Top]					= s.[Top],
				t.[ShowColumnsTotal]	= s.[ShowColumnsTotal],
				t.[ColumnsTotalLabel]	= s.[ColumnsTotalLabel],
				t.[ColumnsTotalLabel2]	= s.[ColumnsTotalLabel2],
				t.[ColumnsTotalLabel3]	= s.[ColumnsTotalLabel3],
				t.[ShowRowsTotal]		= s.[ShowRowsTotal],
				t.[RowsTotalLabel]		= s.[RowsTotalLabel],
				t.[RowsTotalLabel2]		= s.[RowsTotalLabel2],
				t.[RowsTotalLabel3]		= s.[RowsTotalLabel3],
				t.[IsCustomDrilldown]	= s.[IsCustomDrilldown],
				t.[ShowInMainMenu]		= s.[ShowInMainMenu],
				t.[MainMenuSection]		= s.[MainMenuSection],
				t.[MainMenuIcon]		= s.[MainMenuIcon],
				t.[MainMenuSortKey]		= s.[MainMenuSortKey],			
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Code], [Title], [Title2], [Title3], [Description], [Description2], [Description3],
				[Type], [Chart], [DefaultsToChart], [ChartOptions], [Collection], [DefinitionId], [Filter], [Having], [OrderBy], [Top],
				[ShowColumnsTotal], [ColumnsTotalLabel], [ColumnsTotalLabel2], [ColumnsTotalLabel3], 
				[ShowRowsTotal], [RowsTotalLabel], [RowsTotalLabel2], [RowsTotalLabel3], [IsCustomDrilldown],
				[ShowInMainMenu], [MainMenuSection], [MainMenuIcon], [MainMenuSortKey], [CreatedById], [CreatedAt], [ModifiedById], [ModifiedAt]
			)
			VALUES (
				s.[Code], s.[Title], s.[Title2], s.[Title3], s.[Description], s.[Description2], s.[Description3],
				s.[Type], s.[Chart], s.[DefaultsToChart], s.[ChartOptions], s.[Collection], s.[DefinitionId], s.[Filter], s.[Having], s.[OrderBy], s.[Top],
				s.[ShowColumnsTotal], s.[ColumnsTotalLabel], s.[ColumnsTotalLabel2], s.[ColumnsTotalLabel3],
				s.[ShowRowsTotal], s.[RowsTotalLabel], s.[RowsTotalLabel2], s.[RowsTotalLabel3], s.[IsCustomDrilldown],
				s.[ShowInMainMenu], s.[MainMenuSection], s.[MainMenuIcon], s.[MainMenuSortKey], @UserId, @Now, @UserId, @Now
			)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	-- Parameters
	WITH BP AS (
		SELECT * FROM [dbo].[ReportDefinitionParameters]
		WHERE [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[Key], L.[Label], L.[Label2], L.[Label3], L.[Visibility], L.[DefaultExpression], L.[Control], L.[ControlOptions]
		FROM @Parameters L
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Key]					= s.[Key],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[Visibility]			= s.[Visibility],
			t.[DefaultExpression]	= s.[DefaultExpression],
			t.[Control]				= s.[Control],
			t.[ControlOptions]		= s.[ControlOptions]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [ReportDefinitionId], [Key], [Label], [Label2], [Label3], [Visibility], [DefaultExpression], [Control], [ControlOptions]
		)
		VALUES (
			s.[Index], s.[ReportDefinitionId], s.[Key], s.[Label], s.[Label2], s.[Label3], s.[Visibility], s.[DefaultExpression], s.[Control], s.[ControlOptions]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;
	
	-- Select
	WITH BS AS (
		SELECT * FROM [dbo].[ReportDefinitionSelects]
		WHERE [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BS AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[Expression], L.[Localize], L.[Label], L.[Label2], L.[Label3], L.[Control], L.[ControlOptions]
		FROM @Select L JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Expression]			= s.[Expression],
			t.[Localize]			= s.[Localize],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[Control]				= s.[Control],
			t.[ControlOptions]		= s.[ControlOptions]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [ReportDefinitionId], [Expression], [Localize], [Label], [Label2], [Label3], [Control], [ControlOptions]
		)
		VALUES (
			s.[Index], s.[ReportDefinitionId], s.[Expression], s.[Localize], s.[Label], s.[Label2], s.[Label3], s.[Control], s.[ControlOptions]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Rows
	WITH BR AS (
		SELECT * FROM [dbo].[ReportDefinitionDimensions]
		WHERE [Discriminator] = N'Row' AND [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	INSERT INTO @RowsIndexedIds([Index], [HeaderId], [Id])
	SELECT x.[Index], x.[ReportDefinitionId], x.[Id]
	FROM
	(
		MERGE INTO BR AS t
		USING (
			SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[KeyExpression], L.[DisplayExpression], 
			L.[Localize], L.[Label], L.[Label2], L.[Label3], L.[OrderDirection], L.[AutoExpandLevel], L.[ShowAsTree], L.[Control], L.[ControlOptions]
			FROM @Rows L JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
			JOIN @IndexedIds II ON H.[Index] = II.[Index]
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Index]				= s.[Index],
				t.[KeyExpression]		= s.[KeyExpression],
				t.[DisplayExpression]	= s.[DisplayExpression],
				t.[Localize]			= s.[Localize],
				t.[Label]				= s.[Label],
				t.[Label2]				= s.[Label2],
				t.[Label3]				= s.[Label3],
				t.[OrderDirection]		= s.[OrderDirection],
				t.[AutoExpandLevel]		= s.[AutoExpandLevel],
				t.[ShowAsTree]			= s.[ShowAsTree],
				t.[Control]				= s.[Control],
				t.[ControlOptions]		= s.[ControlOptions]
		WHEN NOT MATCHED THEN
			INSERT (
				[Index], [Discriminator], [ReportDefinitionId], [KeyExpression], [DisplayExpression], [Localize], 
				[Label], [Label2], [Label3], [OrderDirection], [AutoExpandLevel], [ShowAsTree], [Control], [ControlOptions]
			)
			VALUES (
				s.[Index], N'Row', s.[ReportDefinitionId], s.[KeyExpression], s.[DisplayExpression], s.[Localize], 
				s.[Label], s.[Label2], s.[Label3], s.[OrderDirection], s.[AutoExpandLevel], s.[ShowAsTree], s.[Control], s.[ControlOptions]
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[ReportDefinitionId], inserted.[Id]
	) AS x
	WHERE [Index] IS NOT NULL;
	
	-- Rows Attributes
	WITH BRA AS (
		SELECT * FROM dbo.[ReportDefinitionDimensionAttributes]
		WHERE [ReportDefinitionDimensionId] IN (SELECT [Id] FROM @RowsIndexedIds)
	)
	MERGE INTO BRA AS t
	USING (
		SELECT
			E.[Id], 
			RDR.Id AS [ReportDefinitionDimensionId], 
			E.[Index], 			
			E.[Expression], 
			E.[Localize], 
			E.[Label], 
			E.[Label2], 
			E.[Label3],
			E.[OrderDirection]
		FROM @RowsAttributes E
		JOIN @IndexedIds RD ON E.[ReportDefinitionIndex] = RD.[Index]
		JOIN @RowsIndexedIds RDR ON E.[HeaderIndex] = RDR.[Index] AND RDR.[HeaderId] = RD.[Id]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN
		UPDATE SET
			t.[Index]					= s.[Index],
			t.[Expression]				= s.[Expression], 
			t.[Localize]				= s.[Localize],	
			t.[Label]					= s.[Label],
			t.[Label2]					= s.[Label2],
			t.[Label3]					= s.[Label3],
			t.[OrderDirection]			= s.[OrderDirection]
	WHEN NOT MATCHED THEN
		INSERT (
			[ReportDefinitionDimensionId],
			[Index], 			
			[Expression], 
			[Localize], 
			[Label], 
			[Label2], 
			[Label3],
			[OrderDirection]
		)
		VALUES (
			s.[ReportDefinitionDimensionId],
			s.[Index], 			
			s.[Expression], 
			s.[Localize], 
			s.[Label], 
			s.[Label2], 
			s.[Label3],
			s.[OrderDirection]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Columns Definitions
	WITH BC AS (
		SELECT * FROM [dbo].[ReportDefinitionDimensions]
		WHERE [Discriminator] = N'Column' AND [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	INSERT INTO @ColumnsIndexedIds([Index], [HeaderId], [Id])
	SELECT x.[Index], x.[ReportDefinitionId], x.[Id]
	FROM
	(
		MERGE INTO BC AS t
		USING (
			SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[KeyExpression], L.[DisplayExpression], 
			L.[Localize], L.[Label], L.[Label2], L.[Label3], L.[OrderDirection], L.[AutoExpandLevel], L.[ShowAsTree], L.[Control], L.[ControlOptions]
			FROM @Columns L JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
			JOIN @IndexedIds II ON H.[Index] = II.[Index]
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Index]				= s.[Index],
				t.[KeyExpression]		= s.[KeyExpression],
				t.[DisplayExpression]	= s.[DisplayExpression],
				t.[Localize]			= s.[Localize],
				t.[Label]				= s.[Label],
				t.[Label2]				= s.[Label2],
				t.[Label3]				= s.[Label3],
				t.[OrderDirection]		= s.[OrderDirection],
				t.[AutoExpandLevel]		= s.[AutoExpandLevel],
				t.[ShowAsTree]			= s.[ShowAsTree],
				t.[Control]				= s.[Control],
				t.[ControlOptions]		= s.[ControlOptions]
		WHEN NOT MATCHED THEN
			INSERT (
				[Index], [Discriminator], [ReportDefinitionId], [KeyExpression], [DisplayExpression], [Localize], 
				[Label], [Label2], [Label3], [OrderDirection], [AutoExpandLevel], [ShowAsTree], [Control], [ControlOptions]
			)
			VALUES (
				s.[Index], N'Column', s.[ReportDefinitionId], s.[KeyExpression], s.[DisplayExpression], s.[Localize], 
				s.[Label], s.[Label2], s.[Label3], s.[OrderDirection], s.[AutoExpandLevel], s.[ShowAsTree], s.[Control], s.[ControlOptions]
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id], inserted.[ReportDefinitionId]
	) AS x
	WHERE [Index] IS NOT NULL;
			
	-- Columns Attributes
	WITH BRA AS (
		SELECT * FROM dbo.[ReportDefinitionDimensionAttributes]
		WHERE [ReportDefinitionDimensionId] IN (SELECT [Id] FROM @ColumnsIndexedIds)
	)
	MERGE INTO BRA AS t
	USING (
		SELECT
			E.[Id], 
			RDC.Id AS [ReportDefinitionDimensionId], 
			E.[Index], 			
			E.[Expression], 
			E.[Localize], 
			E.[Label], 
			E.[Label2], 
			E.[Label3],
			E.[OrderDirection]
		FROM @ColumnsAttributes E
		JOIN @IndexedIds RD ON E.[ReportDefinitionIndex] = RD.[Index]
		JOIN @ColumnsIndexedIds RDC ON E.[HeaderIndex] = RDC.[Index] AND RDC.[HeaderId] = RD.[Id]
	) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED THEN
		UPDATE SET
			t.[Index]					= s.[Index],
			t.[Expression]				= s.[Expression], 
			t.[Localize]				= s.[Localize],	
			t.[Label]					= s.[Label],
			t.[Label2]					= s.[Label2],
			t.[Label3]					= s.[Label3],
			t.[OrderDirection]			= s.[OrderDirection]
	WHEN NOT MATCHED THEN
		INSERT (
			[ReportDefinitionDimensionId],
			[Index], 			
			[Expression], 
			[Localize], 
			[Label], 
			[Label2], 
			[Label3],
			[OrderDirection]
		)
		VALUES (
			s.[ReportDefinitionDimensionId],
			s.[Index], 			
			s.[Expression], 
			s.[Localize], 
			s.[Label], 
			s.[Label2], 
			s.[Label3],
			s.[OrderDirection]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;
				
	-- Measure Definitions
	WITH BM AS (
		SELECT * FROM [dbo].[ReportDefinitionMeasures]
		WHERE [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BM AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[Expression], L.[Label], L.[Label2], L.[Label3], L.[OrderDirection], 
		L.[Control], L.[ControlOptions], L.[DangerWhen], L.[WarningWhen], L.[SuccessWhen]
		FROM @Measures L JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Expression]			= s.[Expression],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[OrderDirection]		= s.[OrderDirection],
			t.[Control]				= s.[Control],
			t.[ControlOptions]		= s.[ControlOptions],
			t.[DangerWhen]			= s.[DangerWhen],
			t.[WarningWhen]			= s.[WarningWhen],
			t.[SuccessWhen]			= s.[SuccessWhen]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [ReportDefinitionId], [Expression], [Label], [Label2], [Label3], [OrderDirection], 
			[Control], [ControlOptions], [DangerWhen], [WarningWhen], [SuccessWhen]
		)
		VALUES (
			s.[Index], s.[ReportDefinitionId], s.[Expression], s.[Label], s.[Label2], s.[Label3], s.[OrderDirection], 
			s.[Control], s.[ControlOptions], s.[DangerWhen], s.[WarningWhen], s.[SuccessWhen]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;
				
	-- Roles
	WITH BM AS (
		SELECT * FROM [dbo].[ReportDefinitionRoles]
		WHERE [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BM AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[RoleId]
		FROM @Roles L 
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[RoleId]				= s.[RoleId]
	WHEN NOT MATCHED THEN
		INSERT (
			[ReportDefinitionId], [RoleId]
		)
		VALUES (
			s.[ReportDefinitionId], s.[RoleId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();
	
	-- Update all users whose report definitions have changed
	IF EXISTS (
		SELECT * FROM dbo.[Roles] R
		JOIN dbo.[ReportDefinitionRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[ReportDefinitions] D ON D.[Id] = DR.[ReportDefinitionId]
		WHERE
			D.[Id] IN (SELECT [Id] FROM @IndexedIds) AND 
			D.[ShowInMainMenu] = 1 AND
			R.[IsActive] = 1 AND
			R.[IsPublic] = 1
	)
	BEGIN
		 -- If a public role is mentioned invalidate the cache for all users
		UPDATE dbo.[Users] SET [PermissionsVersion] = NEWID();
	END
	ELSE
	BEGIN
		-- Invalidate the cache for affected users only
		UPDATE U
		SET U.[PermissionsVersion] = NEWID()
		FROM dbo.[Users] U
		JOIN dbo.[RoleMemberships] RM ON U.[Id] = RM.[UserId]
		JOIN dbo.[Roles] R ON RM.[RoleId] = R.[Id]
		JOIN dbo.[ReportDefinitionRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[ReportDefinitions] D ON D.[Id] = DR.[ReportDefinitionId]
		WHERE 
			D.[Id] IN (SELECT [Id] FROM @IndexedIds) AND 
			D.[ShowInMainMenu] = 1 AND
			R.[IsActive] = 1
	END

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;