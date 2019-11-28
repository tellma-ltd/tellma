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
		[DocumentDefinitionId],
		[SerialNumber],
		[Direction], -- direction will 
		[EntryTypeId],
		[MonetaryValue],
		[Value],
		[VoucherNumericReference] As [CPV_CRV_Ref],
		[Memo],
		[ExternalReference] As [CheckRef],
		[RelatedAgentId] As [OtherParty],
		-- TODO: where to show the related currency?
		[RelatedAmount] As [OtherPartyAmount]
	FROM [dbo].[fi_Journal](@fromDate, @toDate)
	WHERE [AccountId] = @AccountId;
END;
GO;