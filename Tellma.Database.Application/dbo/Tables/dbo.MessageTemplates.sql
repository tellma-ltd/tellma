CREATE TABLE [dbo].[MessageTemplates]
(
	[Id] INT CONSTRAINT [PK_MessageTemplates] PRIMARY KEY NONCLUSTERED IDENTITY,
	[Name] NVARCHAR (255) NOT NULL CONSTRAINT [UQ_MessageTemplates__Name] UNIQUE,
	[Name2] NVARCHAR (255),
	[Name3] NVARCHAR (255),
	[Code] NVARCHAR (50),
	[Description] NVARCHAR (1024),
	[Description2] NVARCHAR (1024),
	[Description3] NVARCHAR (1024),

	-- These 2 columns determine which of the subsequent columns are used
	[Trigger] NVARCHAR (50) NOT NULL CONSTRAINT [CK_MessageTemplates__Trigger] CHECK ([Trigger] IN (N'Automatic', N'Manual')),
	[Cardinality] NVARCHAR (50) NOT NULL CONSTRAINT [CK_MessageTemplates__Cardinality] CHECK ([Cardinality] IN (N'Single', N'Multiple')),

	-- Multiple Only
	[ListExpression] NVARCHAR (MAX), -- The list where each row translates to a Message in multiple mode

	-- Automatic Only
	[Schedule] NVARCHAR (1024), -- CRON Expression for when to trigger the message
	[ConditionExpression] NVARCHAR (1024), -- Condition to evaluate before triggering the message
	[PreventRenotify] BIT NOT NULL DEFAULT 0, -- False if we allow the same message to be sent twice in a row to the same number in auto-mode
	[Version] NVARCHAR (1024), -- Optional: to be evaluated and used to compare messages when implementing Renotify

	-- Manual Only
	[Usage] NVARCHAR (50) NOT NULL CONSTRAINT [CK_MessageTemplates__Usage] CHECK ([Usage] IN (N'FromDetails', N'FromSearchAndDetails', N'Standalone')),
	[Collection] NVARCHAR (50),
	[DefinitionId] INT,

	-- Always
	[PhoneNumber] NVARCHAR (1024), -- Template of the number to receive the message, multiple numbers can be separated by a semi-colon
	[Content] NVARCHAR (MAX), -- Template of the message body
	[Caption]  NVARCHAR (1024) NOT NULL, -- Template to evaluate and store in MessageCommands
	[IsDeployed] BIT NOT NULL DEFAULT 0,
	
	-- For Standalone
	[ShowInMainMenu]	BIT,
	[MainMenuSection]	NVARCHAR (50),	-- IF Null, appears in the "Miscellaneous" section
	[MainMenuIcon]		NVARCHAR (50),
	[MainMenuSortKey]	DECIMAL (9,4),

	[CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT NOT NULL CONSTRAINT [FK_MessageTemplates__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById] INT NOT NULL CONSTRAINT [FK_MessageTemplates__ModifiedById] REFERENCES [dbo].[Users] ([Id])
)
