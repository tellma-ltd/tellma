CREATE FUNCTION [dal].[fn_Document__SupplyEndDate] (
	@DocumentId INT,
	@InvoiceId INT
)
RETURNS DATE
AS
BEGIN
	DECLARE @ZatcaDocumentType NVARCHAR (3) = dal.fn_Document__ZatcaDocumentType(@DocumentId)
	RETURN CASE
	WHEN @ZatcaDocumentType = N'388' -- Invoice
	THEN (
		SELECT MAX(E.[Time2])
		FROM dbo.Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.[DocumentId] = @DocumentId
		AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
	)
	WHEN @ZatcaDocumentType IN (N'381', N'383')
	THEN (
		SELECT MAX(E.[Time2])
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		WHERE D.[NotedAgentId] = @InvoiceId
		AND D.[State] = 1
		AND DD.[ZatcaDocumentType] = N'388'
	) END;
END
GO