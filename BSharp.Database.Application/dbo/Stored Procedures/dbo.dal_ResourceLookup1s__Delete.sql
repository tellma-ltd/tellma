CREATE PROCEDURE [dbo].[dal_ResourceLookup1s__Delete]
	@Ids [dbo].[UuidList] READONLY,
	@IsDeleted BIT = 1
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].ResourceLookup1s AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsDeleted <> @IsDeleted)
	THEN
		UPDATE SET 
			t.[IsDeleted]	= @IsDeleted,
			t.[ModifiedAt]	= @Now,
			t.[ModifiedById]= @UserId;