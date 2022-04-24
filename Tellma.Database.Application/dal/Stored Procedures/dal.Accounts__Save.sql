CREATE PROCEDURE [dal].[Accounts__Save]
	@Entities [dbo].[AccountList] READONLY,
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
		MERGE INTO [dbo].[Accounts] AS t
		USING (
			SELECT 
				[Index], [Id],
				[AccountTypeId],
				[CenterId],
				[Name], 
				[Name2], 
				[Name3], 
				[Code],
				[ClassificationId],
				[AgentDefinitionId],
				[AgentId],
				[ResourceDefinitionId],
				[ResourceId],
				[NotedAgentDefinitionId],
				[NotedAgentId],
				[NotedResourceDefinitionId],
				[NotedResourceId],
				[CurrencyId],
				[EntryTypeId],
				[IsAutoSelected]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[AccountTypeId]		= s.[AccountTypeId],
				t.[CenterId]			= s.[CenterId],
				t.[Name]				= s.[Name],
				t.[Name2]				= s.[Name2],
				t.[Name3]				= s.[Name3],
				t.[Code]				= s.[Code],

				t.[ClassificationId]	= s.[ClassificationId],
				t.[AgentDefinitionId]	= s.[AgentDefinitionId],
				t.[AgentId]				= s.[AgentId],
				t.[ResourceDefinitionId]= s.[ResourceDefinitionId],
				t.[ResourceId]			= s.[ResourceId],
				t.[NotedAgentDefinitionId] = s.[NotedAgentDefinitionId],
				t.[NotedAgentId]		= s.[NotedAgentId],
				t.[NotedResourceDefinitionId] = s.[NotedResourceDefinitionId],
				t.[NotedResourceId]		= s.[NotedResourceId],
				t.[CurrencyId]			= s.[CurrencyId],
				t.[EntryTypeId]			= s.[EntryTypeId],
				t.[IsAutoSelected]		= s.[IsAutoSelected],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[AccountTypeId],
				[CenterId],
				[Name], 
				[Name2], 
				[Name3], 
				[Code],

				[ClassificationId],
				[AgentDefinitionId],
				[AgentId],
				[ResourceDefinitionId],
				[ResourceId],
				[NotedAgentDefinitionId],
				[NotedAgentId],
				[NotedResourceDefinitionId],
				[NotedResourceId],
				[CurrencyId],
				[EntryTypeId],
				[IsAutoSelected],
				[CreatedById], 
				[CreatedAt], 
				[ModifiedById], 
				[ModifiedAt])
			VALUES (
				s.[AccountTypeId],
				s.[CenterId],
				s.[Name], 
				s.[Name2], 
				s.[Name3], 
				s.[Code],

				s.[ClassificationId],
				s.[AgentDefinitionId],
				s.[AgentId],
				s.[ResourceDefinitionId],
				s.[ResourceId],
				s.[NotedAgentDefinitionId],
				s.[NotedAgentId],
				s.[NotedResourceDefinitionId],
				s.[NotedResourceId],
				s.[CurrencyId],
				s.[EntryTypeId],
				s.[IsAutoSelected],
				@UserId, 
				@Now, 
				@UserId, 
				@Now)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;