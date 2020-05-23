CREATE PROCEDURE [bll].[AccountMappings__Preprocess]
	@Entities [dbo].[AccountMappingList] READONLY,
	@PreprocessedEntitiesJson NVARCHAR (MAX) = NULL OUTPUT 
AS
	SET NOCOUNT ON;
	DECLARE @PreprocessedEntities [dbo].[AccountMappingList];
	INSERT INTO @PreprocessedEntities SELECT * FROM @Entities;

	-- if a mapping dimension is null, and it exists in Accounts, copy it.
	-- if not null, then validate will catch any discrepancies
	UPDATE AM
	SET
		AM.[CenterId] = COALESCE(AM.[CenterId], A.[CenterId]),
		AM.[ContractId] = COALESCE(AM.[ContractId], A.[ContractId]),
		AM.[ResourceId] = COALESCE(AM.[ResourceId], A.[ResourceId]),
		AM.[CurrencyId] = COALESCE(AM.[CurrencyId], A.[CurrencyId])
	FROM @PreprocessedEntities AM
	JOIN Accounts A ON AM.[AccountId] = A.[Id]

	SELECT @PreprocessedEntitiesJson = 
	(
		SELECT *
		FROM @PreprocessedEntities
		FOR JSON PATH
	);	

	-- We're still assuming that preprocess only modifies, it doesn't insert nor deletes
	SELECT * FROM @PreprocessedEntities;