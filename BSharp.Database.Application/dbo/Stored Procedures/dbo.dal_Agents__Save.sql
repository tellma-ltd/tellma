CREATE PROCEDURE [dbo].[dal_Agents__Save]
	@Entities [AgentList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[Agents] AS t
	USING (
		SELECT [Index], [Id], 
			[Name], [Name2], [Name3], [Code], [AgentType], [IsRelated], [TaxIdentificationNumber],
			[IsLocal], [Citizenship], [Facebook], [Instagram], [Twitter],
			[PreferredContactChannel1], [PreferredContactAddress1], [PreferredContactChannel2], [PreferredContactAddress2],
			[BirthDateTime], [TitleId], [Gender], [ResidentialAddress], [ImageId], [MaritalStatus], [NumberOfChildren],
			[Religion], [Race],  [TribeId], [RegionId],  
			[EducationLevelId], [EducationSublevelId], [BankId], [BankAccountNumber],
			[OrganizationType], [WebSite], [ContactPerson], [RegisteredAddress], [OwnershipType], [OwnershipPercent]

		FROM @Entities 
		WHERE [EntityState] IN (N'Inserted', N'Updated')
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED
	THEN
		UPDATE SET 
			t.[Name]					= s.[Name],
			t.[Name2]					= s.[Name2],
			t.[Name3]					= s.[Name3],
			t.[Code]					= s.[Code],
			t.[AgentType]				= s.[AgentType], 

			t.[IsRelated]				= s.[IsRelated],
			t.[TaxIdentificationNumber] = s.[TaxIdentificationNumber],
			t.[IsLocal]					= s.[IsLocal],
			t.[Citizenship]				= s.[Citizenship],
			t.[Facebook]				= s.[Facebook],
			t.[Instagram]				= s.[Instagram],
			t.[Twitter]					= s.[Twitter],
			t.[PreferredContactChannel1] = s.[PreferredContactChannel1],
			t.[PreferredContactAddress1] = s.[PreferredContactAddress1],
			t.[PreferredContactChannel2] = s.[PreferredContactChannel2],
			t.[PreferredContactAddress2] = s.[PreferredContactAddress2],

			t.[BirthDateTime]			= s.[BirthDateTime],
			t.[TitleId]					= s.[TitleId],
			t.[Gender]					= s.[Gender],
			t.[ResidentialAddress]		= s.[ResidentialAddress],
			t.[ImageId]					= s.[ImageId],

			t.[MaritalStatus]			= s.[MaritalStatus],
			t.[NumberOfChildren]		= s.[NumberOfChildren],
			t.[Religion]				= s.[Religion],
			t.[Race]					= s.[Race],
			t.[TribeId]					= s.[TribeId],
			t.[RegionId]				= s.[RegionId],

			t.[EducationLevelId]		= s.[EducationLevelId],
			t.[EducationSublevelId]		= s.[EducationSublevelId],
			t.[BankId]					= s.[BankId],
			t.[BankAccountNumber]		= s.[BankAccountNumber],

			t.[OrganizationType]		= s.[OrganizationType],
			t.[WebSite]					= s.[WebSite],
			t.[ContactPerson]			= s.[ContactPerson],
			t.[RegisteredAddress]		= s.[RegisteredAddress],
			t.[OwnershipType]			= s.[OwnershipType],
			t.[OwnershipPercent]		= s.[OwnershipPercent],

			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[Name], [Name2], [Name3], [Code], [AgentType], [IsRelated], [TaxIdentificationNumber],
			[IsLocal], [Citizenship], [Facebook], [Instagram], [Twitter],
			[PreferredContactChannel1], [PreferredContactAddress1], [PreferredContactChannel2], [PreferredContactAddress2],
			[BirthDateTime], [TitleId], [Gender], [ResidentialAddress], [ImageId], [MaritalStatus], [NumberOfChildren],
			[Religion], [Race],  [TribeId], [RegionId],  
			[EducationLevelId], [EducationSublevelId], [BankId], [BankAccountNumber],
			[OrganizationType], [WebSite], [ContactPerson], [RegisteredAddress], [OwnershipType], [OwnershipPercent])
		VALUES (
			s.[Name], s.[Name2], s.[Name3], s.[Code], s.[AgentType], s.[IsRelated], s.[TaxIdentificationNumber],
			s.[IsLocal], s.[Citizenship], s.[Facebook], s.[Instagram], s.[Twitter],
			s.[PreferredContactChannel1], s.[PreferredContactAddress1], s.[PreferredContactChannel2], s.[PreferredContactAddress2],
			s.[BirthDateTime], s.[TitleId], s.[Gender], s.[ResidentialAddress], s.[ImageId], s.[MaritalStatus], s.[NumberOfChildren], s.[Religion], s.[Race], s.[TribeId], s.[RegionId], 
			s.[EducationLevelId], s.[EducationSublevelId], s.[BankId], s.[BankAccountNumber],
			s.[OrganizationType], s.[WebSite], s.[ContactPerson], s.[RegisteredAddress], s.[OwnershipType], s.[OwnershipPercent])
	OUTPUT s.[Index], inserted.[Id];