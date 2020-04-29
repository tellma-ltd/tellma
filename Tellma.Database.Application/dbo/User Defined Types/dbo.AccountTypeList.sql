﻿CREATE TYPE [dbo].[AccountTypeList] AS TABLE (
	[Index]						INT PRIMARY KEY ,
	[ParentIndex]				INT,
	[Id]						INT NOT NULL DEFAULT 0,
	[ParentId]					INT,
	[Name]						NVARCHAR (255)	NOT NULL,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[Code]						NVARCHAR (255)		NOT NULL UNIQUE,
	[IsAssignable]				BIT					NOT NULL DEFAULT 1,
	[EntryTypeParentId]			INT
	--[DueDateLabel]				NVARCHAR (50),
	--[DueDateLabel2]				NVARCHAR (50),
	--[DueDateLabel3]				NVARCHAR (50),
	--[Time1Label]				NVARCHAR (50),
	--[Time1Label2]				NVARCHAR (50),
	--[Time1Label3]				NVARCHAR (50),
	--[Time2Label]				NVARCHAR (50),
	--[Time2Label2]				NVARCHAR (50),
	--[Time2Label3]				NVARCHAR (50),
	--[ExternalReferenceLabel]	NVARCHAR (50),
	--[ExternalReferenceLabel2]	NVARCHAR (50),
	--[ExternalReferenceLabel3]	NVARCHAR (50),
	--[AdditionalReferenceLabel]	NVARCHAR (50),
	--[AdditionalReferenceLabel2]	NVARCHAR (50),
	--[AdditionalReferenceLabel3]	NVARCHAR (50),
	--[NotedAgentNameLabel]		NVARCHAR (50),
	--[NotedAgentNameLabel2]		NVARCHAR (50),
	--[NotedAgentNameLabel3]		NVARCHAR (50),
	--[NotedAmountLabel]			NVARCHAR (50),
	--[NotedAmountLabel2]			NVARCHAR (50),
	--[NotedAmountLabel3]			NVARCHAR (50),
	--[NotedDateLabel]			NVARCHAR (50),
	--[NotedDateLabel2]			NVARCHAR (50),
	--[NotedDateLabel3]			NVARCHAR (50)
);