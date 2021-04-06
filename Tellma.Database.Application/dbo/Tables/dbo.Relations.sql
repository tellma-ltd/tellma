CREATE TABLE [dbo].[Relations] (
--	These includes all the natural and legal persons with which the business entity may interact
	[Id]						INT					CONSTRAINT [PK_Relations] PRIMARY KEY IDENTITY,
	[DefinitionId]				INT					NOT NULL	CONSTRAINT [FK_Relations__DefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]),
								CONSTRAINT [IX_Relations__Id_DefinitionId] UNIQUE ([Id], [DefinitionId]),
	[Name]						NVARCHAR (255)		NOT NULL, -- CONSTRAINT [IX_Relations__Name] UNIQUE,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Code]						NVARCHAR (50),

	[CurrencyId]				NCHAR (3)			CONSTRAINT [FK_Relations__CurrencyId] REFERENCES dbo.[Currencies]([Id]),
	[CenterId]					INT					CONSTRAINT [FK_Relations__CenterId] REFERENCES dbo.[Centers]([Id]),
	[ImageId]					NVARCHAR (50),
	[Description]				NVARCHAR (2048),
	[Description2]				NVARCHAR (2048),
	[Description3]				NVARCHAR (2048),
	[Location]					GEOGRAPHY,
	[LocationJson]				NVARCHAR (MAX),
	[FromDate]					DATE,			-- Joining Date
	[ToDate]					DATE,			-- Terination Date
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
	[Lookup1Id]					INT					CONSTRAINT [FK_Relations__Lookup1Id] REFERENCES [dbo].[Lookups] ([Id]), -- citizenship
	[Lookup2Id]					INT					CONSTRAINT [FK_Relations__Lookup2Id] REFERENCES [dbo].[Lookups] ([Id]), -- religion
	[Lookup3Id]					INT					CONSTRAINT [FK_Relations__Lookup3Id] REFERENCES [dbo].[Lookups] ([Id]), -- Marital Status
	[Lookup4Id]					INT					CONSTRAINT [FK_Relations__Lookup4Id] REFERENCES [dbo].[Lookups] ([Id]), -- Salary Bank
	[Lookup5Id]					INT					CONSTRAINT [FK_Relations__Lookup5Id] REFERENCES [dbo].[Lookups] ([Id]), -- Gender
	[Lookup6Id]					INT					CONSTRAINT [FK_Relations__Lookup6Id] REFERENCES [dbo].[Lookups] ([Id]), -- Profession (as in Id)
	[Lookup7Id]					INT					CONSTRAINT [FK_Relations__Lookup7Id] REFERENCES [dbo].[Lookups] ([Id]), -- Educational Status
	[Lookup8Id]					INT					CONSTRAINT [FK_Relations__Lookup8Id] REFERENCES [dbo].[Lookups] ([Id]), -- 
--	
	[Text1]						NVARCHAR (255), -- Permanent Address
	[Text2]						NVARCHAR (255), -- 
	[Text3]						NVARCHAR (255), -- 
	[Text4]						NVARCHAR (255), -- 

	[AgentId]					INT					CONSTRAINT [FK_Relations__AgentId] REFERENCES [dbo].[Agents] ([Id]),
	[TaxIdentificationNumber]	NVARCHAR (18),
	[JobId]						INT, -- FK to table Jobs
	[BankAccountNumber]			NVARCHAR (34),
	[ExternalReference]			NVARCHAR (255),
	[Relation1Id]				INT					CONSTRAINT [FK_Relations__RelationId] REFERENCES [dbo].[Relations] ([Id]),

	[IsActive]					BIT					NOT NULL DEFAULT 1,
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Relations__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Relations__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Relations__Code]
  ON [dbo].[Relations]([Code]) WHERE [Code] IS NOT NULL;
 GO
 CREATE INDEX [IX_Relations__DefinitionId] ON [dbo].[Relations]([DefinitionId])
 GO