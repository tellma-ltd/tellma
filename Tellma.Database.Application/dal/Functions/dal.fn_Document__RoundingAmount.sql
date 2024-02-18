CREATE FUNCTION dal.fn_Document__RoundingAmount(@DocumentId INT)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	IF dal.fn_Document__ZatcaDocumentType(@DocumentId) IN (N'381', N'383', N'388') -- Credit Note, Debit Note, Tax Invoice
	RETURN ISNULL(
	(
		SELECT SUM(E.[Direction] * E.[Value])
		FROM dbo.Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
		WHERE L.[DocumentId] = @DocumentId
		AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
		AND R.[Code] LIKE 'Rounding%'
	), 0)
	RETURN 0; -- 386: Prepayment invoice 
END
GO