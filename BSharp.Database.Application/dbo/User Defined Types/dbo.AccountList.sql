CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]							INT				PRIMARY KEY,
	[Id]							INT				NOT NULL DEFAULT 0,
	[AccountClassificationId]		INT,
	
	[Name]							NVARCHAR (255)	NOT NULL INDEX IX_Name UNIQUE,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255),

	[AccountTypeId]					NVARCHAR (50)		NOT NULL,
	[AgentDefinitionId]				NVARCHAR (50),
	[ResourceTypeId]				NVARCHAR (50),
	[IsCurrent]						BIT,
-- Minor properties: range of values is restricted by defining a major property. For example, if AccountTypeId = N'Payable', then responsibility center
-- must be an operating segment. 
-- NULL means two things:
--	a) If the type itself is null, then it is not defined
--	b) if the type itself is not null, then it is to be defined in entries.

	[AgentId]						INT,
	[ResourceId]					INT,
	[ResponsibilityCenterId]		INT,
	[DescriptorId]					NVARCHAR (10),
--
	[EntryTypeId]					NVARCHAR (255)
);