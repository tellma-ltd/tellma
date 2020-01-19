CREATE PROCEDURE [dbo].[rpt_BankAccount__Statement]
-- EXEC [dbo].[rpt_BankAccount__Statement](104, '01.01.2015', '01.01.2020')
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100',
	@AccountId INT
AS
BEGIN
	SELECT 	
		[Id],
		[LineId],
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
	FROM [map].[DetailsEntries](@fromDate, @toDate, NULL, NULL, NULL)
	WHERE [AccountId] = @AccountId;
END;
GO;