CREATE TYPE [dbo].[IFRS16ScheduleList] AS TABLE
(
	LeaseId	INT,
	PostingDate DATE,
	Payment DECIMAL (19, 6),
--	PaymentAtEndOfDate BIT,
	NumberOfPeriods INT,
	NetPresentValue DECIMAL (19, 6) DEFAULT (0),
	OpeningLiability DECIMAL (19, 6) DEFAULT (0),
	InterestExpense DECIMAL (19, 6) DEFAULT (0),
	PRIMARY KEY (LeaseId, PostingDate)
);
GO