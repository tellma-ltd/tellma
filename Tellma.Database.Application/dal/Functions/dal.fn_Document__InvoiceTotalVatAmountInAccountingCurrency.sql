﻿CREATE FUNCTION dal.[fn_Document__InvoiceTotalVatAmountInAccountingCurrency](
	@DocumentId INT
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	RETURN ISNULL(
	(
		SELECT -SUM(E.[Direction] * E.[Value])
		FROM dbo.Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
		WHERE L.[DocumentId] = @DocumentId
		AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
		AND NR.[Code] NOT LIKE 'Prepayment%'
	), 0)
END
GO