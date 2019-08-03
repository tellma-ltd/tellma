CREATE PROCEDURE [dbo].[dal_Views__Save]
	@Views [dbo].[ViewList] READONLY, 
	@Permissions [dbo].[PermissionList] READONLY
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	DELETE FROM [dbo].[Permissions]
	WHERE [Id] IN (SELECT [Id] FROM @Permissions WHERE [EntityState] = N'Deleted');

	MERGE INTO [dbo].[Views] AS t
	USING (
		SELECT 
			[Id]
		FROM @Views 
		WHERE [EntityState] IN (N'Inserted', N'Updated')
	) AS s ON (t.Id = s.Id)
	--WHEN MATCHED 
	--THEN
	--	UPDATE SET
	--		t.[ModifiedAt]	= @Now,
	--		t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[Id], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById]
		)
		VALUES (
			s.[Id], @Now,		@UserId,		@Now,		@UserId
		);

		MERGE INTO [dbo].[Permissions] AS t
		USING (
			SELECT [Index], [Id], [ViewId], [Level], [Criteria], [Memo]
			FROM @Permissions
			WHERE [EntityState] IN (N'Inserted', N'Updated')
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
			INSERT ([ViewId], [Level],	[Criteria], [Memo], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
			VALUES (s.[ViewId], s.[Level], s.[Criteria], s.[Memo], @Now,		@UserId,		@Now,		@UserId);
END;