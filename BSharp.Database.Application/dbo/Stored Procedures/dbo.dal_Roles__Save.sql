CREATE PROCEDURE [dbo].[dal_Roles__Save]
	@Roles [dbo].[RoleList] READONLY, 
	@Permissions [dbo].[PermissionList] READONLY
AS
BEGIN
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	DELETE FROM [dbo].[Permissions]
	WHERE [Id] IN (SELECT [Id] FROM @Permissions WHERE [EntityState] = N'Deleted');

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Roles] AS t
		USING (
			SELECT 
				[Index], [Id], [Name], [Name2], [IsPublic], [Code]
			FROM @Roles 
			WHERE [EntityState] IN (N'Inserted', N'Updated')
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Name]			= s.[Name],
				t.[Name2]			= s.[Name2],
				t.[IsPublic]		= s.[IsPublic],
				t.[Code]			= s.[Code],
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Name], [Name2],	[IsPublic],		[Code]
			)
			VALUES (
				s.[Name], s.[Name2], s.[IsPublic], s.[Code]
			)
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

	MERGE INTO [dbo].[Permissions] AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] AS [RoleId], [ViewId], [Level], [Criteria], [Memo]
		FROM @Permissions L
		JOIN @IndexedIds II ON L.[HeaderIndex] = II.[Index]
		WHERE L.[EntityState] IN (N'Inserted', N'Updated')
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[ViewId]		= s.[ViewId], 
			t.[Level]		= s.[Level],
			t.[Criteria]	= s.[Criteria],
			t.[Memo]		= s.[Memo],
			t.[ModifiedAt]	= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([RoleId],	[ViewId],	[Level],	[Criteria], [Memo])
		VALUES (s.[RoleId], s.[ViewId], s.[Level], s.[Criteria], s.[Memo]);
END;