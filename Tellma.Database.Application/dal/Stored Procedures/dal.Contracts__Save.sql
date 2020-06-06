CREATE PROCEDURE [dal].[Contracts__Save]
	@DefinitionId INT,
	@Entities [ContractList] READONLY,
	@ContractUsers dbo.[ContractUserList] READONLY,
	@ImageIds [IndexedImageIdList] READONLY, -- Index, ImageId
	@ReturnIds BIT = 0
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Contracts] AS t
		USING (
			SELECT [Index], [Id], --[OperatingSegmentId],
				@DefinitionId AS [DefinitionId], [Name], [Name2], [Name3], [Code],
				[AgentId],
				[CurrencyId],
				[TaxIdentificationNumber], --[ImageId], -- imageId is handled separately in the code below.
				--[IsLocal], [Citizenship], [Facebook], [Instagram], [Twitter],
				--[PreferredContactChannel1], [PreferredContactAddress1], [PreferredContactChannel2], [PreferredContactAddress2],
				--[PreferredLanguage]
				--[BirthDate], [Title], [TitleId], [Gender], [ResidentialAddress], [MaritalStatus], [NumberOfChildren],
				--[Religion], [Race],  [TribeId], [RegionId],  
				--[EducationLevelId], [EducationSublevelId], [BankId], [BankAccountNumber],
				--[OrganizationType], [WebSite], [ContactPerson], [RegisteredAddress], [OwnershipType], [OwnershipPercent]
				[StartDate],
				--[CreditLine],
				[JobId],
				[BankAccountNumber],
				[UserId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED
		THEN
			UPDATE SET
				--t.[OperatingSegmentId]		= s.[OperatingSegmentId],
				t.[DefinitionId]			= s.[DefinitionId],
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Code]					= s.[Code],
				t.[AgentId]					= s.[AgentId],
				t.[CurrencyId]				= s.[CurrencyId],
				t.[TaxIdentificationNumber] = s.[TaxIdentificationNumber],
			--	t.[ImageId]					= s.[ImageId],
				--t.[IsLocal]					= s.[IsLocal],
				--t.[Citizenship]				= s.[Citizenship],
				--t.[Facebook]				= s.[Facebook],
				--t.[Instagram]				= s.[Instagram],
				--t.[Twitter]					= s.[Twitter],
				--t.[PreferredContactChannel1] = s.[PreferredContactChannel1],
				--t.[PreferredContactAddress1] = s.[PreferredContactAddress1],
				--t.[PreferredContactChannel2] = s.[PreferredContactChannel2],
				--t.[PreferredContactAddress2] = s.[PreferredContactAddress2],
				--t.[PreferredLanguage] = s.[PreferredLanguage],

				--t.[BirthDate]				= s.[BirthDate],
				--t.[Title]					= s.[Title],
				--t.[TitleId]					= s.[TitleId],
				--t.[Gender]					= s.[Gender],
				--t.[ResidentialAddress]		= s.[ResidentialAddress],

				--t.[MaritalStatus]			= s.[MaritalStatus],
				--t.[NumberOfChildren]		= s.[NumberOfChildren],
				--t.[Religion]				= s.[Religion],
				--t.[Race]					= s.[Race],
				--t.[TribeId]					= s.[TribeId],
				--t.[RegionId]				= s.[RegionId],

				--t.[EducationLevelId]		= s.[EducationLevelId],
				--t.[EducationSublevelId]		= s.[EducationSublevelId],
				--t.[BankId]					= s.[BankId],
				--t.[BankAccountNumber]		= s.[BankAccountNumber],

				--t.[OrganizationType]		= s.[OrganizationType],
				--t.[WebSite]					= s.[WebSite],
				--t.[ContactPerson]			= s.[ContactPerson],
				--t.[RegisteredAddress]		= s.[RegisteredAddress],
				--t.[OwnershipType]			= s.[OwnershipType],
				--t.[OwnershipPercent]		= s.[OwnershipPercent],

				t.[StartDate]				= s.[StartDate],
				--[CreditLine],
				t.[JobId]					= s.[JobId],
				t.[BankAccountNumber]		= s.[BankAccountNumber],
				t.[UserId]					= s.[UserId],

				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (--[OperatingSegmentId],
				[DefinitionId], [Name], [Name2], [Name3], [Code],
				[AgentId],
				[CurrencyId],
				[TaxIdentificationNumber],--,[ImageId]
				--[IsLocal], [Citizenship], [Facebook], [Instagram], [Twitter],
				--[PreferredContactChannel1], [PreferredContactAddress1], [PreferredContactChannel2], [PreferredContactAddress2],
				--[PreferredLanguage]
				--[BirthDate], [Title], [TitleId], [Gender], [ResidentialAddress], [MaritalStatus], [NumberOfChildren],
				--[Religion], [Race],  [TribeId], [RegionId],  
				--[EducationLevelId], [EducationSublevelId], [BankId], [BankAccountNumber],
				--[OrganizationType], [WebSite], [ContactPerson], [RegisteredAddress], [OwnershipType], [OwnershipPercent]
				[StartDate],
				--[CreditLine],
				[JobId],
				[BankAccountNumber],
				[UserId]
				)
			VALUES (--s.[OperatingSegmentId],
				s.[DefinitionId], s.[Name], s.[Name2], s.[Name3], s.[Code],
				s.[AgentId],
				s.[CurrencyId],
				s.[TaxIdentificationNumber],--, s[ImageId]
				--s.[IsLocal], s.[Citizenship], s.[Facebook], s.[Instagram], s.[Twitter],
				--s.[PreferredContactChannel1], s.[PreferredContactAddress1], s.[PreferredContactChannel2], s.[PreferredContactAddress2],
				--s.[PreferredLanguage]
				--s.[BirthDate], s.[Title], s.[TitleId], s.[Gender], s.[ResidentialAddress], s.[MaritalStatus], s.[NumberOfChildren], s.[Religion], s.[Race], s.[TribeId], s.[RegionId], 
				--s.[EducationLevelId], s.[EducationSublevelId], s.[BankId], s.[BankAccountNumber],
				--s.[OrganizationType], s.[WebSite], s.[ContactPerson], s.[RegisteredAddress], s.[OwnershipType], s.[OwnershipPercent]
				s.[StartDate],
				--[CreditLine],
				s.[JobId],
				s.[BankAccountNumber],
				s.[UserId]
				)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	WITH AU AS (
		SELECT * FROM dbo.[ContractUsers] RU
		WHERE RU.[ContractId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO AU AS t
	USING (
		SELECT
			RU.[Id],
			I.[Id] AS [ContractId],
			RU.[UserId]
		FROM @ContractUsers RU
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
			[ContractId],
			[UserId]
		) VALUES (
			s.[ContractId],
			s.[UserId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- indices appearing in IndexedImageList will cause the imageId to be update, if different.
	UPDATE A --dbo.Contracts
	SET A.ImageId = L.ImageId
	FROM dbo.[Contracts] A
	JOIN @IndexedIds II ON A.Id = II.[Id]
	JOIN @ImageIds L ON II.[Index] = L.[Index]

	IF @ReturnIds = 1
	SELECT * FROM @IndexedIds;
END