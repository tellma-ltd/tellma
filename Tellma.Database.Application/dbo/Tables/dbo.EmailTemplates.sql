CREATE TABLE [dbo].[EmailTemplates]
(
	[Id] INT CONSTRAINT [PK_EmailTemplates] PRIMARY KEY NONCLUSTERED IDENTITY,
	[Name] NVARCHAR (255) NOT NULL CONSTRAINT [UQ_EmailTemplates__Name] UNIQUE,
	[Name2] NVARCHAR (255),
	[Name3] NVARCHAR (255),
	[Code] NVARCHAR (50),
	[Description] NVARCHAR (1024),
	[Description2] NVARCHAR (1024),
	[Description3] NVARCHAR (1024),
	
	-- These 2 columns determine which of the subsequent columns are used
	[Trigger] NVARCHAR (50) NOT NULL CONSTRAINT [CK_EmailTemplates__Trigger] CHECK ([Trigger] IN (N'Automatic', N'Manual')),
	[Cardinality] NVARCHAR (50) NOT NULL CONSTRAINT [CK_EmailTemplates__Cardinality] CHECK ([Cardinality] IN (N'Single', N'Multiple')),
	
	-- Multiple Only
	[ListExpression] NVARCHAR (MAX),

	-- Automatic Only
	[Schedule] NVARCHAR (1024),
	[ConditionExpression] NVARCHAR (1024),

	-- Manual Only
	[Usage] NVARCHAR (50) CONSTRAINT [CK_EmailTemplates__Usage] CHECK ([Usage] IN (N'FromDetails', N'FromSearchAndDetails', N'Standalone')),
	[Collection] NVARCHAR (50),
	[DefinitionId] INT,

	-- Always
	[EmailAddress] NVARCHAR (1024),
	[Subject] NVARCHAR (1024),
	[Body] NVARCHAR (MAX),
	[Caption]  NVARCHAR (1024) NOT NULL,
	[IsDeployed] BIT NOT NULL DEFAULT 0,
	
	-- For Standalone
	[MainMenuSection]	NVARCHAR (50),	-- IF Null, appears in the "Miscellaneous" section
	[MainMenuIcon]		NVARCHAR (50),
	[MainMenuSortKey]	DECIMAL (9,4),

	[CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT NOT NULL CONSTRAINT [FK_EmailTemplates__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById] INT NOT NULL CONSTRAINT [FK_EmailTemplates__ModifiedById] REFERENCES [dbo].[Users] ([Id]),

	-- For Automatic
	[LastExecuted] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[IsError] BIT NOT NULL DEFAULT 0
)
