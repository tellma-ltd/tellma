CREATE PROCEDURE [dal].[DashboardDefinitions__Save]
	@Entities [dbo].[DashboardDefinitionList] READONLY,
	@Widgets [dbo].[DashboardDefinitionWidgetList] READONLY,
	@Roles [dbo].[DashboardDefinitionRoleList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- Update all users whose dashboard definitions have changed
	IF EXISTS (
		SELECT * FROM dbo.[Roles] R
		JOIN dbo.[DashboardDefinitionRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[DashboardDefinitions] D ON D.[Id] = DR.[DashboardDefinitionId]
		WHERE
			D.[Id] IN (SELECT [Id] FROM @Entities) AND 
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
		JOIN dbo.[DashboardDefinitionRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[DashboardDefinitions] D ON D.[Id] = DR.[DashboardDefinitionId]
		WHERE 
			D.[Id] IN (SELECT [Id] FROM @Entities) AND 
			D.[ShowInMainMenu] = 1 AND
			R.[IsActive] = 1
	END


	-- Dashboard Definitions
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[DashboardDefinitions] AS t
		USING (
			SELECT 
				[Index], [Id], [Code], [Title], [Title2], [Title3], [AutoRefreshPeriodInMinutes],
				[ShowInMainMenu], [MainMenuSection], [MainMenuIcon], [MainMenuSortKey]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Code]				= s.[Code],
				t.[Title]				= s.[Title],
				t.[Title2]				= s.[Title2],
				t.[Title3]				= s.[Title3],
				t.[AutoRefreshPeriodInMinutes] = s.[AutoRefreshPeriodInMinutes],
				t.[ShowInMainMenu]		= s.[ShowInMainMenu],
				t.[MainMenuSection]		= s.[MainMenuSection],
				t.[MainMenuIcon]		= s.[MainMenuIcon],
				t.[MainMenuSortKey]		= s.[MainMenuSortKey],			
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Code], [Title], [Title2], [Title3],[AutoRefreshPeriodInMinutes],
				[ShowInMainMenu], [MainMenuSection], [MainMenuIcon], [MainMenuSortKey], 
				[CreatedById], [CreatedAt], [ModifiedById], [ModifiedAt]
			)
			VALUES (
				s.[Code], s.[Title], s.[Title2], s.[Title3], s.[AutoRefreshPeriodInMinutes],
				s.[ShowInMainMenu], s.[MainMenuSection], s.[MainMenuIcon], s.[MainMenuSortKey],
				@UserId, @Now, @UserId, @Now
			)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	-- Widgets
	WITH WD AS (
		SELECT * FROM [dbo].[DashboardDefinitionWidgets]
		WHERE [DashboardDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO WD AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [DashboardDefinitionId], L.[ReportDefinitionId], 
				L.[OffsetX], L.[OffsetY], L.[Width], L.[Height], L.[Title], L.[Title2], L.[Title3], L.[AutoRefreshPeriodInMinutes]
		FROM @Widgets L
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[ReportDefinitionId]	= s.[ReportDefinitionId],
			t.[OffsetX]				= s.[OffsetX],
			t.[OffsetY]				= s.[OffsetY],
			t.[Width]				= s.[Width],
			t.[Height]				= s.[Height],
			t.[Title]				= s.[Title],
			t.[Title2]				= s.[Title2],
			t.[Title3]				= s.[Title3],
			t.[AutoRefreshPeriodInMinutes]	= s.[AutoRefreshPeriodInMinutes]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [DashboardDefinitionId], [ReportDefinitionId], [OffsetX], [OffsetY], [Width], [Height], [Title], [Title2], [Title3], [AutoRefreshPeriodInMinutes]
		)
		VALUES (
			s.[Index], s.[DashboardDefinitionId], s.[ReportDefinitionId], s.[OffsetX], s.[OffsetY], s.[Width], s.[Height], s.[Title], s.[Title2], s.[Title3], s.[AutoRefreshPeriodInMinutes]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;
				
	-- Roles
	WITH BM AS (
		SELECT * FROM [dbo].[DashboardDefinitionRoles]
		WHERE [DashboardDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BM AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [DashboardDefinitionId], L.[RoleId]
		FROM @Roles L 
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[RoleId]				= s.[RoleId]
	WHEN NOT MATCHED THEN
		INSERT (
			[DashboardDefinitionId], [RoleId]
		)
		VALUES (
			s.[DashboardDefinitionId], s.[RoleId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	-- Update all users whose dashboard definitions have changed
	IF EXISTS (
		SELECT * FROM dbo.[Roles] R
		JOIN dbo.[DashboardDefinitionRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[DashboardDefinitions] D ON D.[Id] = DR.[DashboardDefinitionId]
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
		JOIN dbo.[DashboardDefinitionRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[DashboardDefinitions] D ON D.[Id] = DR.[DashboardDefinitionId]
		WHERE 
			D.[Id] IN (SELECT [Id] FROM @IndexedIds) AND 
			D.[ShowInMainMenu] = 1 AND
			R.[IsActive] = 1
	END

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;