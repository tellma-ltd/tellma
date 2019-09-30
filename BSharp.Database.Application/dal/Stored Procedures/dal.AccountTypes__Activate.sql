CREATE PROCEDURE [dal].[AccountTypes__Activate]
	@Ids [dbo].[StringList] READONLY,
	@IsActive BIT
AS
	MERGE INTO [dbo].[AccountTypes] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[IsActive] <> @IsActive)
	THEN UPDATE SET t.[IsActive] = @IsActive;