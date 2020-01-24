CREATE TABLE [dbo].[TemplateLines] (
	[Id]					INT PRIMARY KEY,
	[DocumentId]			INT	NOT NULL,
	[TemplateLineType]		NVARCHAR (255)		NOT NULL,
	[ValidFrom]				DATETIME2(7)		NOT NULL DEFAULT (CONVERT (date, SYSDATETIME())),
	-- for sales/purchase price lists
	[ResourceId]			INT,
	[Quantity]				DECIMAL (19,4)				DEFAULT 1,
	[Price]					DECIMAL (19,4),
	[Currency]				INT,
	[VAT]					DECIMAL (19,4),
	[TOT]					DECIMAL (19,4),
	-- for employee agreement
	[AgentId]				INT,
	[MonthlyBasicSalary]	DECIMAL (19,4),
	[HourlyOvertimeRate]	DECIMAL (19,4),
	[DailyPerDiem]			DECIMAL (19,4),

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
)
GO
CREATE INDEX [IX_TemplateLines__DocumentId] ON [dbo].[TemplateLines]([DocumentId]);
GO