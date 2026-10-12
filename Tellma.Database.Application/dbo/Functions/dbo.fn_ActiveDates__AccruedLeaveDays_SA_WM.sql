CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_SA_WM]
(
  @FromDate    DATE,
  @ToDate      DATE,
  @YearlyAccrual  INT = 21,
  @InactiveDays  INT = 0,
  @Citizenship  INT
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
  DECLARE @SaudiStaffNewRuleEffectiveDate DATE = '20260630';
  DECLARE @Result DECIMAL (19, 6) = 0;
  DECLARE @SaudiCitizenship INT = dal.fn_LookupDefinition_Code__Id (N'Citizenship',N'SAU');
  SET @FromDate = DATEADD(DAY, @InactiveDays, @FromDate);  

  DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
  DECLARE @YearsInPhase1 INT = 5, @DaysInPhase2 INT = 30;
  DECLARE @FullYears INT;
  DECLARE @FullMonths INT;
  DECLARE @FullDays INT;

  -- NON Saudi and Saudi who left before July 1st remain same as before
  IF @Citizenship <> @SaudiCitizenship OR (@Citizenship = @SaudiCitizenship AND @ToDate <= @SaudiStaffNewRuleEffectiveDate)
  BEGIN
    SET @FullYears = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
    SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
    SET @FullMonths = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
    SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
    SET @FullDays =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 

    SELECT @Result =  IIF(
      -- If employee has been with company more than 5 years
        @FullYears > = @YearsInPhase1,
      -- he deserves @YearlyAccrual days per year, for the first @YearsInPhase1, then @DaysInPhase2 for each additional
        @DaysInPhase2 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0) - (@DaysInPhase2 - @YearlyAccrual) * @YearsInPhase1,
        @YearlyAccrual * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0)
      )
  END
  ELSE IF @Citizenship = @SaudiCitizenship
  IF @FromDate > @SaudiStaffNewRuleEffectiveDate -- After 2026 July 1st All Saudi will get 30 days
  BEGIN
    SET @FullYears = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
    SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
    SET @FullMonths = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
    SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
    SET @FullDays =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 

    SELECT @Result = @DaysInPhase2 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0)
  END
  ELSE IF @FromDate <= @SaudiStaffNewRuleEffectiveDate  -- Saudi Join date Before July 1st
  BEGIN
    SET @FullYears = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @SaudiStaffNewRuleEffectiveDate);   
    SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
    SET @FullMonths = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @SaudiStaffNewRuleEffectiveDate); 
    SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
    SET @FullDays =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @SaudiStaffNewRuleEffectiveDate); 

    -- Before 2026 July 1st remain same as before
    SELECT @Result = IIF(
      -- If employee has been with company more than 5 years
        @FullYears > = @YearsInPhase1,
      -- he deserves @YearlyAccrual days per year, for the first @YearsInPhase1, then @DaysInPhase2 for each additional
        @DaysInPhase2 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0) - (@DaysInPhase2 - @YearlyAccrual) * @YearsInPhase1,
        @YearlyAccrual * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0))

    -- After 2026 July 1st All Saudi will get 30 days
    SET @FromDate = DATEADD(DAY, 1, @SaudiStaffNewRuleEffectiveDate);
    SET @FullYears = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
    SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
    SET @FullMonths = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
    SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
    SET @FullDays =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 

    SELECT @Result = @Result + (@DaysInPhase2 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0))
  END

  RETURN   @Result;
END
