﻿CREATE PROCEDURE [dal].[Reconciliations__Save]
	@AccountId					INT, 
	@AgentId					INT,
	@ExternalEntries			[dbo].[ExternalEntryList] READONLY, -- insert/update
	@Reconciliations			[dbo].[ReconciliationList] READONLY, -- insert
	@ReconciliationEntries		[dbo].[ReconciliationEntryList] READONLY,--  <- insert
	@ReconciliationExternalEntries [dbo].[ReconciliationExternalEntryList] READONLY, -- <- insert
	@DeletedExternalEntryIds	[dbo].[IdList] READONLY,--  <- delete
	@DeletedReconcilationIds	[dbo].[IdList] READONLY, -- <- delete
	@UserId INT
AS
	DECLARE @RIndexedIds [dbo].[IndexedIdList];
	DECLARE @EEIndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- Insert Reconciliations
	INSERT INTO @RIndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Reconciliations] AS t
		USING (
			SELECT 	[Index], 0 AS [Id]
			FROM @Reconciliations 
		) AS s ON (t.[Id] = s.[Id])
		WHEN NOT MATCHED THEN
			INSERT ([CreatedAt], [CreatedById])
			Values (@Now, @UserId)
	OUTPUT s.[Index], inserted.[Id]
	) AS x;

	-- Insert/update External Entries
	INSERT INTO @EEIndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[ExternalEntries] AS t
		USING (
			SELECT 	[Index], [Id], [PostingDate], [Direction], [MonetaryValue], [ExternalReference]
			FROM @ExternalEntries 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED THEN
			UPDATE SET
				t.[AccountId]			= @AccountId,
				t.[AgentId]			= @AgentId,
				t.[PostingDate]			= s.[PostingDate],
				t.[Direction]			= s.[Direction],
				t.[MonetaryValue]		= s.[MonetaryValue],
				t.[ExternalReference]	= s.[ExternalReference],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([AccountId], [AgentId], [PostingDate], [Direction], [MonetaryValue], [ExternalReference], [CreatedById], [CreatedAt], [ModifiedById], [ModifiedAt])
			Values (@AccountId, @AgentId, s.[PostingDate], s.[Direction], s.[MonetaryValue], s.[ExternalReference], @UserId, @Now, @UserId, @Now)
	OUTPUT s.[Index], inserted.[Id]
	) AS x;

	-- Insert internal side of the reconciliation
	INSERT INTO [dbo].[ReconciliationEntries]([ReconciliationId], [EntryId])
	SELECT II.[Id], RE.[EntryId]
	FROM @ReconciliationEntries RE
	JOIN @RIndexedIds II ON RE.[HeaderIndex] = II.[Index];

	-- Insert external side of the reconciliation
	INSERT INTO [dbo].[ReconciliationExternalEntries]([ReconciliationId], [ExternalEntryId])
	SELECT RII.[Id], COALESCE(EEII.[Id], REE.[ExternalEntryId])
	FROM @ReconciliationExternalEntries REE
	JOIN @RIndexedIds RII ON REE.[HeaderIndex] = RII.[Index]
	LEFT JOIN @EEIndexedIds EEII ON REE.[ExternalEntryIndex] = EEII.[Index]

	-- Delete Reconciliations
	DELETE FROM [dbo].[Reconciliations] WHERE Id IN (SELECT [Id] FROM @DeletedReconcilationIds)

	-- Delete External Entries
	DELETE FROM [dbo].[Reconciliations] WHERE Id IN (
		SELECT [ReconciliationId]
		FROM [dbo].[ReconciliationExternalEntries]
		WHERE [ExternalEntryId] IN (SELECT [Id] FROM @DeletedExternalEntryIds)
	)
	DELETE FROM [dbo].[ExternalEntries] WHERE [Id] IN (SELECT [Id] FROM @DeletedExternalEntryIds)