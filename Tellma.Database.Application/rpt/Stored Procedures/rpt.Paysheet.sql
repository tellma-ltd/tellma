CREATE PROCEDURE [rpt].[Paysheet] -- [rpt].[Paysheet] 6625, '82'
	@DocumentId INT,
	@CenterCode NVARCHAR (50) = N'2'
AS
DECLARE @Year INT, @Month INT;

SELECT @Year = YEAR([PostingDate]), @Month = MONTH([PostingDate])
FROM Documents WHERE [Id] = @DocumentId;

DECLARE @EmployeeRLD INT = (SELECT [Id] FROM dbo.RelationDefinitions WHERE [Code] = N'Employee');
DECLARE @SalaryAdvance INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'DX');
DECLARE @CenterNode HIERARCHYID = (SELECT [Node] FROM dbo.[Centers] WHERE [Code] = @CenterCode);

WITH SalariesFact AS (
	SELECT [EmployeeId],
	ISNULL(-[22020490],0) AS [OtherDeductions],
	ISNULL([22020603],0) AS [PensionPayable],
	[22020605] AS [IncomeTax],
	ISNULL([22020607],0) AS [ProvidentPayable],
	[63040101] AS [Basic],
	ISNULL([63040102], 0) AS [Transportation],
	ISNULL([63040103], 0) AS [Commission],
	ISNULL([63040109], 0) AS [Deductions],
	ISNULL([63040121],0) AS [Overtime]
	FROM
	(
		SELECT E.ParticipantId AS EmployeeId, A.[Code], (E.[Direction] * E.[MonetaryValue]) AS [NetMonetaryValue]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.Documents D ON L.DocumentId = D.Id
		JOIN dbo.Relations R ON E.ParticipantId = R.[Id]
		JOIN dbo.[Accounts] A ON E.[AccountId] = A.[Id]
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		WHERE L.[State] = 4
		AND YEAR(L.[PostingDate]) = @Year
		AND MONTH(L.[PostingDate]) = @Month
		AND R.DefinitionId = @EmployeeRLD
		AND (A.[Code] <> N'22020490' OR E.[EntryTypeId] = @SalaryAdvance)
		AND (A.[Code] NOT IN ( N'22020603', N'22020605', N'22020607') OR D.[Id] = @DocumentId) -- Income tax from this document only
		AND (@CenterCode IS NULL OR C.[Node].IsDescendantOf(@CenterNode) = 1)
	) p
	PIVOT
	(
	SUM ([NetMonetaryValue])
	FOR [Code] IN 
	([22020490], [22020603], [22020605], [22020607], [63040101], [63040102],[63040103], [63040109], [63040121])
	) As pvt
	WHERE [63040101] IS NOT NULL
)
SELECT R.[Name], [Basic], [Overtime], [Deductions], [Transportation], [Commission],
		[Basic] + [Transportation] + [Commission] + [Overtime] + [Deductions] AS [Gross Salary],
		[IncomeTax] AS [Income Tax], [PensionPayable] * 7.0/18 AS [Pension 7%], [ProvidentPayable] * 7.0/18 AS [Provident 7%],
		[PensionPayable] * 11.0/18 AS [Pension 11%], [ProvidentPayable] * 11.0/18 AS [Provident 11%],
		[OtherDeductions] AS [Other Deductions],
		[IncomeTax] + [PensionPayable] * 7.0/18 + [ProvidentPayable] * 7.0/18  + [OtherDeductions] AS [Total Deductions],
		[Basic] + [Overtime] + [Deductions] + [Transportation] + [Commission] + [IncomeTax] + [PensionPayable] * 7.0/18 + 
		[ProvidentPayable] * 7.0/18 + [OtherDeductions] AS [Net Pay]
FROM SalariesFact SF
JOIN dbo.Relations R ON SF.[EmployeeId] = R.[Id]