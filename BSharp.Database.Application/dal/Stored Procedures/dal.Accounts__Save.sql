CREATE PROCEDURE [dal].[Accounts__Save]
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
				[AccountClassificationId], 
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				--[PartyReference],
				[AccountTypeId],
				[AgentDefinitionId],
				[ResourceTypeId],
				[IsCurrent],
				[AgentId],
				[ResourceId],
				[ResponsibilityCenterId],
				[DescriptorId],
				[EntryTypeId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[AccountClassificationId]	= s.[AccountClassificationId], 
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Code]					= s.[Code],
				--t.[PartyReference]			= s.[PartyReference],
				t.[AccountTypeId]			= s.[AccountTypeId],
				t.[AgentDefinitionId]		= s.[AgentDefinitionId],
				t.[ResourceTypeId]			= s.[ResourceTypeId],
				t.[IsCurrent]				= s.[IsCurrent],
				t.[AgentId]					= s.[AgentId],
				t.[ResourceId]				= s.[ResourceId],
				t.[ResponsibilityCenterId]	= s.[ResponsibilityCenterId],
				t.[DescriptorId]			= s.[DescriptorId],
				t.[EntryTypeId]				= s.[EntryTypeId],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[AccountClassificationId], 
				[Name], [Name2], [Name3], 
				[Code], 
				--[PartyReference],
				[AccountTypeId],
				[AgentDefinitionId],
				[ResourceTypeId],
				[IsCurrent],
				[AgentId],
				[ResourceId],
				[ResponsibilityCenterId],
				[DescriptorId],
				[EntryTypeId])
			VALUES (
				s.[AccountClassificationId], 
				s.[Name], s.[Name2], s.[Name3], 
				s.[Code], 
				--s.[PartyReference],
				s.[AccountTypeId],
				s.[AgentDefinitionId],
				s.[ResourceTypeId],
				s.[IsCurrent],
				s.[AgentId],
				s.[ResourceId],
				s.[ResponsibilityCenterId],
				s.[DescriptorId],
				s.[EntryTypeId])
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;