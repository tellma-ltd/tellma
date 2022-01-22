CREATE TABLE [dbo].[NotificationTemplates]
(
	[Id] INT CONSTRAINT [PK_NotificationTemplates] PRIMARY KEY NONCLUSTERED IDENTITY,
	[Name] NVARCHAR (255) NOT NULL CONSTRAINT [UQ_NotificationTemplates__Name] UNIQUE,
	[Name2] NVARCHAR (255),
	[Name3] NVARCHAR (255),
	[Code] NVARCHAR (50),
	[Description] NVARCHAR (1024),
	[Description2] NVARCHAR (1024),
	[Description3] NVARCHAR (1024),

	-- These 3 columns determine which of the subsequent columns are used
	[Channel] NVARCHAR (50) NOT NULL CONSTRAINT [CK_NotificationTemplates__Channel] CHECK ([Channel] IN (N'Email', N'Sms')),
	[Trigger] NVARCHAR (50) NOT NULL CONSTRAINT [CK_NotificationTemplates__Trigger] CHECK ([Trigger] IN (N'Automatic', N'Manual')),
	[Cardinality] NVARCHAR (50) NOT NULL CONSTRAINT [CK_NotificationTemplates__Cardinality] CHECK ([Cardinality] IN (N'Single', N'Bulk')),

	[ListExpression] NVARCHAR (MAX), -- Bulk only
	[Schedule] NVARCHAR (1024), -- Automatic only
	[ConditionExpression] NVARCHAR (1024), -- Automatic only
	[MaximumRenotify] INT, -- Automatic+Single only
	[Usage] NVARCHAR (50) NOT NULL CONSTRAINT [CK_NotificationTemplates__Usage] CHECK ([Usage] IN (N'FromDetails', N'FromSearchAndDetails')), -- Manual only
	[Collection] NVARCHAR (50), -- Manual only
	[DefinitionId] INT, -- Manual only
	[ReportDefinitionId] INT CONSTRAINT [FK_NotificationTemplates__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]), -- Manual only
	[Subject] NVARCHAR (1024), -- Email only
	[Body] NVARCHAR (MAX),
	[AddressExpression] NVARCHAR (1024), -- Bulk only
	[Caption]  NVARCHAR (1024) NOT NULL,
	[IsDeployed] BIT NOT NULL DEFAULT 0,

	[CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById] INT NOT NULL CONSTRAINT [FK_NotificationTemplates__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById] INT NOT NULL CONSTRAINT [FK_NotificationTemplates__ModifiedById] REFERENCES [dbo].[Users] ([Id]),

	-- For Automatic
	[LastExecuted] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[IsError] BIT NOT NULL DEFAULT 0
)
