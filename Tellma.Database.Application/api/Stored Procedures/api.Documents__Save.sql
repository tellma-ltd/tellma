CREATE PROCEDURE [api].[Documents__Save]
	@DefinitionId NVARCHAR(255),
	@Documents [dbo].[DocumentList] READONLY,
	@WideLines dbo.[WideLineList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	DECLARE @AllLines dbo.[LineList];
	DECLARE @AllEntries dbo.EntryList;
	DECLARE @PreprocessedWideLines dbo.[WideLineList];
	DECLARE @PreprocessedEntries [dbo].EntryList;

	-- THe following results in nested INSERT EXEC, which is not allowed. Solution: Flatten the SProc.
	--INSERT INTO @PreprocessedWideLines
	--EXEC bll.[WideLines__Preprocess] @WideLines;

	DECLARE @LineDefinitionId NVARCHAR (50);
	DECLARE @Script NVARCHAR (500);

	SELECT @LineDefinitionId = MIN([DefinitionId])
	FROM @WideLines WL
	JOIN dbo.LineDefinitions LD ON LD.[Id] = WL.[DefinitionId]
	WHERE LD.[Script] IS NOT NULL;
	
	WHILE @LineDefinitionId IS NOT NULL
	BEGIN
		SELECT @Script = [Script] FROM dbo.LineDefinitions WHERE [Id] = @LineDefinitionId;

		DECLARE @WL dbo.[WideLineList]; DELETE FROM @WL;
		INSERT INTO @WL SELECT * FROM @WideLines WHERE [DefinitionId] = @LineDefinitionId;

		INSERT INTO @PreprocessedWideLines
		EXECUTE	sp_executesql @Script, N'@WideLines WideLineList READONLY', @WideLines = @WideLines;

		SET @LineDefinitionId = (
			SELECT MIN(WL.[DefinitionId])
			FROM @WideLines WL
			JOIN dbo.LineDefinitions LD ON LD.[Id] = WL.[DefinitionId]
			WHERE LD.[Script] IS NOT NULL
			AND WL.[DefinitionId] > @LineDefinitionId
		);
	END

	INSERT INTO @AllLines(	   
		   [Index],	[DocumentIndex], [Id], [DefinitionId], [ResponsibilityCenterId], [AgentId], [ResourceId], [CurrencyId], [MonetaryValue], [Quantity], [UnitId], [Value], [Memo])
	SELECT [Index], [DocumentIndex], [Id], [DefinitionId], [ResponsibilityCenterId], [AgentId], [ResourceId], [CurrencyId], [MonetaryValue], [Quantity], [UnitId], [Value], [Memo]
	FROM @Lines
	UNION
	SELECT [Index], [DocumentIndex], [Id], [DefinitionId],  [ResponsibilityCenterId], [AgentId], [ResourceId], [CurrencyId], [MonetaryValue], [Quantity], [UnitId], [Value], [Memo]
	FROM @PreprocessedWideLines

	INSERT INTO @AllEntries SELECT * FROM @Entries;
	INSERT INTO @AllEntries
	EXEC [bll].[WideLines__Unpivot] @PreprocessedWideLines;

	-- using line definition Id, the entries wil be filled
	INSERT INTO @PreprocessedEntries
	EXEC bll.[Documents__Preprocess]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @AllEntries;

	-- If Agent Id in Documents is not Null, then propagate it to the entries where the Agent Definition is compatible
	UPDATE PE
	SET AgentId = D.AgentId
	FROM @PreprocessedEntries PE
	JOIN @Lines L ON PE.[LineIndex] = L.[Index]
	JOIN @Documents D ON L.DocumentIndex = D.[Index]
	JOIN dbo.LineDefinitionEntries LDE ON PE.EntryNumber = LDE.EntryNumber AND L.DefinitionId = LDE.LineDefinitionId
	JOIN dbo.Agents AG ON D.AgentId = AG.Id
	WHERE LDE.AgentDefinitionList LIKE N'%' + AG.DefinitionId +'%'
			
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @PreprocessedEntries;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	DECLARE @ReturnResult NVARCHAR (MAX);
	--INSERT INTO @DocumentsIndexedIds
	EXEC [dal].[Documents__Save]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @PreprocessedEntries,
		@ReturnIds = 1,
		@ReturnResult = @ReturnResult OUTPUT;

	DECLARE @DocumentsIndexedIds [dbo].[IndexedIdList];
	INSERT INTO @DocumentsIndexedIds([Index], [Id])
	SELECT [Index], [Id]
	FROM OpenJson(@ReturnResult)
	WITH (
		[Index] INT '$.Index',
		[Id] INT '$.Id'
	);

	---- Assign the new ones to self
	DECLARE @NewDocumentsIds dbo.IdList;
	INSERT INTO @NewDocumentsIds([Id])
	SELECT Id FROM @DocumentsIndexedIds
	WHERE [Index] IN (SELECT [Index] FROM @Documents WHERE [Id] = 0);

	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	EXEC [dal].[Documents__Assign]
		@Ids = @NewDocumentsIds,
		@AssigneeId = @UserId,
		@Comment = N'FYC'

	IF @ReturnIds = 1
		SELECT * FROM @DocumentsIndexedIds;
END;