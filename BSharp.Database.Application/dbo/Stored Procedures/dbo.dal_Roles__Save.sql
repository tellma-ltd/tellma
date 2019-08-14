CREATE PROCEDURE [dbo].[dal_Roles__Save]
	@Roles [dbo].[RoleList] READONLY, 
	@Permissions [dbo].[PermissionList] READONLY,
	@ReturnIds BIT = 0
AS
BEGIN
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Roles] AS t
		USING (
			SELECT 
				[Index], [Id], [Name], [Name2], [IsPublic], [Code]
			FROM @Roles 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Name]			= s.[Name],
				t.[Name2]			= s.[Name2],
				t.[IsPublic]		= s.[IsPublic],
				t.[Code]			= s.[Code]
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
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[ViewId]		= s.[ViewId], 
			t.[Action]		= s.[Level],
			t.[Criteria]	= s.[Criteria],
			t.[Memo]		= s.[Memo]
	WHEN NOT MATCHED THEN
		INSERT ([RoleId],	[ViewId],	[Action],	[Criteria], [Memo])
		VALUES (s.[RoleId], s.[ViewId], s.[Level], s.[Criteria], s.[Memo])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;


	IF (@ReturnIds = 1)
		SELECT * FROM @IndexedIds;
END;