CREATE PROCEDURE [dal].[Accounts__Save]
	@DefinitionId NVARCHAR (50),
	@Entities [dbo].[AccountList] READONLY,
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
		MERGE INTO [dbo].[Accounts] AS t
		USING (
			SELECT 
				[Index], [Id],
				@DefinitionId AS [AccountDefinitionId],
				[AccountClassificationId], 
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				[PartyReference],
				[ResourceId],
				[AgentId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[AccountDefinitionId]		= s.[AccountDefinitionId],
				t.[AccountClassificationId]	= s.[AccountClassificationId], 
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Code]					= s.[Code],
				t.[PartyReference]			= s.[PartyReference],
				t.[ResourceId]				= s.[ResourceId],
				t.[AgentId]					= s.[AgentId],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[AccountDefinitionId],
				[AccountClassificationId], 
				[Name], [Name2], [Name3], 
				[Code], 
				[PartyReference],
				[ResourceId],
				[AgentId])
			VALUES (
				s.[AccountDefinitionId],
				s.[AccountClassificationId], 
				s.[Name], s.[Name2], s.[Name3], 
				s.[Code], 
				s.[PartyReference],
				s.[ResourceId],
				s.[AgentId])
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;