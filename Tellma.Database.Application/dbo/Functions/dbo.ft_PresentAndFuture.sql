CREATE FUNCTION [dbo].[ft_PresentAndFuture] (
	@PostingDate DATE,
	@DaysTotal DECIMAL,
	@StartDate DATE
)
RETURNS @MyResult TABLE (
	EndDate DATE,
	DaysPresent DECIMAL, DaysFuture DECIMAL,
	StartDatePresent DATE, EndDatePresent DATE,
	StartDateFuture DATE, EndDateFuture DATE
)
AS BEGIN
	Declare
		@EndDate DATE, @StartDateFuture DATE, @EndDateFuture DATE,
		@StartDatePresent DATE, @EndDatePresent DATE, 
		@DaysPresent INT, @DaysFuture INT;

	SET @EndDate = DATEADD(DAY, @DaysTotal, @StartDate);
	SET @DaysPresent = DATEDIFF(DAY, @StartDate, EOMONTH(@PostingDate))

	SET @DaysPresent = IIF(@DaysPresent < 0, 0, IIF(@DaysPresent > @DaysTotal, @DaysTotal, @DaysPresent));
	SET @DaysFuture = IIF(@DaysTotal <= @DaysPresent, 0, @DaysTotal - @DaysPresent);

	SET @StartDatePresent = IIF(@StartDate < EOMONTH(@PostingDate), 
				@StartDate , EOMONTH(@PostingDate))
	SET @EndDatePresent = DATEADD(DAY, @DaysPresent, @StartDatePresent)

	SET @StartDateFuture = IIF(@StartDate > EOMONTH(@PostingDate), 
				@StartDate , DATEADD(DAY, 1, EOMONTH(@PostingDate)));
	SET @EndDateFuture = DATEADD(DAY, @DaysFuture, @StartDateFuture)
	INSERT INTO @MyResult VALUES(@EndDate, @DaysPresent, @DaysFuture, @StartDatePresent, @EndDatePresent,
								@StartDateFuture, @EndDateFuture);
	RETURN
END
GO