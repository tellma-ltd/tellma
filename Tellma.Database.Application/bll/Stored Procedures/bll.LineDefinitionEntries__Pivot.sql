CREATE PROCEDURE [bll].[LineDefinitionEntries__Pivot]
	@Index INT,
	@DocumentIndex INT,
	@DefinitionId INT
AS
	DECLARE @WideLines dbo.WideLineList;

	INSERT INTO @WideLines([Index], [DocumentIndex],[DefinitionId])
	SELECT					@Index, @DocumentIndex , @DefinitionId 
	FROM dbo.LineDefinitions
	WHERE [Id] = @DefinitionId

	UPDATE WL
	SET
		WL.[Direction0]					= LDE.[Direction]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 0

	UPDATE WL
	SET
		WL.[Direction1]					= LDE.[Direction]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 1

	UPDATE WL
	SET
		WL.[Direction2]					= LDE.[Direction]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 2

	SELECT * FROM @WideLines;