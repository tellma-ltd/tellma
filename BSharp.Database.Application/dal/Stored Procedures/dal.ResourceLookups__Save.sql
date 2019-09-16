CREATE PROCEDURE [dal].[ResourceLookups__Save]
	@DefinitionId NVARCHAR(255),
	@Entities [ResourceLookupList] READONLY,
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
		MERGE INTO [dbo].[ResourceLookups] AS t
		USING (
			SELECT [Index], [Id], [Name], [Name2], [Name3], [Code]
			FROM @Entities 
		) AS s ON (t.Id = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[Name]			= s.[Name],
				t.[Name2]			= s.[Name2],
				t.[Name3]			= s.[Name3],
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([ResourceLookupDefinitionId], [Name], [Name2], [Name3], [Code])
			VALUES (@DefinitionId, s.[Name], s.[Name2], s.[Name3], s.[Code])
		OUTPUT s.[Index], inserted.[Id]
	) AS x
	OPTION (RECOMPILE);

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;