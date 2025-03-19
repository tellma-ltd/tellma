CREATE TYPE dbo.[LeaseList] AS TABLE (
	LeaseId	INT PRIMARY KEY,
	YearlyDiscountRate DECIMAL (19, 6), --new or updated rate
	DiscountRate DECIMAL (19, 6), --new or updated rate
	TenancyStartDate DATE, -- Original start
	TenancyEndDate DATE, -- Original start
	UpdatedStartDate DATE, -- Date of new/updated conditions
	UpdatedEndDate DATE,
	-- IFRS Model must filled before passing the data
	IFRSModel TINYINT -- 0: Yearly, 1: Half Yearly, 2: Quarterly, 3: Monthly, 4: Daily
)
GO