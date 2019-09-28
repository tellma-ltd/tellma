CREATE PROCEDURE [dal].[Views__Activate]
	@Ids dbo.ViewList READONLY,
	@IsActive bit
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[Views] AS t
		USING (
			SELECT [Id]
			FROM @Ids 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED AND (t.IsActive <> @IsActive)
		THEN
			UPDATE SET 
				t.[IsActive]		= @IsActive,
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId;