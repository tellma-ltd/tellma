CREATE PROCEDURE [dbo].[rpt_BankAccount__Statement]
-- EXEC [dbo].[rpt_BankAccount__Statement](104, '01.01.2015', '01.01.2020')
	@AccountId INT,
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100'
AS
BEGIN
	SELECT 	
		[Id],
		[DocumentLineId],
		[DocumentDate],
		[DocumentTypeId],
		[SerialNumber],
		[Direction],
		[IfrsNoteId],
		[ResponsibilityCenterId],
		[MoneyAmount],
		[Value],
		[VoucherNumericReference] As [CPV_CRV_Ref],
		[Memo],
		[ExternalReference] As [CheckRef],
		[RelatedResourceId] As [OtherPartyCurrency],
		[RelatedAccountId] As [OtherParty],
		[RelatedMoneyAmount] As [OtherPartyAmount]
	FROM [dbo].[fi_Journal](@fromDate, @toDate)
	WHERE [AccountId] = @AccountId;
END;
GO;