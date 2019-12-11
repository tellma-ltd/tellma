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

				[Name], 
				[Name2], 
				[Name3], 
				[Code],
				[IsSmart],
				--[PartyReference],
				[AccountTypeId],
				[AccountClassificationId],
				-- Not used
				[ResponsibilityCenterId],
				[ContractType],
				[AgentDefinitionId],
				[ResourceClassificationId],
				[IsCurrent],
				[AgentId],
				[ResourceId],
				[CurrencyId],
				[Identifier],
				[EntryClassificationId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 

				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Code]					= s.[Code],
				t.[IsSmart]					= s.[IsSmart],
				--t.[PartyReference]			= s.[PartyReference],
				t.[AccountTypeId]			= s.[AccountTypeId],
				t.[AccountClassificationId]	= s.[AccountClassificationId], 
				-- Not used
				t.[ResponsibilityCenterId]	= s.[ResponsibilityCenterId],
				t.[ContractType]			= s.[ContractType],
				t.[AgentDefinitionId]		= s.[AgentDefinitionId],
				t.[ResourceClassificationId]= s.[ResourceClassificationId],
				t.[IsCurrent]				= s.[IsCurrent],
				t.[AgentId]					= s.[AgentId],
				t.[ResourceId]				= s.[ResourceId],
				t.[CurrencyId]				= s.[CurrencyId],
				t.[Identifier]				= s.[Identifier],
				t.[EntryClassificationId]	= s.[EntryClassificationId],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Name], [Name2], [Name3], 
				[Code],
				[IsSmart],
				--[PartyReference],
				[AccountTypeId],
				[AccountClassificationId],
				-- Not used
				[ResponsibilityCenterId],
				[ContractType],
				[AgentDefinitionId],
				[ResourceClassificationId],
				[IsCurrent],
				[AgentId],
				[ResourceId],
				[CurrencyId],
				[Identifier],
				[EntryClassificationId])
			VALUES (
				s.[Name], s.[Name2], s.[Name3], 
				s.[Code],
				s.[IsSmart],
				--s.[PartyReference],
				s.[AccountTypeId],
				s.[AccountClassificationId], 
				s.[ResponsibilityCenterId],
				s.[ContractType],
				s.[AgentDefinitionId],
				s.[ResourceClassificationId],
				s.[IsCurrent],
				s.[AgentId],
				s.[ResourceId],
				s.[CurrencyId],
				s.[Identifier],
				s.[EntryClassificationId])
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;