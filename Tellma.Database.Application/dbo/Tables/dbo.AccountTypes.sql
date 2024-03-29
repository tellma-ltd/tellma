﻿CREATE TABLE [dbo].[AccountTypes] (
	[Id]						INT					CONSTRAINT [PK_AccountTypes]  PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]					INT					CONSTRAINT [FK_AccountTypes__ParentId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[Code]						NVARCHAR (50)		NOT NULL CONSTRAINT [UQ_AccountTypes__Code] UNIQUE NONCLUSTERED, -- 50
	[Concept]					NVARCHAR (255)		NOT NULL CONSTRAINT [UQ_AccountTypes__Concept] UNIQUE NONCLUSTERED,
	[Name]						NVARCHAR (255)		NOT NULL,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[Node]						HIERARCHYID			NOT NULL CONSTRAINT [UQ_AccountTypes__Node] UNIQUE CLUSTERED,
	[IsMonetary]				BIT					DEFAULT 1,
	[IsAssignable]				BIT					NOT NULL DEFAULT 1,
	[StandardAndPure]			BIT					DEFAULT 0,
	[EntryTypeParentId]			INT					CONSTRAINT [FK_AccountTypes__EntryTypeParentId] REFERENCES [dbo].[EntryTypes] ([Id]),	
	[Time1Label]				NVARCHAR (50),
	[Time1Label2]				NVARCHAR (50),
	[Time1Label3]				NVARCHAR (50),
	[Time2Label]				NVARCHAR (50),
	[Time2Label2]				NVARCHAR (50),
	[Time2Label3]				NVARCHAR (50),
	[ExternalReferenceLabel]	NVARCHAR (50),
	[ExternalReferenceLabel2]	NVARCHAR (50),
	[ExternalReferenceLabel3]	NVARCHAR (50),
	[ReferenceSourceLabel]		NVARCHAR (50),
	[ReferenceSourceLabel2]		NVARCHAR (50),
	[ReferenceSourceLabel3]		NVARCHAR (50),
	[InternalReferenceLabel]	NVARCHAR (50),
	[InternalReferenceLabel2]	NVARCHAR (50),
	[InternalReferenceLabel3]	NVARCHAR (50),
	[NotedAgentNameLabel]		NVARCHAR (50),
	[NotedAgentNameLabel2]		NVARCHAR (50),
	[NotedAgentNameLabel3]		NVARCHAR (50),
	[NotedAmountLabel]			NVARCHAR (50),
	[NotedAmountLabel2]			NVARCHAR (50),
	[NotedAmountLabel3]			NVARCHAR (50),
	[NotedDateLabel]			NVARCHAR (50),
	[NotedDateLabel2]			NVARCHAR (50),
	[NotedDateLabel3]			NVARCHAR (50),
	-- Additional properties, Is Active at the end
	[IsActive]					BIT					NOT NULL DEFAULT 1,
	[IsSystem]					BIT					NOT NULL DEFAULT 0,
	[ChildCount]				INT,
	[ActiveChildCount]			INT,
	-- Audit properties
	[SavedById]					INT				NOT NULL CONSTRAINT [FK_AccountTypes__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[AccountTypesHistory]));
GO
CREATE INDEX [IX_AccountTypes__ParentId] ON [dbo].[AccountTypes]([ParentId]);
GO
CREATE TRIGGER dbo.traiu_AccountTypes ON [dbo].[AccountTypes]
AFTER INSERT, UPDATE
AS
	UPDATE AC
	SET AC.[ActiveChildCount] = T.[ActiveChildCount],
		AC.[ChildCount] = T.[ChildCount]
	FROM [dbo].[AccountTypes] AC
	CROSS APPLY (
			SELECT COUNT(*) AS [ChildCount],
			SUM(IIF([IsActive]=1,1,0)) AS  [ActiveChildCount]	
			FROM [dbo].[AccountTypes] 
			WHERE [Node].IsDescendantOf(AC.[Node]) = 1
	) T
	WHERE
		AC.[ActiveChildCount] IS NULL
	OR	AC.[ChildCount] IS NULL
	OR	AC.[ActiveChildCount] <> T.[ActiveChildCount]
	OR	AC.[ChildCount] <> T.[ChildCount]
GO