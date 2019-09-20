CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]							INT				PRIMARY KEY IDENTITY(0, 1),
	[Id]							INT				NOT NULL DEFAULT 0,
	[AccountClassificationId]		INT,
	[IfrsAccountClassificationId]	NVARCHAR (255)	NOT NULL,
	[Name]							NVARCHAR (255)	NOT NULL INDEX IX_Name UNIQUE,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255) INDEX IX_Code UNIQUE,
	[PartyReference]				NVARCHAR (255),
	[IfrsEntryClassificationId]		NVARCHAR (255),
	[IsMultiAgent]					BIT				NOT NULL DEFAULT 0,
	[AgentId]						INT,
	[IsMultiResponsibilityCenter]	BIT				NOT NULL DEFAULT 0,
	[ResponsibilityCenterId]		INT,
	[IsMultiResource]				BIT				NOT NULL DEFAULT 1,
	[ResourceId]					INT
);