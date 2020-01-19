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
				[ResponsibilityCenterId],
				[Name], 
				[Name2], 
				[Name3], 
				[Code],
				[AccountTypeId],
				[IsCurrent],
				--[PartyReference],
				[LegacyClassificationId],
				[LegacyType],
				[AgentDefinitionId],
				[HasResource],
				[HasAgent],
				[IsRelated],
				[HasReferenceAgent],
				[HasReferenceText],
				[HasReferenceAmount],
				[AgentId],
				[ResourceId],
				[CurrencyId],
				[Identifier],
				[EntryTypeId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[ResponsibilityCenterId]	= s.[ResponsibilityCenterId],
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Code]					= s.[Code],
				t.[AccountTypeId]			= s.[AccountTypeId],
				t.[IsCurrent]				= s.[IsCurrent],
				--t.[PartyReference]			= s.[PartyReference],
				t.[LegacyClassificationId]	= s.[LegacyClassificationId], 
				t.[LegacyType]				= s.[LegacyType],
				t.[AgentDefinitionId]		= s.[AgentDefinitionId],
				t.[HasResource]				= s.[HasResource],
				t.[HasAgent]				= s.[HasAgent],
				t.[IsRelated]				= s.[IsRelated],
				t.[HasReferenceAgent]		= s.[HasReferenceAgent],
				t.[HasReferenceText]		= s.[HasReferenceText],
				t.[HasReferenceAmount]		= s.[HasReferenceAmount],
				t.[AgentId]					= s.[AgentId],
				t.[ResourceId]				= s.[ResourceId],
				t.[CurrencyId]				= s.[CurrencyId],
				t.[Identifier]				= s.[Identifier],
				t.[EntryTypeId]				= s.[EntryTypeId],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[ResponsibilityCenterId],
				[Name], 
				[Name2], 
				[Name3], 
				[Code],
				[AccountTypeId],
				[IsCurrent],
				--[PartyReference],
				[LegacyClassificationId],
				[LegacyType],
				[AgentDefinitionId],
				[HasResource],
				[HasAgent],
				[IsRelated],
				[HasReferenceAgent],
				[HasReferenceText],
				[HasReferenceAmount],
				[AgentId],
				[ResourceId],
				[CurrencyId],
				[Identifier],
				[EntryTypeId])
			VALUES (
				s.[ResponsibilityCenterId],
				s.[Name], 
				s.[Name2], 
				s.[Name3], 
				s.[Code],
				s.[AccountTypeId],
				s.[IsCurrent],
				--s.[PartyReference],
				s.[LegacyClassificationId],
				s.[LegacyType],
				s.[AgentDefinitionId],
				s.[HasResource],
				s.[HasAgent],
				s.[IsRelated],
				s.[HasReferenceAgent],
				s.[HasReferenceText],
				s.[HasReferenceAmount],
				s.[AgentId],
				s.[ResourceId],
				s.[CurrencyId],
				s.[Identifier],
				s.[EntryTypeId])
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;