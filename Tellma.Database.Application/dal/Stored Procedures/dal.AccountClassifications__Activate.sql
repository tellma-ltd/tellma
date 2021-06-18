CREATE PROCEDURE [dal].[AccountClassifications__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	MERGE INTO [dbo].[AccountClassifications] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED AND (t.[IsActive] <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]		= @IsActive,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;

	IF @IsActive = 0
		MERGE INTO [dbo].[Accounts] AS t
		USING (
			SELECT [Id]
			FROM [dbo].[Accounts]
			WHERE [ClassificationId] IN (SELECT [Id] FROM @Ids)
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED AND (t.[IsActive] = 0)
		THEN
			UPDATE SET 
				t.[IsActive]		= 1,
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId;
END;