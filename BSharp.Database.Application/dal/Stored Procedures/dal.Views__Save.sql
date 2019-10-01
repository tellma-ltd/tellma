CREATE PROCEDURE [dal].[Views__Save]
	@Views [dbo].[ViewList] READONLY, 
	@Permissions [dbo].[PermissionList] READONLY,
	@ReturnIds BIT = 0
AS
BEGIN
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	MERGE INTO [dbo].[Views] AS t
	USING (
		SELECT [Id]	FROM @Views
	) AS s ON (t.Id = s.Id)
	WHEN NOT MATCHED THEN
		INSERT ([Id]) VALUES (s.[Id]);

	MERGE INTO [dbo].[Permissions] AS t
	USING (
		SELECT [Index], [Id], [RoleId], [ViewId], [Action], [Mask], [Criteria], [Memo]
		FROM @Permissions
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[ViewId]		= s.[ViewId], 
			t.[RoleId]		= s.[RoleId], 
			t.[Action]		= s.[Action],
			t.[Mask]		= s.[Mask],
			t.[Criteria]	= s.[Criteria],
			t.[Memo]		= s.[Memo]
	WHEN NOT MATCHED THEN
		INSERT ([RoleId],	[ViewId],	[Action],	[Mask],		[Criteria], [Memo])
		VALUES (s.[RoleId], s.[ViewId], s.[Action], s.[Mask], s.[Criteria], s.[Memo])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF (@ReturnIds = 1)
		SELECT * FROM @IndexedIds;
END;