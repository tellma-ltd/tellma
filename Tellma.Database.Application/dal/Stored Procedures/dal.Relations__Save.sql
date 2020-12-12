CREATE PROCEDURE [dal].[Relations__Save]
	@DefinitionId INT,
	@Entities dbo.[RelationList] READONLY,
	@RelationUsers dbo.[RelationUserList] READONLY,
	@Attachments [dbo].[RelationAttachmentList] READONLY,
	@ReturnIds BIT = 0
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList], @DeletedImageIds [dbo].[StringList], @DeletedAttachmentIds [dbo].[StringList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Entities whose ImageIds will be updated: capture their old ImageIds first (if any) so C# can delete them from blob storage
	INSERT INTO @DeletedImageIds ([Id])
	SELECT [ImageId] FROM dbo.[Relations] E
	WHERE E.[ImageId] IS NOT NULL 
		AND E.[Id] IN (SELECT [Id] FROM @Entities WHERE [ImageId] IS NULL OR [ImageId] <> N'(Unchanged)');

	-- Save the entities
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Relations] AS t
		USING (
			SELECT [Index], [Id],
				@DefinitionId AS [DefinitionId],
				[Name], 
				[Name2], 
				[Name3],
				[Code],
				[CurrencyId],
				[CenterId],
				[Description],
				[Description2],
				[Description3],
				geography::STGeomFromWKB([LocationWkb], 4326) AS [Location], -- 4326 = World Geodetic System, used by Google Maps
				[LocationJson],
				[FromDate],
				[ToDate],
				[DateOfBirth],
				[ContactEmail],
				[ContactMobile],
				[NormalizedContactMobile],
				[ContactAddress],
				[Date1],
				[Date2],
				[Date3],
				[Date4],
				[Decimal1],
				[Decimal2],
				[Int1],
				[Int2],
				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				[Lookup5Id],
				[Lookup6Id],
				[Lookup7Id],
				[Lookup8Id],
				[Text1],					
				[Text2],
				[Text3], 
				[Text4],
				[AgentId],
				[TaxIdentificationNumber],
				[JobId],
				[BankAccountNumber],
				[Relation1Id],
				[ImageId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED
		THEN
			UPDATE SET
				t.[DefinitionId]			= s.[DefinitionId],
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				
				t.[Code]					= s.[Code],
				t.[CurrencyId]				= s.[CurrencyId],
				t.[CenterId]				= s.[CenterId],
				t.[Description]				= s.[Description],
				t.[Description2]			= s.[Description2],
				t.[Description3]			= s.[Description3],
				t.[Location]				= s.[Location],
				t.[LocationJson]			= s.[LocationJson],
				t.[FromDate]				= s.[FromDate],
				t.[ToDate]					= s.[ToDate],
				t.[DateOfBirth]				= s.[DateOfBirth],
				t.[ContactEmail]			= s.[ContactEmail],
				t.[ContactMobile]			= s.[ContactMobile],
				t.[NormalizedContactMobile] = s.[NormalizedContactMobile],
				t.[ContactAddress]			= s.[ContactAddress],
				t.[Date1]					= s.[Date1],
				t.[Date2]					= s.[Date2],
				t.[Date3]					= s.[Date3],
				t.[Date4]					= s.[Date4],

				t.[Decimal1]				= s.[Decimal1],
				t.[Decimal2]				= s.[Decimal2],
				t.[Int1]					= s.[Int1],
				t.[Int2]					= s.[Int2],
				t.[Lookup1Id]				= s.[Lookup1Id],
				t.[Lookup2Id]				= s.[Lookup2Id],
				t.[Lookup3Id]				= s.[Lookup3Id],
				t.[Lookup4Id]				= s.[Lookup4Id],
				t.[Lookup5Id]				= s.[Lookup5Id],
				t.[Lookup6Id]				= s.[Lookup6Id],
				t.[Lookup7Id]				= s.[Lookup7Id],
				t.[Lookup8Id]				= s.[Lookup8Id],
				t.[Text1]					= s.[Text1],	
				t.[Text2]					= s.[Text2],
				t.[Text3]					= s.[Text3], 
				t.[Text4]					= s.[Text4],
				t.[AgentId]					= s.[AgentId],
				t.[TaxIdentificationNumber] = s.[TaxIdentificationNumber],
				t.[JobId]					= s.[JobId],
				t.[BankAccountNumber]		= s.[BankAccountNumber],
				t.[Relation1Id]				= s.[Relation1Id],

				t.[ImageId]					= IIF(s.[ImageId] = N'(Unchanged)', t.[ImageId], s.[ImageId]),

				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DefinitionId],
				[Name], 
				[Name2], 
				[Name3],
				[Code],
				[CurrencyId],
				[CenterId],
				[Description],
				[Description2],
				[Description3],
				[Location], -- 4326 = World Geodetic System, used by Google Maps
				[LocationJson],
				[FromDate],
				[ToDate],
				[DateOfBirth],
				[ContactEmail],
				[ContactMobile],
				[NormalizedContactMobile],
				[ContactAddress],
				[Date1],
				[Date2],
				[Date3],
				[Date4],
				[Decimal1],
				[Decimal2],
				[Int1],
				[Int2],
				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				[Lookup5Id],
				[Lookup6Id],
				[Lookup7Id],
				[Lookup8Id],
				[Text1],					
				[Text2],
				[Text3], 
				[Text4],
				[AgentId],
				[TaxIdentificationNumber],
				[JobId],
				[BankAccountNumber],
				[Relation1Id],
				[ImageId]
				)
			VALUES (
				s.[DefinitionId],
				s.[Name], 
				s.[Name2], 
				s.[Name3],
				s.[Code],
				s.[CurrencyId],
				s.[CenterId],
				s.[Description],
				s.[Description2],
				s.[Description3],
				s.[Location], -- 4326 = World Geodetic System, used by Google Maps
				s.[LocationJson],
				s.[FromDate],
				s.[ToDate],
				s.[DateOfBirth],
				s.[ContactEmail],
				s.[ContactMobile],
				s.[NormalizedContactMobile],
				s.[ContactAddress],
				s.[Date1],
				s.[Date2],
				s.[Date3],
				s.[Date4],
				s.[Decimal1],
				s.[Decimal2],
				s.[Int1],
				s.[Int2],
				s.[Lookup1Id],
				s.[Lookup2Id],
				s.[Lookup3Id],
				s.[Lookup4Id],
				s.[Lookup5Id],
				s.[Lookup6Id],
				s.[Lookup7Id],
				s.[Lookup8Id],
				s.[Text1],					
				s.[Text2],
				s.[Text3], 
				s.[Text4],
				s.[AgentId],
				s.[TaxIdentificationNumber],
				s.[JobId],
				s.[BankAccountNumber],
				s.[Relation1Id],
				IIF(s.[ImageId] = N'(Unchanged)', NULL, s.[ImageId])
				)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	-- The following code is needed for bulk import, when the reliance is on Relation1Index
	MERGE [dbo].[Relations] As t
	USING (
		SELECT II.[Id], IIRelation1.[Id] As Relation1Id
		FROM @Entities O
		JOIN @IndexedIds IIRelation1 ON IIRelation1.[Index] = O.Relation1Index
		JOIN @IndexedIds II ON II.[Index] = O.[Index]
	) As s
	ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[Relation1Id] = s.[Relation1Id];

	-- Relation Users
	WITH AU AS (
		SELECT * FROM dbo.[RelationUsers] RU
		WHERE RU.[RelationId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO AU AS t
	USING (
		SELECT
			RU.[Id],
			I.[Id] AS [RelationId],
			RU.[UserId]
		FROM @RelationUsers RU
		JOIN @IndexedIds I ON RU.[HeaderIndex] = I.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[UserId] <> s.[UserId])
	THEN
		UPDATE SET
			t.[UserId]					= s.[UserId],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[RelationId],
			[UserId]
		) VALUES (
			s.[RelationId],
			s.[UserId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Attachments
	WITH BA AS (
		SELECT * FROM dbo.[RelationAttachments]
		WHERE [RelationId] IN (
			SELECT II.[Id] FROM @IndexedIds II 
			JOIN @Entities E ON II.[Index] = E.[Index]
			WHERE E.[UpdateAttachments] = 1 -- Is this correct ?
		)
	)
	INSERT INTO @DeletedAttachmentIds([Id])
	SELECT x.[DeletedFileId]
	FROM
	(
		MERGE INTO BA AS t
		USING (
			SELECT
				A.[Id],
				DI.[Id] AS [RelationId],
				A.[CategoryId],
				A.[FileName],
				A.[FileExtension],
				A.[FileId],
				A.[Size]
			FROM @Attachments A 
			JOIN @IndexedIds DI ON A.[HeaderIndex] = DI.[Index]
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[FileName]			= s.[FileName],
				t.[CategoryId]			= s.[CategoryId],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([RelationId], [CategoryId], [FileName], [FileExtension], [FileId], [Size])
			VALUES (s.[RelationId], s.[CategoryId], s.[FileName], s.[FileExtension], s.[FileId], s.[Size])
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT INSERTED.[FileId] AS [InsertedFileId], DELETED.[FileId] AS [DeletedFileId]
	) AS x
	WHERE x.[InsertedFileId] IS NULL

	
	-- Return overwritten Image Ids, so C# can delete them from Blob Storage
	SELECT [Id] FROM @DeletedImageIds;

	-- Return deleted Attachment Ids, so C# can delete them from Blob Storage
	SELECT [Id] FROM @DeletedAttachmentIds;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END