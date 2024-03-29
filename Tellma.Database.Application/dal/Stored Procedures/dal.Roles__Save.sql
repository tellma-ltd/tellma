﻿CREATE PROCEDURE [dal].[Roles__Save]
	@Entities [dbo].[RoleList] READONLY, 
	@Members [dbo].[RoleMembershipList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @ModifiedUserIds [dbo].[IdList];
	DECLARE @PublicRolesInvolved BIT;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- This should include all User Ids whose permissions may have been modified (taking into account public roles
	INSERT INTO @ModifiedUserIds ([Id]) SELECT DISTINCT X.[Id] FROM (
			SELECT [UserId] AS [Id] FROM [dbo].[RoleMemberships] WHERE [RoleId] IN (SELECT [Id] FROM @Entities)
			UNION 
			SELECT [UserId] AS [Id] FROM @Members
		) AS X;

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Roles] AS t
		USING (
			SELECT 
				[Index], [Id], [Name], [Name2], [Name3], [IsPublic], [Code]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Name]			= s.[Name],
				t.[Name2]			= s.[Name2],
				t.[Name3]			= s.[Name3],
				t.[IsPublic]		= s.[IsPublic],
				t.[Code]			= s.[Code],
				t.[SavedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Name],		[Name2],	[Name3],	[IsPublic],		[Code], [SavedById]
			)
			VALUES (
				s.[Name],	s.[Name2],	s.[Name3],	s.[IsPublic], s.[Code], @UserId
			)
			OUTPUT s.[Index], INSERTED.[Id] 
	) As x;

	-- Members
	WITH BE AS (
		SELECT * FROM [dbo].[RoleMemberships]
		WHERE [RoleId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BE AS t
	USING (
		SELECT L.[Index], L.[Id], H.[Id] AS [RoleId], [UserId], [Memo]
		FROM @Members L
		JOIN @IndexedIds H ON L.[HeaderIndex] = H.[Index]
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[UserId]		= s.[UserId], 
			t.[Memo]		= s.[Memo],
			t.[SavedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([RoleId],	[UserId],	[Memo], [SavedById])
		VALUES (s.[RoleId], s.[UserId], s.[Memo], @UserId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Permissions
	WITH BE AS (
		SELECT * FROM [dbo].[Permissions]
		WHERE [RoleId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BE AS t
	USING (
		SELECT L.[Index], L.[Id], H.[Id] AS [RoleId], [View], [Action], [Criteria], [Mask], [Memo]
		FROM @Permissions L
		JOIN @IndexedIds H ON L.[HeaderIndex] = H.[Index]
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[View]		= s.[View], 
			t.[Action]		= s.[Action],
			t.[Criteria]	= s.[Criteria],
			t.[Mask]		= s.[Mask],
			t.[Memo]		= s.[Memo],
			t.[SavedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([RoleId],	[View],	[Action],	[Criteria], [Mask], [Memo], [SavedById])
		VALUES (s.[RoleId], s.[View], s.[Action], s.[Criteria], s.[Mask], s.[Memo], @UserId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	UPDATE [dbo].[Users] SET [PermissionsVersion] = NEWID()
	-- TODO: WHERE [Id] IN (SELECT [Id] FROM @ModifiedUserIds);

	-- Return
	IF (@ReturnIds = 1)
		SELECT * FROM @IndexedIds;
END;