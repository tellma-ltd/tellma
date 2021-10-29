CREATE TYPE [dbo].[NotificationTemplateList] AS TABLE
(
	[Index] INT PRIMARY KEY,
	[Id] INT NOT NULL DEFAULT 0,
	[Name] NVARCHAR (255),
	[Name2] NVARCHAR (255),
	[Name3] NVARCHAR (255),
	[Code] NVARCHAR (50),
	[Description] NVARCHAR (1024),
	[Description2] NVARCHAR (1024),
	[Description3] NVARCHAR (1024),

	-- These 3 columns determine which of the subsequent columns are used
	[Channel] NVARCHAR (50),
	[Trigger] NVARCHAR (50),
	[Cardinality] NVARCHAR (50),

	[ListExpression] NVARCHAR (1024),		-- Bulk only
	[Schedule] NVARCHAR (1024),				-- Automatic only
	[ConditionExpression] NVARCHAR (1024),	-- Automatic only
	[MaximumRenotify] INT,					-- Automatic+Single only
	[Usage] NVARCHAR (50),					-- Manual only
	[Collection] NVARCHAR (50),				-- Manual only
	[DefinitionId] INT,						-- Manual only
	[ReportDefinitionId] INT,				-- Manual only
	[Subject] NVARCHAR (1024),				-- Email only
	[Body] NVARCHAR (MAX),
	[AddressExpression] NVARCHAR (1024),	-- Bulk only
	[Caption]  NVARCHAR (1024),
	[IsDeployed] BIT
)
