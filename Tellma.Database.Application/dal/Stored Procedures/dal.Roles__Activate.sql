CREATE PROCEDURE [dal].[Roles__Activate]
	@Ids [dbo].[IdList] READONLY,
	@IsActive bit
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].Roles AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[SavedById]	= @UserId;

	UPDATE [dbo].[Users] SET [PermissionsVersion] = NEWID()
	-- TODO: WHERE [Id] IN (SELECT [Id] FROM @AffectedUserIds);