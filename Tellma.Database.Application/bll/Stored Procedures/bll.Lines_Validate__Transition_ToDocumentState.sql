CREATE PROCEDURE [bll].[Lines_Validate__Transition_ToDocumentState]
	@Ids dbo.IndexedIdList READONLY, --documents
	@ToDocumentState TINYINT, -- 0: Open, 1:Close
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
	DECLARE
		@Documents DocumentList,
		@DocumentLineDefinitionEntries DocumentLineDefinitionEntryList, -- TODO: Add to signature everywhere
		@Lines LineList,
		@Entries EntryList,
		@ToState TINYINT;

	DECLARE @PreScript NVARCHAR(MAX) = N'
	SET NOCOUNT ON
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	------
	';
	DECLARE @Script NVARCHAR (MAX);
	DECLARE @PostScript NVARCHAR(MAX) = N'
	-----
	SELECT TOP (@Top) * FROM @ValidationErrors;
	';

	INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [ResourceId], [ResourceIsCommon],
		[NotedAgentId], [NotedAgentIsCommon],[NotedResourceId], [NotedResourceIsCommon],
		[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon], 
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]
	)
	SELECT [Id], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],  [ResourceId], [ResourceIsCommon],
		[NotedAgentId], [NotedAgentIsCommon],[NotedResourceId], [NotedResourceIsCommon],
		[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon],
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]
	FROM dbo.Documents
	WHERE [Id] IN (SELECT [Id] FROM @Ids)

	INSERT INTO @DocumentLineDefinitionEntries(
		[Index], [DocumentIndex], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],
		[NotedAgentId], [NotedAgentIsCommon], [ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon],
		[Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Time2], [Time2IsCommon], [ExternalReference], [ExternalReferenceIsCommon], [InternalReference],
		[InternalReferenceIsCommon])
	SELECT 		[Id], [DocumentId], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],
		[NotedAgentId], [NotedAgentIsCommon], [ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon],
		[Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Time2], [Time2IsCommon], [ExternalReference], [ExternalReferenceIsCommon], [InternalReference],
		[InternalReferenceIsCommon]
	FROM DocumentLineDefinitionEntries
	WHERE [DocumentId] IN (SELECT [Id] FROM @Ids)
	AND [LineDefinitionId]  IN (SELECT [Id] FROM map.LineDefinitions() WHERE [HasWorkflow] = 0);

	-- Verify that workflow-less lines in Events can be in state draft
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],		[Memo])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo]
	FROM dbo.Lines L
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	AND L.[DefinitionId] IN (
		SELECT [Id] FROM map.LineDefinitions()
		WHERE [HasWorkflow] = 0
		AND [SignValidateScript] IS NOT NULL -- no need to read lines without transition validation script
	);
	
	INSERT INTO @Entries (
	[Index], [LineIndex], [DocumentIndex], [Id],
	[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId],
	[CenterId],	[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [Time1],
	[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
	[NotedAmount], [NotedDate])
	SELECT
	E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
	E.[Direction],E.[AccountId],E.[CurrencyId],E.[AgentId],E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId],
	E.[CenterId], E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value],E.[Time1],
	E.[Time2],E.[ExternalReference],E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
	E.[NotedAmount],E.[NotedDate]
	FROM dbo.Entries E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	DECLARE @SignValidateScriptLineDefinitions [dbo].[StringList], @LineDefinitionId INT;
	DECLARE @LineState SMALLINT, @D DocumentList, @L LineList, @E EntryList;
	INSERT INTO @SignValidateScriptLineDefinitions
	SELECT DISTINCT DefinitionId FROM @Lines
	WHERE DefinitionId IN (
		SELECT [Id] FROM dbo.LineDefinitions
		WHERE [SignValidateScript] IS NOT NULL
	);
	
	IF EXISTS (SELECT * FROM @SignValidateScriptLineDefinitions)
	BEGIN
		-- run script to validate information
		DECLARE LineDefinition_Cursor CURSOR FOR SELECT [Id] FROM @SignValidateScriptLineDefinitions;  
		OPEN LineDefinition_Cursor  
		FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId; 
		WHILE @@FETCH_STATUS = 0  
		BEGIN 
			SELECT @Script =  @PreScript + ISNULL([SignValidateScript],N'') + @PostScript
			FROM dbo.LineDefinitions WHERE [Id] = @LineDefinitionId;
			DELETE FROM @L; DELETE FROM @E;
			INSERT INTO @L SELECT * FROM @Lines WHERE DefinitionId = @LineDefinitionId
			INSERT INTO @E SELECT E.* FROM @Entries E JOIN @L L ON E.LineIndex = L.[Index] AND E.DocumentIndex = L.DocumentIndex
			
			BEGIN TRY
				INSERT INTO @ValidationErrors
				EXECUTE	dbo.sp_executesql @Script, N'
					@LineDefinitionId INT,
					@ToState SMALLINT,
					@Documents [dbo].[DocumentList] READONLY,
					@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
					@Lines [dbo].[LineList] READONLY, 
					@Entries [dbo].EntryList READONLY,
					@Top INT', 	@LineDefinitionId = @LineDefinitionId, @ToState = @ToState, @Documents = @Documents,
					@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries, @Lines = @L, @Entries = @E, @Top = @Top;
			END TRY
			BEGIN CATCH
				DECLARE @ErrorNumber INT = 100000 + ERROR_NUMBER();
				DECLARE @ErrorMessage NVARCHAR (255) =
					CAST(@LineDefinitionId AS NVARCHAR (50)) + N':::' + ERROR_MESSAGE();
				DECLARE @ErrorState TINYINT = 99;
			--	SELECT TOP(@Top) * FROM @ValidationErrors; -- needed or else C# fails to parse according to contract
				THROW @ErrorNumber, @ErrorMessage, @ErrorState;
			--	RAISERROR( @ErrorMessage, 16, 1)
			END CATCH
			
			FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId;
		END
		CLOSE LineDefinition_Cursor
		DEALLOCATE LineDefinition_Cursor
	END	
	
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	IF (@IsError = 1)
		SELECT TOP(@Top) * FROM @ValidationErrors;
END;
GO