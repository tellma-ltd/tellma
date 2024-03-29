﻿CREATE FUNCTION [dal].[fn_Document__SupplyDate] (
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
		SELECT MIN(E.[Time1])
		FROM dbo.Entries E
		INNER JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
		INNER JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = NR.[DefinitionId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.[DocumentId] = @DocumentId
		AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
		AND NRD.[Code] <> 'Discounts'
		AND E.[Direction] = -1
	)
	WHEN @ZatcaDocumentType IN (N'381', N'383')
	THEN (
		SELECT MIN(E.[Time1])
		FROM dbo.Entries E
		INNER JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
		INNER JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = NR.[DefinitionId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		WHERE D.[NotedAgentId] = @InvoiceId
		AND D.[State] = 1
		AND DD.[ZatcaDocumentType] = N'388'
		AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
		AND NRD.[Code] <> 'Discounts'
		AND E.[Direction] = -1
	) END;
END
GO