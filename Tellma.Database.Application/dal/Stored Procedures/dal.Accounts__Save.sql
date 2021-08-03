﻿CREATE PROCEDURE [dal].[Accounts__Save]
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
				[RelationDefinitionId],
				[RelationId],
				[ResourceDefinitionId],
				[ResourceId],
				[NotedRelationDefinitionId],
				[NotedRelationId],
				[CurrencyId],
				[EntryTypeId]
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
				t.[RelationDefinitionId]= s.[RelationDefinitionId],
				t.[RelationId]			= s.[RelationId],
				t.[ResourceDefinitionId]= s.[ResourceDefinitionId],
				t.[ResourceId]			= s.[ResourceId],
				t.[NotedRelationDefinitionId] = s.[NotedRelationDefinitionId],
				t.[NotedRelationId]		= s.[NotedRelationId],
				t.[CurrencyId]			= s.[CurrencyId],
				t.[EntryTypeId]			= s.[EntryTypeId],
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
				[RelationDefinitionId],
				[RelationId],
				[ResourceDefinitionId],
				[ResourceId],
				[NotedRelationDefinitionId],
				[NotedRelationId],
				[CurrencyId],
				[EntryTypeId], 
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
				s.[RelationDefinitionId],
				s.[RelationId],
				s.[ResourceDefinitionId],
				s.[ResourceId],
				s.[NotedRelationDefinitionId],
				s.[NotedRelationId],
				s.[CurrencyId],
				s.[EntryTypeId], 
				@UserId, 
				@Now, 
				@UserId, 
				@Now)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;