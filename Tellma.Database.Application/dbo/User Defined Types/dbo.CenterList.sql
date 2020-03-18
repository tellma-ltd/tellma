CREATE TYPE [dbo].[CenterList] AS TABLE (
	[Index]				INT					PRIMARY KEY,
	[ParentIndex]		INT,
	[Id]				INT					NOT NULL DEFAULT 0,
	[ParentId]			INT,  
	[CenterType]		NVARCHAR (50)		NOT NULL CHECK ([CenterType] IN (
													N'Investment', N'Profit', N'Revenue', N'Cost')
												),
	[Name]				NVARCHAR (255)		NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[ManagerId]			INT,
-- (Ifrs 8) Profit or Investment Center, Performance regularly reviewed by CODM, discrete financial information is available
	--[IsOperatingSegment]	BIT					NOT NULL DEFAULT 0, -- on each path from root to leaf, at most one O/S

	[Code]				NVARCHAR (255),
	[IsLeaf]			BIT DEFAULT 1
	INDEX IX_CenterList__Code ([Code])
);