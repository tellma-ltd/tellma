CREATE PROCEDURE [dal].[Accounts__Save]
	@Entities [AccountList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].Accounts AS t
		USING (
			SELECT
				[Index], [Id], [CustomClassificationId], [IfrsClassificationId],
				[Name], [Name2], [Name3], [Code], [PartyReference], [AgentId],
				[DefaultDebitIfrsEntryClassificationId], [DefaultCreditIfrsEntryClassificationId], 
				[ResponsibilityCenterId], [DefaultResourceId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[CustomClassificationId]			= s.[CustomClassificationId], 
				t.[IfrsAccountClassificationId]					= s.[IfrsClassificationId],
				t.[Name]							= s.[Name],
				t.[Name2]							= s.[Name2],
				t.[Name3]							= s.[Name3],
				t.[Code]							= s.[Code],
				t.[PartyReference]					= s.[PartyReference],
				t.[AgentId]							= s.[AgentId],
				t.[DefaultDebitIfrsEntryClassificationId]	= s.[DefaultDebitIfrsEntryClassificationId],
				t.[DefaultCreditIfrsEntryClassificationId]	= s.[DefaultCreditIfrsEntryClassificationId],
				t.[ResponsibilityCenterId]			= s.[ResponsibilityCenterId],
				t.[DefaultResourceId]				= s.[DefaultResourceId],
				t.[ModifiedAt]						= @Now,
				t.[ModifiedById]					= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([CustomClassificationId], [IfrsAccountClassificationId],
				[Name], [Name2], [Name3], [Code], [PartyReference], [AgentId],
				[DefaultDebitIfrsEntryClassificationId], [DefaultCreditIfrsEntryClassificationId], [ResponsibilityCenterId], [DefaultResourceId])
			VALUES (s.[CustomClassificationId], s.[IfrsClassificationId],
				s.[Name], s.[Name2], s.[Name3], s.[Code], s.[PartyReference], s.[AgentId],
				s.[DefaultDebitIfrsEntryClassificationId], s.[DefaultCreditIfrsEntryClassificationId], s.[ResponsibilityCenterId], s.[DefaultResourceId])
			OUTPUT s.[Index], inserted.[Id]
	) AS x
	OPTION (RECOMPILE);

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;