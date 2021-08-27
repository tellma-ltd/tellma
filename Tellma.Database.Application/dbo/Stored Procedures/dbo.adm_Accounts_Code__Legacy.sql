CREATE PROCEDURE [dbo].[adm_Accounts_Code__Legacy]
	@ToLegacy BIT
AS
-- Used of the client insists to have Accounts sorted as 
-- 1: Assets
-- 2: Liabilities
-- 3: Equity
-- 4: Revenues
-- 5: Expenses
-- Though we may explain that the same can be achieved by Account Classification.
IF @ToLegacy = 1
UPDATE dbo.Accounts
	SET [Code] = CASE 
					WHEN LEFT([Code],1) = N'3'	THEN N'2' + RIGHT([Code], LEN([Code]) - 1)
					WHEN LEFT([Code],1) = N'2'	THEN N'3' + RIGHT([Code], LEN([Code]) - 1)
					WHEN LEFT([Code],2) IN (N'41', N'42') THEN [Code]
					WHEN LEFT([Code],4) = N'4301' THEN N'5301' + RIGHT([Code], LEN([Code]) - 4)
					WHEN LEFT([Code],4) = N'4302' THEN N'5302' + RIGHT([Code], LEN([Code]) - 4)
					WHEN LEFT([Code],4)
						NOT IN (N'4301', N'4302')
						AND LEFT([Code],1) = N'4' 
						THEN N'6' + RIGHT([Code], LEN([Code]) - 1)
					WHEN LEFT([Code],2) = N'51' THEN N'65' + RIGHT([Code], LEN([Code]) - 2)
					WHEN LEFT([Code],2) = N'62' THEN N'66' + RIGHT([Code], LEN([Code]) - 2)
					ELSE [Code]
				END
-- Reverse
ELSE
UPDATE A
SET A.[Code] = AC.[Code] + RIGHT(A.[Code], 2)
FROM dbo.Accounts A
JOIN dbo.AccountClassifications AC ON A.[ClassificationId] = AC.[Id]
