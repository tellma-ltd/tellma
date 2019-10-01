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
	[ResponsibilityCenterId]		INT, -- e.g., Ashenafi
	[CustodianId]					INT, -- Alex
	[ResourceId]					INT,
	[LocationId]					INT	
);