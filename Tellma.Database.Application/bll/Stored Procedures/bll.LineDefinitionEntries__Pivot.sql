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

	UPDATE WL
	SET
		WL.[Direction3]					= LDE.[Direction]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 3

		UPDATE WL
	SET
		WL.[Direction4]					= LDE.[Direction]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 4

	UPDATE WL
	SET
		WL.[Direction5]					= LDE.[Direction]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 5

	UPDATE WL
	SET
		WL.[Direction6]					= LDE.[Direction]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 6

	UPDATE WL
	SET
		WL.[Direction7]					= LDE.[Direction]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 7

	SELECT * FROM @WideLines;