CREATE FUNCTION [dbo].[fe_EndDateTime__Frequency_Duration_StartDateTime] (
	@Frequency		NVARCHAR (255),
	@Duration		INT,
	@StartDateTime	DATETIMEOFFSET (7)
)
RETURNS DATETIMEOFFSET (7)
AS
BEGIN
	DECLARE @Result DATETIMEOFFSET (7);
	SELECT @Result =
		CASE 
				WHEN @Frequency = N'OneTime' THEN @StartDateTime
				WHEN @Frequency = N'Daily' THEN DATEADD(DAY, @Duration, @StartDateTime)
				WHEN @Frequency = N'Weekly' THEN DATEADD(WEEK, @Duration, @StartDateTime)
				WHEN @Frequency = N'Monthly' THEN DATEADD(MONTH, @Duration, @StartDateTime)
				WHEN @Frequency = N'Quarterly' THEN DATEADD(QUARTER, @Duration, @StartDateTime)
				WHEN @Frequency = N'Yearly' THEN DATEADD(YEAR, @Duration, @StartDateTime)
		END;
	RETURN @Result;
END;