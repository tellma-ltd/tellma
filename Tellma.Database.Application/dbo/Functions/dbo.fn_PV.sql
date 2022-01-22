CREATE FUNCTION [dbo].[fn_PV] (
-- https://www.mssqltips.com/sqlservertip/2000/calculating-and-verifying-financial-values-in-sql-server/
    @as_of_date date = NULL, -- evaluate as of this date, null for today
    @monthly_rate float, -- ex 0.01 for 12%/yr
    @payment_date date,  -- Date payment scheduled
    @disbursement_or_payment CHAR(1), -- D or P
    @cash_flow_amount money
) RETURNS money
AS
BEGIN

    DECLARE @periods integer;
   
    SET @periods = DATEDIFF(MONTH,
                        ISNULL(@as_of_date, GETDATE()),
                        @payment_date
                    );
   
    RETURN
        CASE @disbursement_or_payment 
            WHEN 'D' THEN -1.0 -- disbursement
            ELSE 1.0 -- payment
        END
        * @cash_flow_amount
        / POWER (1.0 + @monthly_rate, @periods)
END;