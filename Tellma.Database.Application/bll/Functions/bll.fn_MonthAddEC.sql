CREATE FUNCTION [bll].[fn_MonthAddEC](
	@Months Decimal (19,6),
	@FromDate DATE
)
-- TODO: The naming convention is not according to standard
-- Check if this code is duplicated in the wiz schema
RETURNS DATE
AS
BEGIN
	DECLARE @Result DATE, @ToDate DATE, @PagumeDays INT;
	DECLARE @PagumeMonths TABLE (
		[FromDate]			DATE PRIMARY KEY,
		[ToDate]			DATE
	);
	INSERT INTO @PagumeMonths VALUES
		(N'2019-09-06', N'2019-09-11'), --  N'Pagume 1-6, 2011'
		(N'2020-09-06', N'2020-09-10'), --  N'Pagume 1-5, 2012'
		(N'2021-09-06', N'2021-09-10'), --  N'Pagume 1-5, 2013'
		(N'2022-09-06', N'2022-09-10'), --  N'Pagume 1-5, 2014'
		(N'2023-09-06', N'2023-09-11'), --  N'Pagume 1-6, 2015'
		(N'2024-09-06', N'2024-09-10'), --  N'Pagume 1-5, 2016'
		(N'2025-09-06', N'2025-09-10'), --  N'Pagume 1-5, 2017'
		(N'2026-09-06', N'2026-09-10'), --  N'Pagume 1-5, 2018'
		(N'2027-09-06', N'2027-09-11'), --  N'Pagume 1-6, 2019'
		(N'2028-09-06', N'2028-09-10'), --  N'Pagume 1-5, 2020'
		(N'2029-09-06', N'2029-09-10'), --  N'Pagume 1-5, 2021'
		(N'2030-09-06', N'2030-09-10'), --  N'Pagume 1-5, 2022'
		(N'2031-09-06', N'2031-09-11'), --  N'Pagume 1-6, 2023'
		(N'2032-09-06', N'2032-09-10'), --  N'Pagume 1-5, 2024'
		(N'2033-09-06', N'2033-09-10'), --  N'Pagume 1-5, 2025'
		(N'2034-09-06', N'2034-09-10'), --  N'Pagume 1-5, 2026'
		(N'2035-09-06', N'2035-09-11'), --  N'Pagume 1-6, 2027'
		(N'2036-09-06', N'2036-09-10'), --  N'Pagume 1-5, 2028'
		(N'2037-09-06', N'2037-09-10'), --  N'Pagume 1-5, 2029'
		(N'2038-09-06', N'2038-09-10'), --  N'Pagume 1-5, 2030'
		(N'2039-09-06', N'2039-09-11'); --  N'Pagume 1-6, 2031'

	SET @ToDate = DATEADD(DAY, ROUND(30 * @Months, 0) - 1, @FromDate);
	
	SELECT @PagumeDays = SUM(1+DATEDIFF(DAY,IIF(@FromDate<[FromDate], [FromDate], @FromDate), [ToDate]))
	FROM @PagumeMonths WHERE (@ToDate >= [FromDate] AND @FromDate <= [ToDate])

	SET @Result = DATEADD(DAY,ISNULL(@PagumeDays,0),@ToDate)

	RETURN @Result;
END;
GO
