CREATE TYPE [dbo].[LineDefinitionList] AS TABLE (
	[Index]								INT PRIMARY KEY,
	[Id]								NVARCHAR (50) NOT NULL UNIQUE,
	[Description]						NVARCHAR (255),
	[Description2]						NVARCHAR (255),
	[Description3]						NVARCHAR (255),
	[TitleSingular]						NVARCHAR (255) NOT NULL,
	[TitleSingular2]					NVARCHAR (255),
	[TitleSingular3]					NVARCHAR (255),
	[TitlePlural]						NVARCHAR (255) NOT NULL,
	[TitlePlural2]						NVARCHAR (255),
	[TitlePlural3]						NVARCHAR (255),
	[AgentDefinitionList]				NVARCHAR (1024),
	[ResponsibilityTypeList]			NVARCHAR (1024),
	--[AccountTypeCode]		NVARCHAR (255),
	[AllowSelectiveSigning]				BIT DEFAULT 0,
	[Script]							NVARCHAR (MAX) -- to store SQL code that populates the line
);