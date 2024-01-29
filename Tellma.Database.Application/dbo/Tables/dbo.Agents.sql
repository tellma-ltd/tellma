CREATE TABLE [dbo].[Agents] (
--	These includes all the natural and legal persons with which the business entity may interact
	[Id]						INT					CONSTRAINT [PK_Agents] PRIMARY KEY IDENTITY,
	[DefinitionId]				INT					NOT NULL	CONSTRAINT FK_Agents__DefinitionId REFERENCES dbo.[AgentDefinitions]([Id]),
								CONSTRAINT [UQ_Agents__Id_DefinitionId] UNIQUE ([Id], [DefinitionId]),
	[Name]						NVARCHAR (255)		NOT NULL,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Code]						NVARCHAR (50),

	[CurrencyId]				NCHAR (3)			CONSTRAINT [FK_Agents__CurrencyId] REFERENCES [dbo].[Currencies]([Id]),
	[CenterId]					INT					CONSTRAINT [FK_Agents__CenterId] REFERENCES [dbo].[Centers]([Id]),
	[ImageId]					NVARCHAR (50),
	[Description]				NVARCHAR (2048),
	[Description2]				NVARCHAR (2048),
	[Description3]				NVARCHAR (2048),
	[Location]					GEOGRAPHY,
	[LocationJson]				NVARCHAR (MAX),
	[FromDate]					DATE,			-- Joining Date
	[ToDate]					DATE,			-- Termination Date
	--
	[DateOfBirth]				DATE,
	[ContactEmail]				NVARCHAR (255),
	[ContactMobile]				NVARCHAR (50),
	[NormalizedContactMobile]	NVARCHAR (50),
	[ContactAddress]			NVARCHAR (255),
	[Date1]				DATE, -- Visa
	[Date2]				DATE, -- Passport
	[Date3]				DATE, -- Medical Insurance
	[Date4]				DATE, -- ..
	--
	[Decimal1]					DECIMAL (19,4),
	[Decimal2]					DECIMAL (19,4),
	[Int1]						INT,
	[Int2]						INT,
	[Lookup1Id]					INT					CONSTRAINT [FK_Agents__Lookup1Id] REFERENCES [dbo].[Lookups] ([Id]), -- citizenship
	[Lookup2Id]					INT					CONSTRAINT [FK_Agents__Lookup2Id] REFERENCES [dbo].[Lookups] ([Id]), -- religion
	[Lookup3Id]					INT					CONSTRAINT [FK_Agents__Lookup3Id] REFERENCES [dbo].[Lookups] ([Id]), -- Marital Status
	[Lookup4Id]					INT					CONSTRAINT [FK_Agents__Lookup4Id] REFERENCES [dbo].[Lookups] ([Id]), -- Salary Bank
	[Lookup5Id]					INT					CONSTRAINT [FK_Agents__Lookup5Id] REFERENCES [dbo].[Lookups] ([Id]), -- Gender
	[Lookup6Id]					INT					CONSTRAINT [FK_Agents__Lookup6Id] REFERENCES [dbo].[Lookups] ([Id]), -- Profession (as in Id)
	[Lookup7Id]					INT					CONSTRAINT [FK_Agents__Lookup7Id] REFERENCES [dbo].[Lookups] ([Id]), -- Educational Status
	[Lookup8Id]					INT					CONSTRAINT [FK_Agents__Lookup8Id] REFERENCES [dbo].[Lookups] ([Id]), -- 
--	
	[Text1]						NVARCHAR (255), -- Permanent Address
	[Text2]						NVARCHAR (255), -- 
	[Text3]						NVARCHAR (255), -- 
	[Text4]						NVARCHAR (255), -- 

	-- Address
	[AddressStreet]				NVARCHAR (50),
	[AddressAdditionalStreet]	NVARCHAR (50),
	[AddressBuildingNumber]		NVARCHAR (50),
	[AddressAdditionalNumber]	NVARCHAR (50),
	[AddressCity]				NVARCHAR (50),
	[AddressPostalCode]			NVARCHAR (50),
	[AddressProvince]			NVARCHAR (50),
	[AddressDistrict]			NVARCHAR (50),
	[AddressCountryCode]		NVARCHAR (2),

	[TaxIdentificationNumber]	NVARCHAR (18),
	[JobId]						INT, -- FK to table Jobs
	[BankAccountNumber]			NVARCHAR (34),
	[ExternalReference]			NVARCHAR (255),
	[UserId]					INT					CONSTRAINT [FK_Agents__UserId] REFERENCES [dbo].[Users] ([Id]),
	[Agent1Id]					INT					CONSTRAINT [FK_Agents__Agent1Id] REFERENCES [dbo].[Agents] ([Id]),
	[Agent2Id]					INT					CONSTRAINT [FK_Agents__Agent2Id] REFERENCES [dbo].[Agents] ([Id]),

	[IsActive]					BIT					NOT NULL DEFAULT 1,
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL CONSTRAINT [FK_Agents__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]				INT					NOT NULL CONSTRAINT [FK_Agents__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Agents__Definition_Code]
  ON [dbo].[Agents]([DefinitionId], [Code]) WHERE [Code] IS NOT NULL;
 GO