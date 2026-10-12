CREATE FUNCTION [dbo].[fn_UnitCode__UneceRec20] (
    @Code NVARCHAR (50)
)
RETURNS NVARCHAR (3)
AS
BEGIN
    RETURN CASE @Code
        -- Pure / Count
        WHEN N'pure'     THEN N'C62' -- one (dimensionless)
        WHEN N'ea'       THEN N'EA'  -- each
        WHEN N'pcs'      THEN N'H87' -- piece
        WHEN N'share'    THEN N'E21' -- share
        WHEN N'trip'     THEN N'E54' -- trip
        WHEN N'manmonth' THEN N'3C'  -- manmonth
        -- Time
        WHEN N's'        THEN N'SEC' -- second
        WHEN N'min'      THEN N'MIN' -- minute
        WHEN N'hr'       THEN N'HUR' -- hour
        WHEN N'd'        THEN N'DAY' -- day
        WHEN N'wk'       THEN N'WEE' -- week
        WHEN N'mo'       THEN N'MON' -- month
        WHEN N'yr'       THEN N'ANN' -- year
        WHEN N'wd'       THEN N'E49' -- working day
        WHEN N'wmo'      THEN N'WM'  -- working month
        -- Mass
        WHEN N'g'        THEN N'GRM' -- gram
        WHEN N'kg'       THEN N'KGM' -- kilogram
        WHEN N'qn100'    THEN N'DTN' -- decitonne (metric quintal, 100 kg)
        WHEN N'mt'       THEN N'TNE' -- tonne (metric ton)
        -- Volume
        WHEN N'ltr'      THEN N'LTR' -- litre
        -- Distance
        WHEN N'cm'       THEN N'CMT' -- centimetre
        WHEN N'm'        THEN N'MTR' -- metre
        WHEN N'km'       THEN N'KMT' -- kilometre
        -- No Rec 20 equivalent: emo, wwk, wyr, qn50, uq-yr, hmo, manhour
        ELSE @Code
    END;
END;
GO
