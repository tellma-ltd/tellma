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
				[AccountTypeId],
				[CenterId],
				[Name], 
				[Name2], 
				[Name3], 
				[Code],
				[ClassificationId],
				[ContractDefinitionId],
				[ContractId],
				[ResourceDefinitionId],
				[ResourceId],
				[CurrencyId],
				[EntryTypeId],
				[NotedContractDefinitionId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
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
				t.[ContractDefinitionId]= s.[ContractDefinitionId],
				t.[ContractId]			= s.[ContractId],
				t.[ResourceDefinitionId]= s.[ResourceDefinitionId],
				t.[ResourceId]			= s.[ResourceId],
				t.[CurrencyId]			= s.[CurrencyId],
				t.[EntryTypeId]			= s.[EntryTypeId],
				t.[NotedContractDefinitionId]= s.[NotedContractDefinitionId],
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
				[ContractDefinitionId],
				[ContractId],
				[ResourceDefinitionId],
				[ResourceId],
				[CurrencyId],
				[EntryTypeId],
				[NotedContractDefinitionId])
			VALUES (
				s.[AccountTypeId],
				s.[CenterId],
				s.[Name], 
				s.[Name2], 
				s.[Name3], 
				s.[Code],

				s.[ClassificationId],
				s.[ContractDefinitionId],
				s.[ContractId],
				s.[ResourceDefinitionId],
				s.[ResourceId],
				s.[CurrencyId],
				s.[EntryTypeId],
				s.[NotedContractDefinitionId])
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;