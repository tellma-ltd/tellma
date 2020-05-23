CREATE PROCEDURE [dal].[AccountMappings__Save]
	@Entities [dbo].[AccountMappingList] READONLY,
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
		MERGE INTO [dbo].[AccountMappings] AS t
		USING (
			SELECT 
				[Index], [Id],
				[AccountTypeId],
				[CenterId],
				[ContractId],
				[ResourceId],
				[CurrencyId],
				[AccountId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[DesignationId]= s.[AccountTypeId],
				t.[CenterId]			= s.[CenterId],
				t.[ContractId]			= s.[ContractId],
				t.[ResourceId]			= s.[ResourceId],
				t.[CurrencyId]			= s.[CurrencyId],
				t.[AccountId]			= s.[AccountId],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DesignationId],
				[CenterId],
				[ContractId],
				[ResourceId],
				[CurrencyId],
				[AccountId])
			VALUES (
				s.[AccountTypeId],
				s.[CenterId],
				s.[ContractId],
				s.[ResourceId],
				s.[CurrencyId],
				s.[AccountId])
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;