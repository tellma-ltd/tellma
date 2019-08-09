CREATE PROCEDURE [dbo].[bll_Documents__Fill] -- UI logic to fill missing fields
	@Transactions [dbo].[DocumentList] READONLY, 
	@Lines [dbo].[DocumentWideLineList] READONLY, 
	@Entries [dbo].DocumentLineEntryList READONLY,
	@DocumentLineTypes [dbo].[DocumentLineTypeList] READONLY,
	@ResultJson NVARCHAR(MAX) OUTPUT
AS
DECLARE @DEBUG INT = CONVERT(INT, SESSION_CONTEXT(N'Debug'));
DECLARE @TransactionsLocal [dbo].[DocumentList], @LinesLocal [dbo].[LineList], @DocumentLineTypesLocal [dbo].[DocumentLineTypeList],
		@EntriesLocal [dbo].[EntryList], @SmartEntriesLocal [dbo].[EntryList], @Offset INT;
BEGIN -- fill missing data from the inserted/updated
	INSERT INTO @TransactionsLocal SELECT * FROM @Transactions;
	INSERT INTO @DocumentLineTypesLocal SELECT * FROM @DocumentLineTypes;
	INSERT INTO @LinesLocal SELECT * FROM @Lines;
	INSERT INTO @EntriesLocal SELECT * FROM @Entries;
