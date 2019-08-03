CREATE PROCEDURE [dbo].[dal_Document__Save]
	@DocumentTypeId NVARCHAR(255),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[DocumentLineList] READONLY, 
	@Entries [dbo].DocumentLineEntryList READONLY
AS
BEGIN
	DECLARE @IndexedIds [dbo].[IndexedUuidList], @LinesIndexedIds [dbo].[IndexedUuidList];

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[Documents] AS t
	USING (
		SELECT 
			[Index], [Id], [DocumentDate], [VoucherNumericReference], [Memo], [Frequency], [Repetitions],
			ROW_Number() OVER (PARTITION BY [EntityState] ORDER BY [Index]) + (
				-- max(SerialNumber) per document type.
				SELECT ISNULL(MAX([SerialNumber]), 0) FROM dbo.Documents WHERE [DocumentTypeId] = @DocumentTypeId
			) As [SerialNumber]
		FROM @Documents 
		WHERE [EntityState] IN (N'Inserted', N'Updated')
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED
	THEN
		UPDATE SET
			t.[DocumentDate]			= s.[DocumentDate],
			t.[VoucherNumericReference]	= s.[VoucherNumericReference],
			t.[Memo]					= s.[Memo],
			t.[Frequency]				= s.[Frequency],
			t.[Repetitions]				= s.[Repetitions],

			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[DocumentDate], [VoucherNumericReference],[Memo],[Frequency],[Repetitions]
		)
		VALUES (
			s.[DocumentDate], s.[VoucherNumericReference], s.[Memo], s.[Frequency], s.[Repetitions]
		);
	
	-- Assign the new ones to self
	INSERT INTO dbo.DocumentAssignments(DocumentId, AssigneeId)
	SELECT Id, @UserId
	FROM @Documents

	MERGE INTO [dbo].[DocumentLines] AS t
	USING (
		SELECT L.Id, L.DocumentId, [TemplateLineId], [ScalingFactor]
		FROM @Lines L
		WHERE L.[EntityState] IN (N'Inserted', N'Updated')
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[TemplateLineId]	= s.[TemplateLineId], 
			t.[ScalingFactor]	= s.[ScalingFactor],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([DocumentId], [TemplateLineId], [ScalingFactor], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
		VALUES (s.[DocumentId], s.[TemplateLineId], s.[ScalingFactor], @Now, @UserId, @Now, @UserId);

	MERGE INTO [dbo].[DocumentLineEntries] AS t
	USING (
		SELECT
			[Id], [DocumentLineId], [Direction], [AccountId], [IfrsNoteId], [ResponsibilityCenterId],
				[ResourceId], [InstanceId], [BatchCode], [DueDate], [Quantity],
				[MoneyAmount], [Mass], [Volume], [Area], [Length], [Time], [Count], [Value], [Memo],
				[ExternalReference], [AdditionalReference], 
				[RelatedResourceId], [RelatedAgentId], [RelatedMoneyAmount]
		FROM @Entries
		WHERE [EntityState] IN (N'Inserted', N'Updated')
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET
			t.[Direction]				= s.[Direction],	
			t.[AccountId]				= s.[AccountId],
			t.[IfrsNoteId]				= s.[IfrsNoteId],
			t.[ResponsibilityCenterId]	= s.[ResponsibilityCenterId],
			t.[ResourceId]				= s.[ResourceId],
			t.[InstanceId]				= s.[InstanceId],
			t.[BatchCode]				= s.[BatchCode],
			t.[Quantity]				= s.[Quantity],
			t.[MoneyAmount]				= s.[MoneyAmount],
			t.[Mass]					= s.[Mass],
			t.[Volume]					= s.[Volume],
			t.[Area]					= s.[Area],
			t.[Length]					= s.[Length],
			t.[Time]					= s.[Time],
			t.[Count]					= s.[Count],
			t.[Value]					= s.[Value],
			t.[Memo]					= s.[Memo],
			t.[ExternalReference]		= s.[ExternalReference],
			t.[AdditionalReference]		= s.[AdditionalReference],
			t.[RelatedResourceId]		= s.[RelatedResourceId],
			t.[RelatedAccountId]		= s.[RelatedAgentId],
			t.[RelatedMoneyAmount]		= s.[RelatedMoneyAmount],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([DocumentLineId], [Direction], [AccountId], [IfrsNoteId], [ResponsibilityCenterId],
				[ResourceId], [InstanceId], [BatchCode], [Quantity],
				[MoneyAmount], [Mass], [Volume], [Area], [Length], [Time], [Count],  [Value], [Memo],
				[ExternalReference], [AdditionalReference], [RelatedResourceId], [RelatedAccountId], [RelatedMoneyAmount])
		VALUES (s.[DocumentLineId], s.[Direction], s.[AccountId], s.[IfrsNoteId], s.[ResponsibilityCenterId],
				s.[ResourceId], s.[InstanceId], s.[BatchCode], s.[Quantity],
				s.[MoneyAmount], s.[Mass], s.[Volume], s.[Area], s.[Length], s.[Time], s.[Count], s.[Value], s.[Memo],
				s.[ExternalReference], s.[AdditionalReference], s.[RelatedResourceId], s.[RelatedAgentId], s.[RelatedMoneyAmount])
		;
END;