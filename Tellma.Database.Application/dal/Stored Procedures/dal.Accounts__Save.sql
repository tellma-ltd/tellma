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
				[DefinitionId],
				[CenterId],
				[Name], 
				[Name2], 
				[Name3], 
				[Code],
				[IfrsTypeId],
				[ClassificationId],
				[RelationId],
				[ContractId],
				[ResourceId],
				[CurrencyId],
				[EntryTypeId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[DefinitionId]		= s.[DefinitionId],
				t.[CenterId]			= s.[CenterId],
				t.[Name]				= s.[Name],
				t.[Name2]				= s.[Name2],
				t.[Name3]				= s.[Name3],
				t.[Code]				= s.[Code],
				t.[IfrsTypeId]			= s.[IfrsTypeId],
				t.[ClassificationId]	= s.[ClassificationId], 
				t.[RelationId]			= s.[RelationId],
				t.[ContractId]			= s.[ContractId],
				t.[ResourceId]			= s.[ResourceId],
				t.[CurrencyId]			= s.[CurrencyId],
				t.[EntryTypeId]			= s.[EntryTypeId],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DefinitionId],
				[CenterId],
				[Name], 
				[Name2], 
				[Name3], 
				[Code],
				[IfrsTypeId],
				[ClassificationId],
				[RelationId],
				[ContractId],
				[ResourceId],
				[CurrencyId],
				[EntryTypeId])
			VALUES (
				s.[DefinitionId],
				s.[CenterId],
				s.[Name], 
				s.[Name2], 
				s.[Name3], 
				s.[Code],
				s.[IfrsTypeId],
				s.[ClassificationId],
				s.[RelationId],
				s.[ContractId],
				s.[ResourceId],
				s.[CurrencyId],
				s.[EntryTypeId])
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;