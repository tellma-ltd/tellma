CREATE PROCEDURE [dal].[Lookups__Save]
	@DefinitionId INT,
	@Entities [dbo].[LookupList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Lookups] AS t
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
				t.[Code]			= s.[Code],
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([DefinitionId], [Name], [Name2], [Name3], [Code], [SortKey], [CreatedById], [CreatedAt], [ModifiedById], [ModifiedAt])
			VALUES (@DefinitionId, s.[Name], s.[Name2], s.[Name3], s.[Code], s.[Index], @UserId, @Now, @UserId, @Now)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;
