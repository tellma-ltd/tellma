CREATE PROCEDURE [dal].[Documents__SaveAndRefresh]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].[EntryList] READONLY,
	@Attachments [dbo].[AttachmentList] READONLY,
	@ReturnIds BIT = 0
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
				[SerialNumber] As ManualSerialNumber,
				[PostingDate],
				[PostingDateIsCommon],
				[Clearance],
				[Memo], -- [Frequency], [Repetitions],
				[MemoIsCommon],
				[SegmentId],
				[CenterId],
				[CenterIsCommon],
				[ParticipantId],
				[ParticipantIsCommon],
				[CurrencyId],
				[CurrencyIsCommon],
				[ExternalReference],
				[ExternalReferenceIsCommon],
				[AdditionalReference],
				[AdditionalReferenceIsCommon],
				ROW_NUMBER() OVER (PARTITION BY [Id] ORDER BY [Index]) + (
					-- max(SerialNumber) per document type.
					SELECT ISNULL(MAX([SerialNumber]), 0) FROM dbo.Documents WHERE [DefinitionId] = @DefinitionId
				) As [AutoSerialNumber]
			FROM @Documents D
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[SerialNumber]				= IIF(@IsOriginalDocument = 1, 
													t.[SerialNumber],
													s.[ManualSerialNumber]),
				t.[PostingDate]					= s.[PostingDate],
				t.[PostingDateIsCommon]			= s.[PostingDateIsCommon],
				t.[Clearance]					= s.[Clearance],
				t.[Memo]						= s.[Memo],
				t.[MemoIsCommon]				= s.[MemoIsCommon],
				t.[SegmentId]					= s.[SegmentId],
				t.[CenterId]					= s.[CenterId],
				t.[CenterIsCommon]				= s.[CenterIsCommon],
				t.[ParticipantId]				= s.[ParticipantId],
				t.[ParticipantIsCommon]			= s.[ParticipantIsCommon],
				t.[CurrencyId]					= s.[CurrencyId],
				t.[CurrencyIsCommon]			= s.[CurrencyIsCommon],
				t.[ExternalReference]			= s.[ExternalReference],
				t.[ExternalReferenceIsCommon]	= s.[ExternalReferenceIsCommon],
				t.[AdditionalReference]			= s.[AdditionalReference],
				t.[AdditionalReferenceIsCommon]	= s.[AdditionalReferenceIsCommon],
				t.[ModifiedAt]					= @Now,
				t.[ModifiedById]				= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DefinitionId],
				[SerialNumber], 
				[PostingDate],
				[PostingDateIsCommon],
				[Clearance],
				[Memo],
				[MemoIsCommon],
				[SegmentId],
				[CenterId],
				[CenterIsCommon],
				[ParticipantId],
				[ParticipantIsCommon],
				[CurrencyId],
				[CurrencyIsCommon],
				[ExternalReference],
				[ExternalReferenceIsCommon],
				[AdditionalReference],
				[AdditionalReferenceIsCommon]
			)
			VALUES (
				@DefinitionId,
				IIF(@IsOriginalDocument = 1, s.[AutoSerialNumber], s.[ManualSerialNumber]),
				s.[PostingDate],
				s.[PostingDateIsCommon],
				s.[Clearance],
				s.[Memo],
				s.[MemoIsCommon],
				s.[SegmentId],
				s.[CenterId],
				s.[CenterIsCommon],
				s.[ParticipantId],
				s.[ParticipantIsCommon],		
				s.[CurrencyId],
				s.[CurrencyIsCommon],
				s.[ExternalReference],
				s.[ExternalReferenceIsCommon],
				s.[AdditionalReference],
				s.[AdditionalReferenceIsCommon]
			)
		OUTPUT s.[Index], inserted.[Id] 
	) As x;

	WITH BDLDE AS (
		SELECT * FROM dbo.[DocumentLineDefinitionEntries]
		WHERE DocumentId IN (SELECT [Id] FROM @DocumentsIndexedIds)
	)
	MERGE INTO BDLDE AS t
	USING (
		SELECT
			LDE.[Id],
			DI.Id AS DocumentId,
			LDE.[LineDefinitionId],

			LDE.[EntryIndex],
			LDE.[PostingDate], 
			LDE.[PostingDateIsCommon],
			LDE.[Memo],
			LDE.[MemoIsCommon],
	
			LDE.[ParticipantId],
			LDE.[ParticipantIsCommon],

			LDE.[CenterId],
			LDE.[CenterIsCommon],
			LDE.[CurrencyId],
			LDE.[CurrencyIsCommon],

			LDE.[CustodyId],
			LDE.[CustodyIsCommon],
			LDE.[ResourceId],
			LDE.[ResourceIsCommon],
			LDE.[Quantity],
			LDE.[QuantityIsCommon],
			LDE.[UnitId],
			LDE.[UnitIsCommon],

			LDE.[Time1],
			LDE.[Time1IsCommon],
			LDE.[Time2],
			LDE.[Time2IsCommon],

			LDE.[ExternalReference],
			LDE.[ExternalReferenceIsCommon],

			LDE.[AdditionalReference],
			LDE.[AdditionalReferenceIsCommon]
		FROM @DocumentLineDefinitionEntries LDE
		JOIN @DocumentsIndexedIds DI ON LDE.[DocumentIndex] = DI.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED THEN
		UPDATE SET
			t.[DocumentId]					= s.[DocumentId],
			t.[LineDefinitionId]			= s.[LineDefinitionId],

			t.[EntryIndex]					= s.[EntryIndex],
			t.[PostingDate]					= s.[PostingDate], 
			t.[PostingDateIsCommon]			= s.[PostingDateIsCommon],
			t.[Memo]						= s.[Memo],
			t.[MemoIsCommon]				= s.[MemoIsCommon],
	
			t.[ParticipantId]				= s.[ParticipantId],
			t.[ParticipantIsCommon]			= s.[ParticipantIsCommon],

			t.[CurrencyId]					= s.[CurrencyId],
			t.[CurrencyIsCommon]			= s.[CurrencyIsCommon],

			t.[CustodyId]					= s.[CustodyId],
			t.[CustodyIsCommon]				= s.[CustodyIsCommon],
			t.[ResourceId]					= s.[ResourceId],
			t.[ResourceIsCommon]			= s.[ResourceIsCommon],
			t.[Quantity]					= s.[Quantity],
			t.[QuantityIsCommon]			= s.[QuantityIsCommon],
			t.[UnitId]						= s.[UnitId],
			t.[UnitIsCommon]				= s.[UnitIsCommon],

			t.[CenterId]					= s.[CenterId],
			t.[CenterIsCommon]				= s.[CenterIsCommon],

			t.[Time1]						= s.[Time1],
			t.[Time1IsCommon]				= s.[Time1IsCommon],
			t.[Time2]						= s.[Time2],
			t.[Time2IsCommon]				= s.[Time2IsCommon],

			t.[ExternalReference]			= s.[ExternalReference],
			t.[ExternalReferenceIsCommon]	= s.[ExternalReferenceIsCommon],

			t.[AdditionalReference]			= s.[AdditionalReference],
			t.[AdditionalReferenceIsCommon] = s.[AdditionalReferenceIsCommon],
			t.[ModifiedAt]					= @Now,
			t.[ModifiedById]				= @UserId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (
			[DocumentId],
			[LineDefinitionId],

			[EntryIndex],
			[PostingDate], 
			[PostingDateIsCommon],
			[Memo],
			[MemoIsCommon],
	
			[ParticipantId],
			[ParticipantIsCommon],

			[CurrencyId],
			[CurrencyIsCommon],

			[CustodyId],
			[CustodyIsCommon],
			[ResourceId],
			[ResourceIsCommon],
			[Quantity],
			[QuantityIsCommon],
			[UnitId],
			[UnitIsCommon],

			[CenterId],
			[CenterIsCommon],

			[Time1],
			[Time1IsCommon],
			[Time2],
			[Time2IsCommon],

			[ExternalReference],
			[ExternalReferenceIsCommon],

			[AdditionalReference],
			[AdditionalReferenceIsCommon]
		)
		VALUES (
			s.[DocumentId],
			s.[LineDefinitionId],

			s.[EntryIndex],
			s.[PostingDate], 
			s.[PostingDateIsCommon],
			s.[Memo],
			s.[MemoIsCommon],
	
			s.[ParticipantId],
			s.[ParticipantIsCommon],

			s.[CurrencyId],
			s.[CurrencyIsCommon],

			s.[CustodyId],
			s.[CustodyIsCommon],
			s.[ResourceId],
			s.[ResourceIsCommon],
			s.[Quantity],
			s.[QuantityIsCommon],
			s.[UnitId],
			s.[UnitIsCommon],

			s.[CenterId],
			s.[CenterIsCommon],

			s.[Time1],
			s.[Time1IsCommon],
			s.[Time2],
			s.[Time2IsCommon],

			s.[ExternalReference],
			s.[ExternalReferenceIsCommon],

			s.[AdditionalReference],
			s.[AdditionalReferenceIsCommon]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

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
				L.[Id],
				DI.Id AS DocumentId,
				L.[DefinitionId],
				L.[Index],
				L.[PostingDate],
				L.[TemplateLineId],
				L.[Multiplier],
				L.[Memo]
			FROM @Lines L
			JOIN @DocumentsIndexedIds DI ON L.[DocumentIndex] = DI.[Index]
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[DefinitionId]		= s.[DefinitionId],
				t.[Index]				= s.[Index],
				t.[PostingDate]			= s.[PostingDate],
				t.[TemplateLineId]		= s.[TemplateLineId],
				t.[Multiplier]			= s.[Multiplier],
				t.[Memo]				= s.[Memo],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED BY TARGET THEN
			INSERT ([DocumentId],	[DefinitionId], [Index],	[PostingDate],		[TemplateLineId],	[Multiplier], [Memo])
			VALUES (s.[DocumentId], s.[DefinitionId], s.[Index], s.[PostingDate], s.[TemplateLineId], s.[Multiplier], s.[Memo])
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
			E.[Id], LI.Id AS [LineId], E.[Index], E.[IsSystem], E.[Direction], E.[AccountId],  E.[CurrencyId],
			E.[CustodianId], E.[CustodyId], E.[ParticipantId], E.[ResourceId], E.[CenterId],
			E.[EntryTypeId],
			E.[MonetaryValue], E.[Quantity], E.[UnitId], E.[Value],
			E.[Time1], E.[Time2],
			E.[ExternalReference],
			E.[AdditionalReference],
			E.[NotedAgentName], 
			E.[NotedAmount], 
			E.[NotedDate]
		FROM @Entries E
		JOIN @DocumentsIndexedIds DI ON E.[DocumentIndex] = DI.[Index]
		JOIN @LinesIndexedIds LI ON E.[LineIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED THEN
		UPDATE SET
			t.[Index]					= s.[Index],
			t.[IsSystem]				= s.[IsSystem], 
			t.[Direction]				= s.[Direction],	
			t.[AccountId]				= s.[AccountId],
			t.[CurrencyId]				= s.[CurrencyId],
			t.[CustodianId]				= s.[CustodianId],
			t.[CustodyId]				= s.[CustodyId],
			t.[ParticipantId]			= s.[ParticipantId],
			t.[ResourceId]				= s.[ResourceId],
			t.[CenterId]				= s.[CenterId],
			t.[EntryTypeId]				= s.[EntryTypeId],
			t.[MonetaryValue]			= s.[MonetaryValue],
			t.[Quantity]				= s.[Quantity],
			t.[UnitId]					= s.[UnitId],
			t.[Value]					= s.[Value],
			t.[Time1]					= s.[Time1],
			t.[Time2]					= s.[Time2],	
			t.[ExternalReference]		= s.[ExternalReference],
			t.[AdditionalReference]		= s.[AdditionalReference],
			t.[NotedAgentName]			= s.[NotedAgentName],
			t.[NotedAmount]				= s.[NotedAmount],
			t.[NotedDate]				= s.[NotedDate],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineId], [Index], [IsSystem], [Direction], [AccountId], [CurrencyId],
			[CustodianId], [CustodyId], [ParticipantId], [ResourceId], [CenterId],
			[EntryTypeId],
			[MonetaryValue], [Quantity], [UnitId], [Value],
			[Time1], [Time2],
			[ExternalReference],
			[AdditionalReference],
			[NotedAgentName], 
			[NotedAmount], 
			[NotedDate]
		)
		VALUES (s.[LineId], s.[Index], s.[IsSystem], s.[Direction], s.[AccountId], s.[CurrencyId],
			s.[CustodianId], s.[CustodyId], s.[ParticipantId], s.[ResourceId], s.[CenterId],
			s.[EntryTypeId],
			s.[MonetaryValue], s.[Quantity], s.[UnitId], s.[Value],
			s.[Time1], s.[Time2],
			s.[ExternalReference],
			s.[AdditionalReference],
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

	-- if we added/deleted draft lines, the document state should change

	--DECLARE @DocIds dbo.IdList;
	--INSERT INTO @DocIds([Id])
	--SELECT [Id] FROM @DocumentsIndexedIds;
	--EXEC dal.Documents_State__Refresh @DocIds;

	---- Assign the new ones to self
	DECLARE @NewDocumentsIds dbo.IdList;
	INSERT INTO @NewDocumentsIds([Id])
	SELECT Id FROM @DocumentsIndexedIds
	WHERE [Index] IN (SELECT [Index] FROM @Documents WHERE [Id] = 0);

	-- This automatically returns the new notification counts
	EXEC [dal].[Documents__Assign]
		@Ids = @NewDocumentsIds,
		@AssigneeId = @UserId --,
		-- @Comment = N'FYC' -- Not necessary, also doesn't work in other languages

	-- Return deleted File IDs, so C# can delete them from Blob Storage
	SELECT [Id] FROM @DeletedFileIds;

	-- Return the document Ids if requested
	IF (@ReturnIds = 1) 
		SELECT * FROM @DocumentsIndexedIds;
END;