END
BEGIN -- Inherit from defaults
	UPDATE D -- Set End Date Time
	SET [EndDateTime] = 
		[dbo].[fn_EndDateTime__Frequency_Duration_StartDateTime]([Frequency], [Duration], [StartDateTime])
	FROM @TransactionsLocal D

	Update E
	SET
		E.[OperationId] = CASE WHEN D.[OperationId] IS NOT NULL THEN D.[OperationId] ELSE E.[OperationId] END,
		E.[AccountId] = CASE WHEN D.[AccountId] IS NOT NULL THEN D.[AccountId] ELSE E.[AccountId] END,
		E.[AgentId] = CASE WHEN D.[AgentId] IS NOT NULL THEN D.[AgentId] ELSE E.[AgentId] END,
		E.[ResourceId] = CASE WHEN D.[ResourceId] IS NOT NULL THEN D.[ResourceId] ELSE E.[ResourceId] END,
		E.[Amount] = CASE WHEN D.[Amount] IS NOT NULL THEN D.[Amount] ELSE E.[Amount] END,
		E.[Value] = CASE WHEN D.[Value] IS NOT NULL THEN D.[Value] ELSE E.[Value] END,
		E.[NoteId] = CASE WHEN D.[NoteId] IS NOT NULL THEN D.[NoteId] ELSE E.[NoteId] END,
		E.[Reference] = CASE WHEN D.[Reference] IS NOT NULL THEN D.[Reference] ELSE E.[Reference] END,
		E.[RelatedReference] = CASE WHEN D.[RelatedReference] IS NOT NULL THEN D.[RelatedReference] ELSE E.[RelatedReference] END,
		E.[RelatedAgentId] = CASE WHEN D.[RelatedAgentId] IS NOT NULL THEN D.[RelatedAgentId] ELSE E.[RelatedAgentId] END,
		E.[RelatedResourceId] = CASE WHEN D.[RelatedResourceId] IS NOT NULL THEN D.[RelatedResourceId] ELSE E.[RelatedResourceId] END,
		E.[RelatedAmount] = CASE WHEN D.[RelatedAmount] IS NOT NULL THEN D.[RelatedAmount] ELSE E.[RelatedAmount] END
	FROM @EntriesLocal E
	JOIN @TransactionsLocal D ON E.[DocumentIndex] = D.[Index];

	UPDATE L -- Inherit lines data from tab headers data. Useful..
	SET 
		L.[BaseLineId]	= CASE WHEN DLT.[BaseLineId] IS NOT NULL THEN DLT.[BaseLineId] ELSE L.[BaseLineId] END,
		L.[ScalingFactor]= CASE WHEN DLT.[ScalingFactor] IS NOT NULL THEN DLT.[ScalingFactor] ELSE L.[ScalingFactor] END,
		L.[Memo] = CASE WHEN DLT.[Memo] IS NOT NULL THEN DLT.[Memo] ELSE L.[Memo] END,

		L.[OperationId1] = COALESCE(D.[OperationId], DLT.[OperationId1], L.[OperationId1]),
		L.[AccountId1] = COALESCE(D.[AccountId], DLT.[AccountId1], L.[AccountId1]),
		L.[AgentId1] = COALESCE(D.[AgentId], DLT.[AgentId1], L.[AgentId1]),
		L.[ResourceId1] = COALESCE(D.[ResourceId], DLT.[ResourceId1], L.[ResourceId1]),
		L.[Amount1] = COALESCE(D.[Amount], DLT.[Amount1], L.[Amount1]),
		L.[Value1] = COALESCE(D.[Value], DLT.[Value1], L.[Value1]),
		L.[NoteId1] = COALESCE(D.[NoteId], DLT.[NoteId1], L.[NoteId1]),
		L.[Reference1] = COALESCE(D.[Reference], DLT.[Reference1], L.[Reference1]),
		L.[RelatedReference1] = COALESCE(D.[RelatedReference], DLT.[RelatedReference1], L.[RelatedReference1]),
		L.[RelatedAgentId1] = COALESCE(D.[RelatedAgentId], DLT.[RelatedAgentId1], L.[RelatedAgentId1]),
		L.[RelatedResourceId1] = COALESCE(D.[RelatedResourceId], DLT.[RelatedResourceId1], L.[RelatedResourceId1]),
		L.[RelatedAmount1] = COALESCE(D.[RelatedAmount], DLT.[RelatedAmount1], L.[RelatedAmount1]),

		L.[OperationId2] = COALESCE(D.[OperationId], DLT.[OperationId2], L.[OperationId2]),
		L.[AccountId2] = COALESCE(D.[AccountId], DLT.[AccountId2], L.[AccountId2]),
		L.[AgentId2] = COALESCE(D.[AgentId], DLT.[AgentId2], L.[AgentId2]),
		L.[ResourceId2] = COALESCE(D.[ResourceId], DLT.[ResourceId2], L.[ResourceId2]),
		L.[Amount2] = COALESCE(D.[Amount], DLT.[Amount2], L.[Amount2]),
		L.[Value2] = COALESCE(D.[Value], DLT.[Value2], L.[Value2]),
		L.[NoteId2] = COALESCE(D.[NoteId], DLT.[NoteId2], L.[NoteId2]),
		L.[Reference2] = COALESCE(D.[Reference], DLT.[Reference2], L.[Reference2]),
		L.[RelatedReference2] = COALESCE(D.[RelatedReference], DLT.[RelatedReference2], L.[RelatedReference2]),
		L.[RelatedAgentId2] = COALESCE(D.[RelatedAgentId], DLT.[RelatedAgentId2], L.[RelatedAgentId2]),
		L.[RelatedResourceId2] = COALESCE(D.[RelatedResourceId], DLT.[RelatedResourceId2], L.[RelatedResourceId2]),
		L.[RelatedAmount2] = COALESCE(D.[RelatedAmount], DLT.[RelatedAmount2], L.[RelatedAmount2]),

		L.[OperationId3] = COALESCE(D.[OperationId], DLT.[OperationId3], L.[OperationId3]),
		L.[AccountId3] = COALESCE(D.[AccountId], DLT.[AccountId3], L.[AccountId3]),
		L.[AgentId3] = COALESCE(D.[AgentId], DLT.[AgentId3], L.[AgentId3]),
		L.[ResourceId3] = COALESCE(D.[ResourceId], DLT.[ResourceId3], L.[ResourceId3]),
		L.[Amount3] = COALESCE(D.[Amount], DLT.[Amount3], L.[Amount3]),
		L.[Value3] = COALESCE(D.[Value], DLT.[Value3], L.[Value3]),
		L.[NoteId3] = COALESCE(D.[NoteId], DLT.[NoteId3], L.[NoteId3]),
		L.[Reference3] = COALESCE(D.[Reference], DLT.[Reference3], L.[Reference3]),
		L.[RelatedReference3] = COALESCE(D.[RelatedReference], DLT.[RelatedReference3], L.[RelatedReference3]),
		L.[RelatedAgentId3] = COALESCE(D.[RelatedAgentId], DLT.[RelatedAgentId3], L.[RelatedAgentId3]),
		L.[RelatedResourceId3] = COALESCE(D.[RelatedResourceId], DLT.[RelatedResourceId3], L.[RelatedResourceId3]),
		L.[RelatedAmount3] = COALESCE(D.[RelatedAmount], DLT.[RelatedAmount3], L.[RelatedAmount3]),
		
		L.[OperationId4] = COALESCE(D.[OperationId], DLT.[OperationId4], L.[OperationId4]),
		L.[AccountId4] = COALESCE(D.[AccountId], DLT.[AccountId4], L.[AccountId4]),
		L.[AgentId4] = COALESCE(D.[AgentId], DLT.[AgentId4], L.[AgentId4]),
		L.[ResourceId4] = COALESCE(D.[ResourceId], DLT.[ResourceId4], L.[ResourceId4]),
		L.[Amount4] = COALESCE(D.[Amount], DLT.[Amount4], L.[Amount4]),
		L.[Value4] = COALESCE(D.[Value], DLT.[Value4], L.[Value4]),
		L.[NoteId4] = COALESCE(D.[NoteId], DLT.[NoteId4], L.[NoteId4]),
		L.[Reference4] = COALESCE(D.[Reference], DLT.[Reference4], L.[Reference4]),
		L.[RelatedReference4] = COALESCE(D.[RelatedReference], DLT.[RelatedReference4], L.[RelatedReference4]),
		L.[RelatedAgentId4] = COALESCE(D.[RelatedAgentId], DLT.[RelatedAgentId4], L.[RelatedAgentId4]),
		L.[RelatedResourceId4] = COALESCE(D.[RelatedResourceId], DLT.[RelatedResourceId4], L.[RelatedResourceId4]),
		L.[RelatedAmount4] = COALESCE(D.[RelatedAmount], DLT.[RelatedAmount4], L.[RelatedAmount4])
	FROM @LinesLocal L
	JOIN @DocumentLineTypesLocal DLT ON L.DocumentIndex = DLT.[DocumentIndex] AND L.[LineType] = DLT.[LineType]
	JOIN @TransactionsLocal D ON D.[Index] = L.[DocumentIndex]
