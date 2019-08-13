CREATE PROCEDURE [dbo].[dal_ResourceLookup1s__Save]
	@Entities [ResourceLookupList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].ResourceLookup1s AS t
	USING (
		SELECT [Index], [Id], [SortKey], [Name], [Name2], [Name3]
		FROM @Entities 
		WHERE [EntityState] IN (N'Inserted', N'Updated')
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET 
			t.[Name]			= s.[Name],
			t.[Name2]			= s.[Name2],
			t.[Name3]			= s.[Name3],
			t.[SortKey]			= s.[SortKey],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([Name], [Name2], [Name3], [SortKey])
		VALUES (s.[Name], s.[Name2], s.[Name3], s.[SortKey])
	OPTION (RECOMPILE);