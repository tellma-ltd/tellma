CREATE PROCEDURE [dal].[AccountTypes__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	MERGE INTO [dbo].[AccountTypes] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[SavedById]	= @UserId;
END;