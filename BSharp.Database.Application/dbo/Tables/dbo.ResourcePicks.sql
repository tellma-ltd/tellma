CREATE TABLE [dbo].[ResourcePicks] (
	[Id]					INT					CONSTRAINT [PK_ResourceInstances] PRIMARY KEY IDENTITY,
	[ResourceId]			INT					NOT NULL CONSTRAINT [FK_ResourceInstances__ResourceId]	FOREIGN KEY ([ResourceId])	REFERENCES [dbo].[Resources] ([Id]),
	[Name]					NVARCHAR(255),
	[Name2]					NVARCHAR(255),
	[Name3]					NVARCHAR(255),
--	Tag #, Coil #, Check #, LC #
	[Code]					NVARCHAR (255)		NOT NULL,
	
	[Area]					DECIMAL,
	[Count]					DECIMAL,
	[Length]				DECIMAL,
	[Mass]					DECIMAL,
	[MonetaryValue]			DECIMAL,
	[Time]					DECIMAL,
	[Volume]				DECIMAL,
	[Description]			NVARCHAR (2048), -- full details
	[Description2]			NVARCHAR (2048),
	[Description3]			NVARCHAR (2048),
-- Google Drive, One Drive, etc. | Activate collaboration
	[AttachmentsFolderURL]	NVARCHAR (255), 

	-- The following three properties apply to the same three tables...
	-- LinkType between Document and Resource, Document and Agent, Agent and Resource
	-- [LinkedAgentsRelations] specifies RelatedAgentRelation with the resource, 
	-- [LinkedDocuments]
	-- [LinkedResources]

-- Services Rendered
	-- AgentRelation = Employee
	[AccountManagerId]		INT				CONSTRAINT [FK_ResourceInstances__AccountManagerId]	FOREIGN KEY ([AccountManagerId]) REFERENCES [dbo].[Agents] ([Id]),
	-- AgentRelation = Employee
	[ProjectManagerId]		INT				CONSTRAINT [FK_ResourceInstances__ProjectManagerId]	FOREIGN KEY ([ProjectManagerId]) REFERENCES [dbo].[Agents] ([Id]),
	-- Account Type = N'TradeAndOtherCurrentReceivables'
	[ReceivableAccountId]	INT				CONSTRAINT [FK_ResourceInstances__CustomerId] FOREIGN KEY ([ReceivableAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	[State]					AS (CASE
									WHEN [IsActive] = 1 THEN N'Active'
									WHEN [IsActive] = 1 AND [AvailableTill] IS NOT NULL THEN N'Error!'
									WHEN [IsActive] = 0 AND [AvailableTill] IS NULL THEN N'Dormant'
									WHEN [IsActive] = 0 AND [AvailableTill] IS NOT NULL THEN N'Closed'
									ELSE NULL
								END) PERSISTED,
-- Case of Issued Financial Instruments (Liabilities/Equity)
	[Beneficiary]			NVARCHAR (255),
	[IssuingBankAccountId]	INT,
	-- For issued LC, we need a supplementary table generating the swift codes.
-- Case of Received Financial Instruments (Assets)
	[IssuingBankId]			INT,
-- Dynamic properties, defined by specs.

	[Text1]					NVARCHAR (255), -- Plate Number
	[Text2]					NVARCHAR (255), -- VIN
	[Date1]					DATE,			-- Registration Date
	[Date2]					DATE,			-- Oil change date
	-- Examples of the following properties are given for SKD
	-- However, they could also work for company vehicles, using Year, Make, and Model for Lookups
	[Lookup1Id]				INT,			-- External Color
	[Lookup2Id]				INT,			-- Internal Color
	[Lookup3Id]				INT,			-- Leather type
	[Lookup4Id]				INT,			-- Tire type
	[Lookup5Id]				INT,			-- Audio system
	-- ...
--
	[AvailableSince]		DATE, -- check date, service start date, PPE acquisition date, etc...
	[AvailableTill]			DATE, -- Check expiry date, service closure, PPE disposal, etc...
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	CONSTRAINT [CK_ResourceInstances__AvailableTo_IsActive] CHECK ([IsActive] = 0 OR [AvailableTill] IS NULL),
	[SortKey]				DECIMAL (9,4),
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceInstances__CreatedById]	FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) 	CONSTRAINT [FK_ResourceInstances__ModifiedById]	FOREIGN KEY ([ModifiedById])REFERENCES [dbo].[Users] ([Id])
);
GO;