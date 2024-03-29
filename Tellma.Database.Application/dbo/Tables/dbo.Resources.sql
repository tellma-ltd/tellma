﻿CREATE TABLE [dbo].[Resources] (
-- Resource can be seen as the true leaf level of "real" Account Types.
	[Id]						INT					CONSTRAINT [PK_Resources] PRIMARY KEY IDENTITY,
	[DefinitionId]				INT					NOT NULL CONSTRAINT [FK_Resources__DefinitionId] REFERENCES dbo.ResourceDefinitions([Id]),
	[Name]						NVARCHAR (255)		NOT NULL,
	CONSTRAINT [UQ_Resources__ResourceDefinitionId_Name_Identifier] UNIQUE ([DefinitionId],[Name],[Identifier]),
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Code]						NVARCHAR (50),
	-- CurrencyId was allowed to be NULL on Oct 31, 2021
	[CurrencyId]				NCHAR (3)			CONSTRAINT [FK_Resources__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	[CenterId]					INT					CONSTRAINT [FK_Resources__CenterId] REFERENCES dbo.[Centers]([Id]),
	[ImageId]					NVARCHAR (50),
	[Description]				NVARCHAR (2048),
	[Description2]				NVARCHAR (2048),
	[Description3]				NVARCHAR (2048),
	[Location]					GEOGRAPHY,
	[LocationJson]				NVARCHAR (MAX),
	[FromDate]					DATE,
	[ToDate]					DATE,
	[Date1]						DATE,
	[Date2]						DATE,
	[Date3]						DATE,
	[Date4]						DATE,
	[Decimal1]					DECIMAL (19,4),
	[Decimal2]					DECIMAL (19,4),
	[Decimal3]					DECIMAL (19,4),
	[Decimal4]					DECIMAL (19,4),
	[Int1]						INT,
	[Int2]						INT,
	[Lookup1Id]					INT					CONSTRAINT [FK_Resources__Lookup1Id] REFERENCES [dbo].[Lookups] ([Id]),
	[Lookup2Id]					INT					CONSTRAINT [FK_Resources__Lookup2Id] REFERENCES [dbo].[Lookups] ([Id]),
	[Lookup3Id]					INT					CONSTRAINT [FK_Resources__Lookup3Id] REFERENCES [dbo].[Lookups] ([Id]),
	[Lookup4Id]					INT					CONSTRAINT [FK_Resources__Lookup4Id] REFERENCES [dbo].[Lookups] ([Id]),
	[Text1]						NVARCHAR (255),
	[Text2]						NVARCHAR (255), 
-- Specific to resources
	[Identifier]				NVARCHAR (50),
	[VatRate]					DECIMAL (19,4)		CONSTRAINT [CK_Resources__VatRate] CHECK ([VatRate] BETWEEN 0 AND 1),
	[ReorderLevel]				DECIMAL (19,4),
	[EconomicOrderQuantity]		DECIMAL (19,4),
	-- for non current, unit = usage unit (must NOT be pure). 
	-- And the "pure" is implicit, and allowed in 2 account types: Non current and disposal (needs flag: Allows pure)
	[UnitId]					INT					CONSTRAINT [FK_Resources__UnitId] REFERENCES [dbo].[Units] ([Id]),
	[UnitMass]					DECIMAL (19,4),
	[UnitMassUnitId]			INT					CONSTRAINT [FK_Resources__MassUnitId] REFERENCES [dbo].[Units] ([Id]),
	-- 
	[MonetaryValue]				DECIMAL (19,4),
	[Agent1Id]					INT					CONSTRAINT [FK_Resources__Agent1Id] REFERENCES [dbo].[Agents] ([Id]),
	[Agent2Id]					INT					CONSTRAINT [FK_Resources__Agent12d] REFERENCES [dbo].[Agents] ([Id]),
	[Resource1Id]				INT					CONSTRAINT [FK_Resources__Resource1Id] REFERENCES dbo.[Resources] ([Id]),
	[Resource2Id]				INT					CONSTRAINT [FK_Resources__Resource2Id] REFERENCES dbo.[Resources] ([Id]),

	[IsActive]					BIT					NOT NULL DEFAULT 1,
	
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL,
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL,
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Resources__DefinitionId_Name2_Identifier]
  ON [dbo].[Resources]([DefinitionId], [Name2], [Identifier]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Resources__DefinitionId_Name3_Identifier]
  ON [dbo].[Resources]([DefinitionId], [Name3], [Identifier]) WHERE [Name3] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Resources__ResourceDefinitionId_Code]
  ON [dbo].[Resources]([DefinitionId], [Code]) WHERE [Code] IS NOT NULL;
GO
CREATE INDEX [IX_Resources__ResourceDefinitionId]   ON [dbo].[Resources]([DefinitionId]);
GO