END

BEGIN -- Fill lines from specifications
	-- copy the directions as is...
	UPDATE L
	SET
		L.Direction1 = LTS.Direction1,
		L.Direction2 = LTS.Direction2,
		L.Direction3 = LTS.Direction3,
		L.Direction4 = LTS.Direction4
	FROM @LinesLocal L
	JOIN dbo.[LineTypesSpecifications] LTS ON L.LineType = LTS.LineType;

	DECLARE @Sql NVARCHAR(4000), @ParmDefinition NVARCHAR (255), @AppendSql NVARCHAR(4000), @LineType NVARCHAR (255);
	SELECT @LineType = MIN(LineType) FROM @DocumentLineTypes;
	WHILE @LineType IS NOT NULL
	BEGIN
		SET @Sql = N'
			DECLARE @LinesLocal [dbo].[LineList];
			INSERT INTO @LinesLocal SELECT * FROM @Lines;
			UPDATE L
			SET 
			' +	ISNULL('L.OperationId1 = ' + (SELECT [OperationId1FillSql] FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.AccountId1 = ' +	(SELECT [AccountId1FillSql] FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.AgentId1 = ' +	(SELECT [AgentId1FillSql] FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.ResourceId1 = ' +	(SELECT ResourceId1FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.Amount1 = ' +	(SELECT Amount1FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.Value1 = ' +	(SELECT Value1FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.NoteId1 = ' +	(SELECT NoteId1FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.Reference1 = ' +	(SELECT Reference1FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.OperationId2 = ' +	(SELECT OperationId2FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.AccountId2 = ' +	(SELECT AccountId2FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.AgentId2 = ' +	(SELECT [AgentId2FillSql] FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.ResourceId2 = ' +	(SELECT ResourceId2FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.Amount2 = ' +	(SELECT Amount2FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.Value2 = ' +	(SELECT Value2FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.NoteId2 = ' +	(SELECT NoteId2FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + ISNULL('L.Reference2 = ' +	(SELECT Reference2FillSql FROM [LineTypesSpecifications] WHERE [Id] = @LineType) + ',
			', '') + 'L.[Index] = L.[Index]
			FROM @LinesLocal L
			JOIN @TransactionsLocal D ON D.[Index] = L.[DocumentIndex]
			JOIN @DocumentLineTypesLocal DLT ON D.[Index] = DLT.[DocumentIndex] AND L.[LineType] = DLT.[LineType];
			SELECT * FROM @LinesLocal;
		';
		SELECT @AppendSql = AppendSql FROM dbo.[LineTypesSpecifications] WHERE [Id] = @LineType;

		IF @DEBUG = 1
		BEGIN
			PRINT @Sql;
			Print @AppendSql;
		END;

		DECLARE @LinesInput LineList;
		SET @ParmDefinition = N'@TransactionsLocal dbo.DocumentList READONLY, @DocumentLineTypesLocal dbo.DocumentLineTypeList READONLY, @Lines dbo.LineList READONLY';		

		DELETE FROM @LinesInput; INSERT INTO @LinesInput SELECT * FROM @LinesLocal WHERE LineType = @LineType;
				
		DELETE FROM @LinesLocal WHERE LineType = @LineType;
		INSERT INTO @LinesLocal -- would be nice if we can use merge instead.
			EXEC sp_executesql @Sql, @ParmDefinition,
				@TransactionsLocal = @TransactionsLocal, @DocumentLineTypesLocal = @DocumentLineTypesLocal, @Lines = @LinesInput;

		EXEC sp_executeSql @AppendSql, @ParmDefinition,
			@TransactionsLocal = @TransactionsLocal, @DocumentLineTypesLocal = @DocumentLineTypesLocal, @Lines = @LinesLocal;

		SELECT @LineType = MIN(LineType) FROM @DocumentLineTypes WHERE LineType > @LineType;
	END
END
BEGIN	-- Smart Posting
	INSERT @SmartEntriesLocal([Index],	[DocumentIndex], [Id], [DocumentId], [LineType],	
		[Direction], [AccountId], [ResponsibilityCenterId], [NoteId], [AgentAccountId], [ResourceId],
		[MoneyAmount], [Mass], [Volume], [Count], [Time], [Value], [ExpectedClosingDate], [Reference], [Memo], [RelatedReference],
		[RelatedResourceId], [RelatedAgentAccountId], [RelatedMoneyAmount], [RelatedMass],
		[RelatedVolume], [RelatedCount], [RelatedTime], [RelatedValue], [EntityState]
	) -- assuming a line will not capture more than 100 entries (currently it only captures 4)
	SELECT 100 + [Index],	[DocumentIndex], [Id], [DocumentId], [LineType],
		[Direction1], [AccountId1], [ResponsibilityCenterId1], [NoteId1], [AgentAccountId1], [ResourceId1],
		[MoneyAmount1], [Mass1], [Volume1], [Count1], [Time1], [Value1], [ExpectedClosingDate1], [Reference1], [Memo1], [RelatedReference1],
		[RelatedResourceId1], [RelatedAgentAccountId1], [RelatedMoneyAmount1], [RelatedMass1],
		[RelatedVolume1], [RelatedCount1], [RelatedTime1], [RelatedValue1], [EntityState]
	FROM @LinesLocal WHERE [Direction1] IS NOT NULL
	UNION
	SELECT 200 + [Index],	[DocumentIndex], [Id], [DocumentId], [LineType],
		[Direction2], [AccountId2], [ResponsibilityCenterId2], [NoteId2], [AgentAccountId2], [ResourceId2],
		[MoneyAmount2], [Mass2], [Volume2], [Count2], [Time2], [Value2], [ExpectedClosingDate2], [Reference2], [Memo2], [RelatedReference2],
		[RelatedResourceId2], [RelatedAgentAccountId2], [RelatedMoneyAmount2], [RelatedMass2],
		[RelatedVolume2], [RelatedCount2], [RelatedTime2], [RelatedValue2], [EntityState]
	FROM @LinesLocal WHERE [Direction2] IS NOT NULL
	UNION
	SELECT 300 + [Index],	[DocumentIndex], [Id], [DocumentId], [LineType],
		[Direction3], [AccountId3], [ResponsibilityCenterId3], [NoteId3], [AgentAccountId3], [ResourceId3],
		[MoneyAmount3], [Mass3], [Volume3], [Count3], [Time3], [Value3], [ExpectedClosingDate3], [Reference3], [Memo3], [RelatedReference3],
		[RelatedResourceId3], [RelatedAgentAccountId3], [RelatedMoneyAmount3], [RelatedMass3],
		[RelatedVolume3], [RelatedCount3], [RelatedTime3], [RelatedValue3], [EntityState]
	FROM @LinesLocal WHERE [Direction3] IS NOT NULL
	UNION
	SELECT 400 + [Index],	[DocumentIndex], [Id], [DocumentId], [LineType],
		[Direction4], [AccountId4], [ResponsibilityCenterId4], [NoteId4], [AgentAccountId4], [ResourceId4],
		[MoneyAmount4], [Mass4], [Volume4], [Count4], [Time4], [Value4], [ExpectedClosingDate4], [Reference4], [Memo4], [RelatedReference4],
		[RelatedResourceId4], [RelatedAgentAccountId4], [RelatedMoneyAmount4], [RelatedMass4],
		[RelatedVolume4], [RelatedCount4], [RelatedTime4], [RelatedValue4], [EntityState]
	FROM @LinesLocal WHERE [Direction4] IS NOT NULL;
	
--	SELECT * FROM @SmartEntriesLocal;
	UPDATE @SmartEntriesLocal SET [Index] = [Index] + (SELECT ISNULL(MAX([Index]), 0) FROM @EntriesLocal);
	IF @DEBUG = 2 SELECT * FROM @SmartEntriesLocal;
	INSERT INTO @EntriesLocal([Index], [DocumentIndex], [Id], [DocumentId], [LineType],
		[Direction], [AccountId], [ResponsibilityCenterId], [NoteId], [AgentAccountId], [ResourceId],
		[MoneyAmount], [Mass], [Volume], [Count], [Time], [Value], [ExpectedClosingDate], [Reference], [Memo], [RelatedReference],
		[RelatedResourceId], [RelatedAgentAccountId], [RelatedMoneyAmount], [RelatedMass],
		[RelatedVolume], [RelatedCount], [RelatedTime], [RelatedValue])
		-- I used the sort key in order to make the entries grouped together in the same order as the DLT.
	SELECT ROW_NUMBER() OVER(ORDER BY S.[DocumentIndex], DLT.[SortKey], S.[Direction] DESC), S.[DocumentIndex], S.[Id], S.[DocumentId], S.[LineType],
		S.[Direction], S.[AccountId], S.[ResponsibilityCenterId], S.[NoteId], S.[AgentAccountId], S.[ResourceId],
		SUM(S.[MoneyAmount]), SUM(S.[Mass]), SUM(S.[Volume]), SUM(S.[Count]), SUM(S.[Time]), SUM(S.[Value]), 
		S.[ExpectedClosingDate], S.[Reference], S.[Memo], S.[RelatedReference],
		S.[RelatedResourceId], S.[RelatedAgentAccountId], S.[RelatedMoneyAmount], S.[RelatedMass],
		S.[RelatedVolume], S.[RelatedCount], S.[RelatedTime], S.[RelatedValue]
	FROM @SmartEntriesLocal S
	JOIN @DocumentLineTypesLocal DLT ON S.[DocumentIndex] = DLT.[DocumentIndex] AND S.[LineType] = DLT.[LineType]
	GROUP BY S.[DocumentIndex], S.[Id], S.[DocumentId], S.[LineType], 
		S.[Direction], S.[AccountId], S.[ResponsibilityCenterId], S.[NoteId], S.[AgentAccountId], S.[ResourceId],
		S.[ExpectedClosingDate], S.[Reference], S.[Memo], S.[RelatedReference],
		S.[RelatedResourceId], S.[RelatedAgentAccountId], S.[RelatedMoneyAmount], S.[RelatedMass],
		S.[RelatedVolume], S.[RelatedCount], S.[RelatedTime], S.[RelatedValue], DLT.[SortKey]
	HAVING(SUM(S.[MoneyAmount]) > 0 OR SUM(S.[Mass]) > 0 OR SUM(S.[Volume]) > 0 OR SUM(S.[Count]) > 0 OR SUM(S.[Time]) > 0 OR SUM(S.[Value]) > 0)
END

IF @DEBUG = 1
BEGIN
	select * from @TransactionsLocal;
	select * from @DocumentLineTypesLocal;
	select * from @LinesLocal;
	select * from @EntriesLocal;
END