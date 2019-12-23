CREATE PROCEDURE [dal].[Documents__Save]
	@DefinitionId NVARCHAR(255),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
	@ReturnIds BIT = 0
AS
BEGIN
	DECLARE @DocumentsIndexedIds [dbo].[IndexedIdList], @LinesIndexedIds [dbo].[IndexedIdList], @EntriesIndexedIds [dbo].[IndexedIdList];

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @DocumentsIndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Documents] AS t
		USING (
			SELECT 
				[Index], [Id], --[OperatingSegmentId], 
				[DocumentDate], [VoucherNumericReference], --[SortKey],
				[Memo],-- [Frequency], [Repetitions],
				ROW_Number() OVER (PARTITION BY [Id] ORDER BY [Index]) + (
					-- max(SerialNumber) per document type.
					SELECT ISNULL(MAX([SerialNumber]), 0) FROM dbo.Documents WHERE [DefinitionId] = @DefinitionId
				) As [SerialNumber]
			FROM @Documents D
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[DocumentDate]			= s.[DocumentDate],
				--t.[OperatingSegmentId]		= s.[OperatingSegmentId],
				t.[VoucherNumericReference]	= s.[VoucherNumericReference],
				t.[Memo]					= s.[Memo],
				--t.[Frequency]				= s.[Frequency],
				--t.[Repetitions]			= s.[Repetitions],

				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DefinitionId], [SerialNumber], --[OperatingSegmentId], 
				[DocumentDate], [VoucherNumericReference], --[SortKey],
				[Memo]--, [Frequency], [Repetitions]
			)
			VALUES (
				@DefinitionId, s.[SerialNumber], --s.[OperatingSegmentId], 
				s.[DocumentDate], s.[VoucherNumericReference], --s.[SerialNumber], 
				s.[Memo]--, s.[Frequency], s.[Repetitions]
			)
		OUTPUT s.[Index], inserted.[Id] 
	) As x;
	
	WITH BL AS (
		SELECT * FROM dbo.[Lines]
		WHERE DocumentId IN (SELECT [Id] FROM @DocumentsIndexedIds)
	)
	INSERT INTO @LinesIndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO BL AS t
		USING (
			SELECT
				L.[Index],
				L.[Id],
				DI.Id AS DocumentId,
				L.[DefinitionId], 
				L.[AgentId],
				L.[ResourceId],
				L.[Amount],
				L.[Memo],
				L.[ExternalReference],
				L.[AdditionalReference]
			FROM @Lines L
			JOIN @DocumentsIndexedIds DI ON L.[DocumentIndex] = DI.[Index]
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[DefinitionId]		= s.[DefinitionId],
				t.[AgentId]				= s.[AgentId],
				t.[ResourceId]			= s.[ResourceId],
				t.[Amount]				= s.[Amount],
				t.[Memo]				= s.[Memo],
				t.[ExternalReference]	= s.[ExternalReference],
				t.[AdditionalReference]	= s.[AdditionalReference],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([DocumentId], [DefinitionId], [SortKey],
				[AgentId],
				[ResourceId],
				[Amount],
				[Memo],
				[ExternalReference],
				[AdditionalReference]
			)
			VALUES (s.[DocumentId], s.[DefinitionId], s.[Index],
				s.[AgentId],
				s.[ResourceId],
				s.[Amount],
				s.[Memo],
				s.[ExternalReference],
				s.[AdditionalReference]
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id] 
	) AS x
	WHERE [Index] IS NOT NULL;

	WITH BE AS (
		SELECT * FROM dbo.[Entries]
		WHERE [LineId] IN (SELECT [Id] FROM @LinesIndexedIds)
	)
	MERGE INTO BE AS t
	USING (
		SELECT
			E.[Index], E.[Id], LI.Id AS [LineId], E.[EntryNumber], E.[Direction], E.[AccountId],
			E.[IsCurrent],
			E.[AgentId], E.[ResourceId], E.[ResponsibilityCenterId], E.[AccountIdentifier], E.[ResourceIdentifier],
			E.[CurrencyId], E.[EntryClassificationId], --[BatchCode], 
			E.[DueDate], E.[MonetaryValue], E.[Count], E.[Mass], E.[Volume], E.[Time], E.[Value],
			E.[ExternalReference], E.[AdditionalReference], E.[RelatedAgentId], E.[RelatedAgentName], E.[RelatedAmount],
			E.[RelatedDate], E.[Time1], E.[Time2]
		FROM @Entries E
		JOIN @DocumentsIndexedIds DI ON E.[DocumentIndex] = DI.[Index]
		JOIN @LinesIndexedIds LI ON E.[LineIndex] = LI.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED THEN
		UPDATE SET
			t.[Direction]				= s.[Direction],	
			t.[AccountId]				= s.[AccountId],
			t.[IsCurrent]				= s.[IsCurrent],
			t.[AgentId]					= s.[AgentId],
			t.[ResourceId]				= s.[ResourceId],
			t.[ResponsibilityCenterId]	= s.[ResponsibilityCenterId],
			t.[AccountIdentifier]		= s.[AccountIdentifier],
			t.[ResourceIdentifier]		= s.[ResourceIdentifier],
			t.[CurrencyId]				= s.[CurrencyId],
			t.[EntryClassificationId]	= s.[EntryClassificationId],
			t.[DueDate]					= s.[DueDate],
			t.[MonetaryValue]			= s.[MonetaryValue],
			t.[Count]					= s.[Count],
			t.[Mass]					= s.[Mass],
			t.[Volume]					= s.[Volume],
			t.[RelatedDate]				= s.[RelatedDate],
			t.[Time]					= s.[Time],
			t.[Value]					= s.[Value],
			t.[ExternalReference]		= s.[ExternalReference],
			t.[AdditionalReference]		= s.[AdditionalReference],
			t.[RelatedAgentId]			= s.[RelatedAgentId],
			t.[RelatedAgentName]		= s.[RelatedAgentName],
			t.[RelatedAmount]			= s.[RelatedAmount],
			t.[Time1]					= s.[Time1],
			t.[Time2]					= s.[Time2],	
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineId], [EntryNumber], [Direction], [AccountId],[IsCurrent],
			[AgentId], [ResourceId], [ResponsibilityCenterId], [AccountIdentifier], [ResourceIdentifier],
			[CurrencyId], [EntryClassificationId], --[BatchCode], 
			[DueDate], [MonetaryValue], [Count], [Mass], [Volume], [Time], [Value],
			[ExternalReference], [AdditionalReference], [RelatedAgentId], [RelatedAgentName], [RelatedAmount],
			[RelatedDate], [Time1], [Time2]
		)
		VALUES (s.[LineId], s.[EntryNumber], s.[Direction], s.[AccountId], s.[IsCurrent],
			s.[AgentId], s.[ResourceId], s.[ResponsibilityCenterId], s.[AccountIdentifier], s.[ResourceIdentifier],
			s.[CurrencyId], s.[EntryClassificationId], --[BatchCode], 
			s.[DueDate], s.[MonetaryValue], s.[Count], s.[Mass], s.[Volume], s.[Time], s.[Value],
			s.[ExternalReference], s.[AdditionalReference], s.[RelatedAgentId], s.[RelatedAgentName], s.[RelatedAmount],
			s.[RelatedDate], s.[Time1], s.[Time2]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF (@ReturnIds = 1)
		SELECT * FROM @DocumentsIndexedIds;
END;