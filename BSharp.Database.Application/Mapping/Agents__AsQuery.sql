CREATE FUNCTION [map].[Agents__AsQuery]
(	
@Entities [dbo].[AgentList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Index] AS [Id],
		[Name],
		[Name2],
		[Name3],
		[Code],--	Common
		[IsRelated],
		[TaxIdentificationNumber],
		--[IsLocal],
		--[Citizenship],
		--[Facebook],
		--[Instagram],
		--[Twitter],
		--[PreferredContactChannel1],
		--[PreferredContactAddress1],
		--[PreferredContactChannel2],
		--[PreferredContactAddress2],
		--[PreferredLanguage],
	--	Individuals only
	--	--	Personal
		--[BirthDate],
		--[Title],
		--[TitleId],
		--[Gender],
		--[ResidentialAddress],
		NULL AS [ImageId],

	--	--	Social
		--[MaritalStatus],
		--[NumberOfChildren],
		--[Religion],
		--[Race],
		--[TribeId],
		--[RegionId],
	--	--	Academic
		--[EducationLevelId],
		--[EducationSublevelId],
	--	--	Financial
		--[BankId],
		--[BankAccountNumber],
	--	Organizations only
	--	Organization type is defined by the government entity responsible for this organization. For instance, banks
	--	are all handled by the central bank. Charities are handled by a different body, and so on.
		--[OrganizationType],
		--[WebSite],
		--[ContactPerson],
		--[RegisteredAddress],
		1 AS [IsActive],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
);
