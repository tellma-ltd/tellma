CREATE PROCEDURE [rpt].[SalesInvoices__Missing]
AS
	DECLARE @CashPaymentVoucherDD INT  = (
		SELECT [Id] FROM dbo.[DocumentDefinitions]
		WHERE [Code] = 'CashPaymentVoucher'
	);

	DECLARE @IssuedInvoices TABLE (
		MachineId INT PRIMARY KEY IDENTITY (0,1),
		FromCount INT,
		ToCount INT
	)
	INSERT INTO @IssuedInvoices VALUES
	(1, 698), -- Sebeta
	(662, 3004), -- Assela
	(1357, 1523), -- HQ
	(451, 1191), -- Aman
	(1, 355);-- Gadisa

	DECLARE @InvoiceRepetitions TABLE (
		InvoiceId		INT PRIMARY KEY,
		TellmaCount		INT DEFAULT (0),
		MachineCount	INT DEFAULT (0)
	)
	INSERT INTO @InvoiceRepetitions(InvoiceId, TellmaCount)
	SELECT RIGHT(InternalReference, 6) As InvoiceNumber, COUNT(T.Id) AS Repetitions
	FROM (
	SELECT ISNULL(D.InternalReference, E.InternalReference) AS InternalReference, D.Id
	From Documents D
	LEFT Join dbo.Lines L ON L.DocumentId = D.Id 
	LEFT Join dbo.Entries E ON E.LineId = L.Id 
	WHERE D.DefinitionId <> @CashPaymentVoucherDD
	AND (L.DefinitionId IS NULL OR L.DefinitionId <> 96)
	AND ISNULL(D.InternalReference, E.InternalReference) Like N'FS%'
	GROUP BY ISNULL(D.InternalReference, E.InternalReference), D.Id
	) T
	GROUP BY  RIGHT(InternalReference, 6)
	ORDER BY  RIGHT(InternalReference, 6);

	WITH IntList AS (
		SELECT TOP (10000) InvoiceId = ROW_NUMBER() OVER (ORDER BY [object_id]) FROM sys.all_objects ORDER BY InvoiceId	
	),
	MachineRepetitions AS (
		SELECT IL.InvoiceId, COUNT(II.[MachineId]) AS MachineCount
		FROM IntList IL
		JOIN @IssuedInvoices II ON IL.InvoiceId BETWEEN II.FromCount AND II.ToCount
		GROUP BY IL.[InvoiceId]
	)
	MERGE INTO @InvoiceRepetitions AS t
	USING MachineRepetitions AS s
	ON (s.[InvoiceId] = t.[InvoiceId])
	WHEN MATCHED THEN
		UPDATE SET t.[MachineCount] = s.[MachineCount]
	WHEN NOT MATCHED THEN 
		INSERT([InvoiceId],	[MachineCount]) VALUES(s.[InvoiceId],s.[MachineCount]);
	
	SELECT N'FS' + FORMAT([InvoiceId], 'D6') AS [Invoice], [TellmaCount], [MachineCount]
	FROM @InvoiceRepetitions
	WHERE [TellmaCount] <> [MachineCount]
	ORDER BY [InvoiceId];

	SELECT SUM([MachineCount] - [TellmaCount]) AS Missing
	FROM @InvoiceRepetitions
	WHERE [TellmaCount] < [MachineCount]
/*
	DECLARE @SalesInvoice NVARCHAR(10) = N'FS00002687'
	SELECT * FROM map.Documents()
	WHERE [InternalReference] = @SalesInvoice
	OR Id IN (
		SELECT DISTINCT DocumentId FROM dbo.DocumentLineDefinitionEntries
		WHERE [InternalReference] = @SalesInvoice
	)
	OR Id IN (
		SELECT DISTINCT DocumentId FROM dbo.Lines
		WHERE [Id] IN (
			SELECT [LineId] FROM dbo.Entries
			WHERE [InternalReference] = @SalesInvoice)
	)
*/
GO
