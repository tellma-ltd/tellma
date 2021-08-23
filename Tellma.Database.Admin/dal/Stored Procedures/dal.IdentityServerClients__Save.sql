CREATE PROCEDURE [dal].[IdentityServerClients__Save]
	@Entities [dbo].[IdentityServerClientList] READONLY,
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
		MERGE INTO [dbo].[IdentityServerClients] AS t
		USING (
			SELECT [Index], [Id], [Name], [Memo], [ClientId], [ClientSecret]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[Name]				= s.[Name],
				t.[Memo]				= s.[Memo],
				--t.[ClientId]			= s.[ClientId],
				--t.[ClientSecret]		= s.[ClientSecret],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([Name], [Memo], [ClientId], [ClientSecret], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
			VALUES (s.[Name], s.[Memo], s.[ClientId], s.[ClientSecret], @Now, @UserId, @Now, @UserId)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	-- Return the results if needed
	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END
