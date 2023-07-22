CREATE TYPE dbo.IFRS16Schedule AS TABLE (
	PaymentDate DATE PRIMARY KEY,
	Payment DECIMAL (19, 6),
	NetPresentValue DECIMAL (19, 6),
	OpeningLiability DECIMAL (19, 6),
	InterestExpense DECIMAL (19, 6)
);
GO