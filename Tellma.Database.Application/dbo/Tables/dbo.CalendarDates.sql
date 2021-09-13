CREATE TABLE [dbo].[CalendarDates]
(
	[GCDate]		DATE CONSTRAINT [PK_dbo.CalendarDates] PRIMARY KEY,
	[ETDateYear]	SMALLINT,
	[ETDateQuarter]	INT,
	[ETDateMonth]	INT,
	[ETDateDay]		INT,
	[UQDateYear]	SMALLINT,
	[UQDateQuarter]	INT,
	[UQDateMonth]	INT,
	[UQDateDay]		INT,
);
GO
CREATE INDEX IX_CalendarDates__ETDateYear ON dbo.CalendarDates([ETDateYear]);
GO
CREATE INDEX IX_CalendarDates__ETDateQuarter ON dbo.CalendarDates([ETDateQuarter]);
GO
CREATE INDEX IX_CalendarDates__ETDateMonth ON dbo.CalendarDates([ETDateMonth]);
GO
CREATE INDEX IX_CalendarDates__ETDateDay ON dbo.CalendarDates([ETDateDay]);
GO
CREATE INDEX IX_CalendarDates__UQDateYear ON dbo.CalendarDates([UQDateYear]);
GO
CREATE INDEX IX_CalendarDates__UQDateQuarter ON dbo.CalendarDates([UQDateQuarter]);
GO
CREATE INDEX IX_CalendarDates__UQDateMonth ON dbo.CalendarDates([UQDateMonth]);
GO
CREATE INDEX IX_CalendarDates__UQDateDay ON dbo.CalendarDates([UQDateDay]);
GO
