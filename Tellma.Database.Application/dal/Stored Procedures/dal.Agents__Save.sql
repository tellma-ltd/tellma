CREATE PROCEDURE [dal].[Agents__Save]
	@DefinitionId INT,
	@Entities dbo.[AgentList] READONLY,
	@AgentUsers dbo.[AgentUserList] READONLY,
	@Attachments [dbo].[AgentAttachmentList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList], @DeletedImageIds [dbo].[StringList], @DeletedAttachmentIds [dbo].[StringList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- Entities whose ImageIds will be updated: capture their old ImageIds first (if any) so C# can delete them from blob storage
	INSERT INTO @DeletedImageIds ([Id])
	SELECT [ImageId] FROM [dbo].[Agents] E
	WHERE E.[ImageId] IS NOT NULL 
		AND E.[Id] IN (SELECT [Id] FROM @Entities WHERE [ImageId] IS NULL OR [ImageId] <> N'(Unchanged)');

	-- Save the entities
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Agents] AS t
		USING (
			SELECT [Index], [Id],
				@DefinitionId AS [DefinitionId],
				[Name], 
				[Name2], 
				[Name3],
				[Code],
				[Identifier],
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
				[AddressStreet],
				[AddressAdditionalStreet],
				[AddressBuildingNumber],
				[AddressAdditionalNumber],
				[AddressCity],
				[AddressPostalCode],
				[AddressProvince],
				[AddressDistrict],
				[AddressCountryId],
				[TaxIdentificationNumber],
				[BankAccountNumber],
				[ExternalReference],
				[UserId],
				[Agent1Id],
				[Agent2Id],
				[ImageId]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED
		THEN
			UPDATE SET
				t.[DefinitionId]			= s.[DefinitionId],
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Identifier]				= s.[Identifier],
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
				t.[AddressStreet]			= s.[AddressStreet],
				t.[AddressAdditionalStreet]	= s.[AddressAdditionalStreet],
				t.[AddressBuildingNumber]	= s.[AddressBuildingNumber],
				t.[AddressAdditionalNumber]	= s.[AddressAdditionalNumber],
				t.[AddressCity]				= s.[AddressCity],
				t.[AddressPostalCode]		= s.[AddressPostalCode],
				t.[AddressProvince]			= s.[AddressProvince],
				t.[AddressDistrict]			= s.[AddressDistrict],
				t.[AddressCountryId]		= s.[AddressCountryId],
				t.[TaxIdentificationNumber] = s.[TaxIdentificationNumber],
				t.[BankAccountNumber]		= s.[BankAccountNumber],
				t.[ExternalReference]		= s.[ExternalReference],
				t.[UserId]					= s.[UserId],
				t.[Agent1Id]				= s.[Agent1Id],
				t.[Agent2Id]				= s.[Agent2Id],

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
				[Identifier],
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
				[AddressStreet],
				[AddressAdditionalStreet],
				[AddressBuildingNumber],
				[AddressAdditionalNumber],
				[AddressCity],
				[AddressPostalCode],
				[AddressProvince],
				[AddressDistrict],
				[AddressCountryId],
				[TaxIdentificationNumber],
				[BankAccountNumber],
				[ExternalReference],
				[UserId],
				[Agent1Id],
				[Agent2Id],
				[ImageId],
				[CreatedById], 
				[CreatedAt], 
				[ModifiedById], 
				[ModifiedAt]
				)
			VALUES (
				s.[DefinitionId],
				s.[Name], 
				s.[Name2], 
				s.[Name3],
				s.[Code],
				s.[Identifier],
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
				s.[AddressStreet],
				s.[AddressAdditionalStreet],
				s.[AddressBuildingNumber],
				s.[AddressAdditionalNumber],
				s.[AddressCity],
				s.[AddressPostalCode],
				s.[AddressProvince],
				s.[AddressDistrict],
				s.[AddressCountryId],
				s.[TaxIdentificationNumber],
				s.[BankAccountNumber],
				s.[ExternalReference],
				s.[UserId],
				s.[Agent1Id],
				s.[Agent2Id],
				IIF(s.[ImageId] = N'(Unchanged)', NULL, s.[ImageId]),
				@UserId,
				@Now,
				@UserId,
				@Now
				)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	-- The following code is needed for bulk import, when the reliance is on Agent1Index
	MERGE [dbo].[Agents] As t
	USING (
		SELECT II.[Id], IIAgent1.[Id] As Agent1Id
		FROM @Entities O
		JOIN @IndexedIds IIAgent1 ON IIAgent1.[Index] = O.[Agent1Index]
		JOIN @IndexedIds II ON II.[Index] = O.[Index]
	) As s
	ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[Agent1Id] = s.[Agent1Id];

	MERGE [dbo].[Agents] As t
	USING (
		SELECT II.[Id], IIAgent2.[Id] As Agent2Id
		FROM @Entities O
		JOIN @IndexedIds IIAgent2 ON IIAgent2.[Index] = O.[Agent2Index]
		JOIN @IndexedIds II ON II.[Index] = O.[Index]
	) As s
	ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[Agent2Id] = s.[Agent2Id];

	-- Agent Users
	WITH AU AS (
		SELECT * FROM dbo.[AgentUsers] RU
		WHERE RU.[AgentId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO AU AS t
	USING (
		SELECT
			RU.[Id],
			I.[Id] AS [AgentId],
			RU.[UserId]
		FROM @AgentUsers RU
		JOIN @IndexedIds I ON RU.[HeaderIndex] = I.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED AND (t.[UserId] <> s.[UserId])
	THEN
		UPDATE SET
			t.[UserId]			= s.[UserId],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[AgentId],
			[UserId],
			[CreatedById], 
			[CreatedAt], 
			[ModifiedById], 
			[ModifiedAt]
		) VALUES (
			s.[AgentId],
			s.[UserId],
			@UserId,
			@Now,
			@UserId,
			@Now
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Attachments
	WITH BA AS (
		SELECT * FROM dbo.[AgentAttachments]
		WHERE [AgentId] IN (
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
				DI.[Id] AS [AgentId],
				A.[CategoryId],
				A.[FileName],
				A.[FileExtension],
				A.[FileId],
				A.[Size]
			FROM @Attachments A 
			JOIN @IndexedIds DI ON A.[HeaderIndex] = DI.[Index]
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED THEN
			UPDATE SET
				t.[FileName]		= s.[FileName],
				t.[CategoryId]		= s.[CategoryId],
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([AgentId], [CategoryId], [FileName], [FileExtension], [FileId], [Size], [CreatedById], [CreatedAt], [ModifiedById], [ModifiedAt])
			VALUES (s.[AgentId], s.[CategoryId], s.[FileName], s.[FileExtension], s.[FileId], s.[Size], @UserId, @Now, @UserId, @Now)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT INSERTED.[FileId] AS [InsertedFileId], DELETED.[FileId] AS [DeletedFileId]
	) AS x
	WHERE x.[InsertedFileId] IS NULL

	
	-- Return overwritten Image Ids, so C# can delete them from Blob Storage
	SELECT [Id] FROM @DeletedImageIds;

	-- Return the Ids of the saved entities
	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;

	-- Return deleted Attachment Ids, so C# can delete them from Blob Storage
	SELECT [Id] FROM @DeletedAttachmentIds;
END;
