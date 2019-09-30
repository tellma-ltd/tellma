CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]							INT				PRIMARY KEY,
	[Id]							INT				NOT NULL DEFAULT 0,
	[AccountTypeId]					NVARCHAR (255)	NOT NULL,
	[AccountClassificationId]		INT,
	[Name]							NVARCHAR (255)	NOT NULL INDEX IX_Name UNIQUE,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255),
	[PartyReference]				NVARCHAR (255),
	[SubAccountId]					INT				NOT NULL DEFAULT 0,
	[ResponsibleActorId]			INT, -- e.g., Ashenafi
	[ResponsibleRoleId]				INT, -- e.g., Marketing Dept Manager
	[CustodianActorId]				INT, -- Alex
	[CustodianRoleId]				INT, -- Raw Materials Warehouse Keeper
	[ResourceId]					INT,
	[LocationId]					INT	
);