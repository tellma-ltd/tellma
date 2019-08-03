CREATE PROCEDURE [dbo].[rpt_Account__Statement]
-- EXEC [dbo].[rpt_Account__Statement](104, '01.01.2015', '01.01.2020')
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
		[ResourceId],
		[Quantity],
		[MoneyAmount],
		[Mass],
		-- NormalizedMass,
		[Volume], 
		-- NormalizedVolume,
		[Count],
		-- NormalizedCount,
		[Time],
		[Value],
		[Memo],
		[ExternalReference],
		[AdditionalReference]
	FROM [dbo].[fi_JournalDetails](@fromDate, @toDate)
	WHERE [AccountId] = @AccountId;
END;
GO