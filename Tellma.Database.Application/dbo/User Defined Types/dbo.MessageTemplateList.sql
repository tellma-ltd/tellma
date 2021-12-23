CREATE TYPE [dbo].[MessageTemplateList] AS TABLE
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

	-- These 2 columns determine which of the subsequent columns are used
	[Trigger] NVARCHAR (50),
	[Cardinality] NVARCHAR (50),
	
	-- Multiple only
	[ListExpression] NVARCHAR (MAX),
	
	-- Automatic only
	[Schedule] NVARCHAR (1024),
	[ConditionExpression] NVARCHAR (1024),
	[Renotify] BIT,
	[Version] NVARCHAR (1024),

	-- Manual only
	[Usage] NVARCHAR (50),
	[Collection] NVARCHAR (50),
	[DefinitionId] INT,

	-- Always
	[PhoneNumber] NVARCHAR (1024),
	[Content] NVARCHAR (MAX),
	[Caption]  NVARCHAR (1024),
	[IsDeployed] BIT
)
