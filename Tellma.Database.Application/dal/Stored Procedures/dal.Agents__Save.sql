CREATE PROCEDURE [dal].[Agents__Save]
	@Entities [dbo].[AgentList] READONLY,
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
		MERGE INTO [dbo].[Agents] AS t
		USING (
			SELECT 	
				[Index], [Id],
				[Name], 
				[Name2], 
				[Name3],		
				[IsRelated]						
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
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
				[IsRelated], 
				[CreatedById], 
				[CreatedAt], 
				[ModifiedById], 
				[ModifiedAt])
			VALUES (
				s.[Name], 
				s.[Name2], 
				s.[Name3],					
				s.[IsRelated], 
				@UserId, 
				@Now, 
				@UserId, 
				@Now)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;