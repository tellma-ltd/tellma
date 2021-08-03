CREATE PROCEDURE [dal].[Roles__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	MERGE INTO [dbo].[Roles] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED AND (t.[IsActive] <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[SavedById]	= @UserId;

	-- So clients update their cache
	UPDATE [dbo].[Users] SET [PermissionsVersion] = NEWID()
	-- TODO: WHERE [Id] IN (SELECT [Id] FROM @AffectedUserIds);
END;