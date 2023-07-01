CREATE TYPE dbo.DatedAmountList AS TABLE (
	AmountDate DATE PRIMARY KEY,
	Amount DECIMAL (19, 6)
);
GO