CREATE TYPE [dbo].[AgentList] AS TABLE (
	[Index]						INT					PRIMARY KEY,
	[Id]						INT					NOT NULL DEFAULT 0,
	[Name]						NVARCHAR (255)		NOT NULL,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Code]						NVARCHAR (30),
	--[SystemCode]				NVARCHAR (30), -- some used are anoymous, self, parent
--	Common
	[AgentType]					NVARCHAR (30)		NOT NULL,  -- -- 'Individual', 'System' (software), 'Organization' (Dept, Team)
	[IsRelated]					BIT					NOT NULL DEFAULT 0,
	--[TaxIdentificationNumber]	NVARCHAR (30),  -- China has the maximum, 18 characters
	--[IsLocal]					BIT,
	--[Citizenship]				NCHAR(2),		-- ISO 3166-1 Alpha-2 code
	--[Facebook]					NVARCHAR (50),				
	--[Instagram]					NVARCHAR (30),				
	--[Twitter]					NVARCHAR (15),
	--[PreferredContactChannel1]	INT,			-- e.g., Mobile
	--[PreferredContactAddress1]	NVARCHAR (255),  -- e.g., +251 94 123 4567
	--[PreferredContactChannel2]	INT,			-- e.g., email
	--[PreferredContactAddress2]	NVARCHAR (255),	-- e.g., info@contoso.com
	[PreferredLanguage]			NCHAR (2)			NOT NULL DEFAULT (N'en'), 
----	Individuals only
----	--	Personal
--	[BirthDate]					DATE,
--	[Title]						NVARCHAR (50),		-- LKT
--	[TitleId]					TINYINT,		-- LKT
--	[Gender]					TINYINT,		-- ISO/IEC 5218. 0=unknown, 1=Male, 2=Female, 9=N/A
--	[ResidentialAddress]		NVARCHAR (1024), -- in the country language
--	[ImageId]					NVARCHAR (50),
----	--	Social
--	[MaritalStatus]				TINYINT,		-- LKT
--	[NumberOfChildren]			TINYINT,
--	[Religion]					NCHAR (1),		-- (?) I=Islam, C=Christianity, X=Others -- , J=Judaism, H=Hinduism, B=Buddhism
--	[Race]						TINYINT,		-- LKT
--	[TribeId]					INT,			-- LKT
--	[RegionId]					INT,			-- LKT
----	--	Academic
--	[EducationLevelId]			INT,			-- LKT
--	[EducationSublevelId]		INT,			-- ===
----	--	Financial
--	[BankId]					INT,			-- LKT
--	[BankAccountNumber]			NVARCHAR (34),  -- IBAN length
----	Organizations only
----	Organization type is defined by the government entity responsible for this organization. For instance, banks
----	are all handled by the central bank. Charities are handled by a different body, and so on.
--	[OrganizationType]			INT,			-- UDL General/Bank/Insurance/Charity/NGO/TaxOrg/Diplomatic
--	[WebSite]					NVARCHAR (255),
--	[ContactPerson]				NVARCHAR (255),
--	[RegisteredAddress]			NVARCHAR (1024),
--	[OwnershipType]				NVARCHAR (255), -- Investment/Shareholder/SisterCompany/Other(Default) -- We Own shares in them, they own share in us, ...
--	[OwnershipPercent]			DECIMAL	DEFAULT 0, -- If investment, how much the entity owns in this agent. If shareholder, how much he owns in the entity

	INDEX IX_AgentList__Code ([Code])
);