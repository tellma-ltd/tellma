CREATE PROCEDURE [dal].[Documents__SaveAndRefresh]
	@DefinitionId NVARCHAR(255),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].[EntryList] READONLY,
	@Attachments [dbo].[AttachmentList] READONLY,
	@ReturnIds BIT = 0
	--,	@ReturnResult NVARCHAR(MAX) = NULL OUTPUT
AS
BEGIN
	DECLARE @DocumentsIndexedIds [dbo].[IndexedIdList], @LinesIndexedIds [dbo].[IndexIdWithHeaderList], @DeletedFileIds [dbo].[StringList];

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @IsOriginalDocument BIT = (SELECT IsOriginalDocument FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId);

	INSERT INTO @DocumentsIndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Documents] AS t
		USING (
			SELECT 
				[Index], [Id],
				[DocumentDate],
				[Clearance],
				[Memo], -- [Frequency], [Repetitions],
				[MemoIsCommon],
				[AgentId],
				[AgentIsCommon],
				[InvestmentCenterId],
				[InvestmentCenterIsCommon],
				[Time1],
				[Time1IsCommon],
				[Time2],
				[Time2IsCommon],
				[Quantity],
				[QuantityIsCommon],
				[UnitId],
				[UnitIsCommon],
				[SerialNumber] As ManualSerialNumber,
				ROW_Number() OVER (PARTITION BY [Id] ORDER BY [Index]) + (
					-- max(SerialNumber) per document type.
					SELECT ISNULL(MAX([SerialNumber]), 0) FROM dbo.Documents WHERE [DefinitionId] = @DefinitionId
				) As [AutoSerialNumber]
			FROM @Documents D
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[SerialNumber]			= IIF(@IsOriginalDocument = 1, 
												t.[SerialNumber],
												s.[ManualSerialNumber]),
				t.[DocumentDate]			= s.[DocumentDate],
				t.[Clearance]				= s.[Clearance],
				t.[Memo]					= s.[Memo],
				t.[MemoIsCommon]			= s.[MemoIsCommon],
				t.[AgentId]					= s.[AgentId],
				t.[AgentIsCommon]			= s.[AgentIsCommon],
				t.[InvestmentCenterId]		= s.[InvestmentCenterId],
				t.[InvestmentCenterIsCommon]= s.[InvestmentCenterIsCommon],
				t.[Time1]					= s.[Time1],
				t.[Time1IsCommon]			= s.[Time1IsCommon],
				t.[Time2]					= s.[Time2],
				t.[Time2IsCommon]			= s.[Time2IsCommon],
				t.[Quantity]				= s.[Quantity],
				t.[QuantityIsCommon]		= s.[QuantityIsCommon],
				t.[UnitId]		= s.[UnitId],
				t.[UnitIsCommon]	= s.[UnitIsCommon],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DefinitionId],
				[SerialNumber], 
				[DocumentDate],
				[Clearance],
				[Memo],
				[MemoIsCommon],
				[AgentId],
				[AgentIsCommon],
				[InvestmentCenterId],
				[InvestmentCenterIsCommon],
				[Time1],
				[Time1IsCommon],
				[Time2],
				[Time2IsCommon],
				[Quantity],
				[QuantityIsCommon],
				[UnitId],
				[UnitIsCommon]
			)
			VALUES (
				@DefinitionId,
				IIF(@IsOriginalDocument = 1, s.[AutoSerialNumber], s.[ManualSerialNumber]),
				s.[DocumentDate],
				s.[Clearance],
				s.[Memo],
				s.[MemoIsCommon],
				s.[AgentId],
				s.[AgentIsCommon],
				s.[InvestmentCenterId],
				s.[InvestmentCenterIsCommon],
				s.[Time1],
				s.[Time1IsCommon],
				s.[Time2],
				s.[Time2IsCommon],
				s.[Quantity],
				s.[QuantityIsCommon],
				s.[UnitId],
				s.[UnitIsCommon]
			)
		OUTPUT s.[Index], inserted.[Id] 
	) As x;
	
	WITH BL AS (
		SELECT * FROM dbo.[Lines]
		WHERE DocumentId IN (SELECT [Id] FROM @DocumentsIndexedIds)
	)
	INSERT INTO @LinesIndexedIds([Index], [HeaderId], [Id])
	SELECT x.[Index], x.[DocumentId], x.[Id]
	FROM
	(
		MERGE INTO BL AS t
		USING (
			SELECT
				L.[Index],
				L.[Id],
				DI.Id AS DocumentId,
				L.[DefinitionId],
				L.[ResponsibilityCenterId],
				L.[AgentId],
				L.[ResourceId],
				L.[CurrencyId],
				L.[MonetaryValue],
				L.[Quantity],
				L.[UnitId],
				L.[Value],
				L.[Memo]
			FROM @Lines L
			JOIN @DocumentsIndexedIds DI ON L.[DocumentIndex] = DI.[Index]
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[DefinitionId]		= s.[DefinitionId],
				t.[ResponsibilityCenterId]=s.[ResponsibilityCenterId],
				t.[AgentId]				= s.[AgentId],
				t.[ResourceId]			= s.[ResourceId],
				t.[CurrencyId]			= s.[CurrencyId],
				t.[MonetaryValue]		= s.[MonetaryValue],
				t.[Quantity]			= s.[Quantity],
				t.[UnitId]				= s.[UnitId],
				t.[Value]				= s.[Value],
				t.[Memo]				= s.[Memo],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([DocumentId], [DefinitionId], [SortKey],
				[ResponsibilityCenterId],
				[AgentId],
				[ResourceId],
				[CurrencyId],
				[MonetaryValue],
				[Quantity],
				[UnitId],
				[Value],
				[Memo]
			)
			VALUES (s.[DocumentId], s.[DefinitionId], s.[Index],
				s.[ResponsibilityCenterId],
				s.[AgentId],
				s.[ResourceId],
				s.[CurrencyId],
				s.[MonetaryValue],
				s.[Quantity],
				s.[UnitId],
				s.[Value],
				s.[Memo]
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id], inserted.[DocumentId]
	) AS x
	WHERE [Index] IS NOT NULL;

	WITH BE AS (
		SELECT * FROM dbo.[Entries]
		WHERE [LineId] IN (SELECT [Id] FROM @LinesIndexedIds)
	)
	MERGE INTO BE AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineId], E.[Index], E.[Direction], E.[AccountId],  E.[CurrencyId],
			E.[AgentId], E.[ResourceId], E.[ResponsibilityCenterId],-- E.[AccountIdentifier], E.[ResourceIdentifier],
			E.[EntryTypeId], --[BatchCode], 
			E.[DueDate], E.[MonetaryValue], E.[Quantity], E.[UnitId], E.[Value],
			E.[Time1], E.[Time2],
			E.[ExternalReference],
			E.[AdditionalReference],
			E.[NotedAgentId], 
			E.[NotedAgentName], 
			E.[NotedAmount], 
			E.[NotedDate]
		FROM @Entries E
		JOIN @DocumentsIndexedIds DI ON E.[DocumentIndex] = DI.[Index]
		JOIN @LinesIndexedIds LI ON E.[LineIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED THEN
		UPDATE SET
			t.[Direction]				= s.[Direction],	
			t.[AccountId]				= s.[AccountId],
			t.[CurrencyId]				= s.[CurrencyId],
			t.[AgentId]					= s.[AgentId],
			t.[ResourceId]				= s.[ResourceId],
			t.[ResponsibilityCenterId]	= s.[ResponsibilityCenterId],
			--t.[AccountIdentifier]		= s.[AccountIdentifier],
			--t.[ResourceIdentifier]		= s.[ResourceIdentifier],
			t.[EntryTypeId]				= s.[EntryTypeId],
			t.[DueDate]					= s.[DueDate],
			t.[MonetaryValue]			= s.[MonetaryValue],
			t.[Quantity]				= s.[Quantity],
			t.[UnitId]					= s.[UnitId],
			t.[Value]					= s.[Value],
			t.[Time1]					= s.[Time1],
			t.[Time2]					= s.[Time2],	
			t.[ExternalReference]	= s.[ExternalReference],
			t.[AdditionalReference]	= s.[AdditionalReference],
			t.[NotedAgentId]		= s.[NotedAgentId],
			t.[NotedAgentName]		= s.[NotedAgentName],
			t.[NotedAmount]			= s.[NotedAmount],
			t.[NotedDate]			= s.[NotedDate],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineId], [Index], [Direction], [AccountId], [CurrencyId],
			[AgentId], [ResourceId], [ResponsibilityCenterId], --[AccountIdentifier], [ResourceIdentifier],
			[EntryTypeId], --[BatchCode], 
			[DueDate], [MonetaryValue], [Quantity], [UnitId], [Value],
			[Time1], [Time2],
			[ExternalReference],
			[AdditionalReference],
			[NotedAgentId], 
			[NotedAgentName], 
			[NotedAmount], 
			[NotedDate]
		)
		VALUES (s.[LineId], s.[Index], s.[Direction], s.[AccountId], s.[CurrencyId],
			s.[AgentId], s.[ResourceId], s.[ResponsibilityCenterId],-- s.[AccountIdentifier], s.[ResourceIdentifier],
			s.[EntryTypeId], --[BatchCode], 
			s.[DueDate], s.[MonetaryValue], s.[Quantity], s.[UnitId], s.[Value],
			s.[Time1], s.[Time2],
			s.[ExternalReference],
			s.[AdditionalReference],
			s.[NotedAgentId], 
			s.[NotedAgentName], 
			s.[NotedAmount], 
			s.[NotedDate]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BA AS (
		SELECT * FROM dbo.[Attachments]
		WHERE [DocumentId] IN (SELECT [Id] FROM @DocumentsIndexedIds)
	)
	INSERT INTO @DeletedFileIds([Id])
	SELECT x.[DeletedFileId]
	FROM
	(
		MERGE INTO BA AS t
		USING (
			SELECT
				A.[Id],
				DI.[Id] AS [DocumentId],
				A.[FileName],
				A.[FileExtension],
				A.[FileId],
				A.[Size]
			FROM @Attachments A
			JOIN @DocumentsIndexedIds DI ON A.[DocumentIndex] = DI.[Index]
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[FileName]			= s.[FileName],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([DocumentId], [FileName], [FileExtension], [FileId], [Size])
			VALUES (s.[DocumentId], s.[FileName], s.[FileExtension], s.[FileId], s.[Size])
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT INSERTED.[FileId] AS [InsertedFileId], DELETED.[FileId] AS [DeletedFileId]
	) AS x
	WHERE x.[InsertedFileId] IS NULL
		
	--SELECT @ReturnResult = (SELECT * FROM @DocumentsIndexedIds FOR JSON PATH);

	-- Return deleted File IDs, so C# can delete them from Blob Storage
	SELECT [Id] FROM @DeletedFileIds;
	
	-- Make sure Nonworkflow lines are stored with State = 4 (Finalized)
	UPDATE dbo.Lines
	SET [State] = 4
	WHERE [Id] IN (SELECT [Id] FROM @LinesIndexedIds)
	AND [DefinitionId] NOT IN (SELECT [LineDefinitionId] FROM dbo.Workflows)

	-- if we added/deleted draft lines, the document state should change
	DECLARE @DocIds dbo.IdList;
	INSERT INTO @DocIds([Id])
	SELECT [Id] FROM @DocumentsIndexedIds;
	EXEC dal.Documents_State__Refresh @DocIds;

	---- Assign the new ones to self
	DECLARE @NewDocumentsIds dbo.IdList;
	INSERT INTO @NewDocumentsIds([Id])
	SELECT Id FROM @DocumentsIndexedIds
	WHERE [Index] IN (SELECT [Index] FROM @Documents WHERE [Id] = 0);

	EXEC [dal].[Documents__Assign]
		@Ids = @NewDocumentsIds,
		@AssigneeId = @UserId,
		@Comment = N'FYC'

	-- Return the document Ids if requested
	IF (@ReturnIds = 1) 
		SELECT * FROM @DocumentsIndexedIds;
END;