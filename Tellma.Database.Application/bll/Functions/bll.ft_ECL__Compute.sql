CREATE FUNCTION [bll].[ft_ECL__Compute] (
	@AccountTypeParentConcept NVARCHAR (255) = N'OtherCurrentFinancialAssets',
	@AsOfDate DATE = '2022-9-30'
)
RETURNS @MyResult TABLE(ECL DECIMAL (19, 4), CurrencyId NCHAR (3))
AS BEGIN
	DECLARE @Result DECIMAL (19,4);
	DECLARE @AccountTypeParentNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@AccountTypeParentConcept);

	WITH ECLAccounts AS (
			SELECT A.[Id]
			FROM dbo.Accounts A
			JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
			WHERE AC.[Node].IsDescendantOf(@AccountTypeParentNode) = 1
		)
	INSERT INTO @MyResult
	SELECT ROUND(SUM(ECL), 2), [CurrencyId]
	FROM 
	(SELECT E.[NotedDate], E.[CurrencyId],
			SUM(E.[Direction] * E.[MonetaryValue]) *
			CASE
				WHEN E.[NotedDate] > @AsOfDate THEN 0.01
				WHEN E.[NotedDate] > DATEADD(MONTH, -3, @AsOfDate) THEN 0.25
				WHEN E.[NotedDate] > DATEADD(MONTH, -6, @AsOfDate) THEN 0.75
				ELSE 1
			END AS ECL
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
		JOIN dbo.AgentDefinitions AGD ON AGD.[Id] = AG.[DefinitionId]
		WHERE L.[State] = 4
		AND E.[AccountId] IN (SELECT [Id] FROM ECLAccounts)
		AND AGD.[Code] <> N'Employee'
		AND L.[PostingDate] <= @AsOfDate
		GROUP BY E.[NotedDate],  E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	) T GROUP BY [CurrencyId];
	RETURN
END
GO