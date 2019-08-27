CREATE PROCEDURE [dal].[IfrsConcepts__Activate]
	@Ids [dbo].[StringList] READONLY,
	@IsActive bit
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[IfrsAccountClassifications] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]		= @IsActive;--,
			--t.[ModifiedAt]		= @Now,
			--t.[ModifiedById]	= @UserId;

	MERGE INTO [dbo].[IfrsEntryClassifications] AS t
		USING (
			SELECT [Id]
			FROM @Ids
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED AND (t.IsActive <> @IsActive)
		THEN
			UPDATE SET 
				t.[IsActive]		= @IsActive;--,
			--t.[ModifiedAt]		= @Now,
			--t.[ModifiedById]	= @UserId;

	MERGE INTO [dbo].[IfrsDisclosures] AS t
		USING (
			SELECT [Id]
			FROM @Ids
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED AND (t.IsActive <> @IsActive)
		THEN
			UPDATE SET 
				t.[IsActive]		= @IsActive;