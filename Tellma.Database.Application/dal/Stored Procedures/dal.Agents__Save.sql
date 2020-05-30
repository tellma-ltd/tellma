CREATE PROCEDURE [dal].[Agents__Save]
	@Entities [dbo].[AgentList] READONLY,
	@AgentUsers dbo.[AgentUserList] READONLY,
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
		MERGE INTO [dbo].[Agents] AS t
		USING (
			SELECT 	
				[Index], [Id],
				[Name], 
				[Name2], 
				[Name3],		
				[IsRelated]						
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Name]				= s.[Name],
				t.[Name2]				= s.[Name2],
				t.[Name3]				= s.[Name3],
				t.[IsRelated]			= s.[IsRelated],	
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Name], 
				[Name2], 
				[Name3],					
				[IsRelated]			
				)
			VALUES (
				s.[Name], 
				s.[Name2], 
				s.[Name3],					
				s.[IsRelated]		
				)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	WITH AU AS (
		SELECT * FROM dbo.AgentUsers RU
		WHERE RU.AgentId IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO AU AS t
	USING (
		SELECT
			RU.[Id],
			I.[Id] AS [AgentId],
			RU.[UserId]
		FROM @AgentUsers RU
		JOIN @IndexedIds I ON RU.[HeaderIndex] = I.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[UserId] <> s.[UserId])
	THEN
		UPDATE SET
			t.[UserId]					= s.[UserId],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[AgentId],
			[UserId]
		) VALUES (
			s.[AgentId],
			s.[UserId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
