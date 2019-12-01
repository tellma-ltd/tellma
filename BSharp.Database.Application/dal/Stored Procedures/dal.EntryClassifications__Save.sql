CREATE PROCEDURE [dal].[EntryClassifications__Save]
	@Entities [EntryClassificationList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[EntryClassifications] AS t
		USING (
			SELECT
				E.[Index], E.[Id], E.[Code],
				E.[Name], E.[Name2], E.[Name3], E.[Node], E.[IsAssignable],
				E.[ForDebit], E.[ForCredit]
			FROM @Entities E
		) AS s ON (t.[Code] = s.[Code])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Name]				= s.[Name],
				t.[Name2]				= s.[Name2],
				t.[Name3]				= s.[Name3],
				t.[Node]				= s.[Node],
				t.[IsAssignable]		= s.[IsAssignable],
				t.[ForDebit]			= s.[ForDebit],
				t.[ForCredit]			= s.[ForCredit],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([Code], [Name],	[Name2], [Name3], [Node], [IsAssignable], [ForDebit], [ForCredit])
			VALUES (s.[Code], s.[Name], s.[Name2], s.[Name3], s.[Node], s.[IsAssignable], s.[ForDebit], s.[ForCredit])
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;