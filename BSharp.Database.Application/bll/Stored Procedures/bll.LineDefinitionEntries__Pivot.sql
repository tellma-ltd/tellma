CREATE PROCEDURE [bll].[LineDefinitionEntries__Pivot]
	@Index INT,
	@DocumentIndex INT,
	@DefinitionId NVARCHAR (50)
AS
	DECLARE @WideLines dbo.WideLineList;

	INSERT INTO @WideLines([Index], [DocumentIndex],[DefinitionId])
	SELECT					@Index, @DocumentIndex , @DefinitionId 
	FROM dbo.LineDefinitions
	WHERE [Id] = @DefinitionId

	UPDATE WL
	SET
		WL.[Direction1]			= LDE.[Direction],
		WL.[ContractType1]		= LDE.ContractType,
		WL.[AgentDefinitionId1]	= LDE.AgentDefinitionId
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.EntryNumber = 1

	UPDATE WL
	SET
		WL.[Direction2]			= LDE.[Direction],
		WL.[ContractType2]		= LDE.ContractType,
		WL.[AgentDefinitionId2]	= LDE.AgentDefinitionId
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.EntryNumber = 2

	UPDATE WL
	SET
		WL.[Direction3]			= LDE.[Direction],
		WL.[ContractType3]		= LDE.ContractType,
		WL.[AgentDefinitionId3]	= LDE.AgentDefinitionId
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.EntryNumber = 3

	SELECT * FROM @WideLines;