CREATE FUNCTION  [rpt].[Account__Statement] (
-- SELECT * FROM [rpt].[Account__Statement](104, '01.01.2015', '01.01.2020')
	@AccountId INT,
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100'
) RETURNS TABLE
AS 
RETURN
	SELECT 	
		[Id],
		[DocumentLineId],
		[DocumentDate],
		[DocumentTypeId],
		[SerialNumber],
		[Direction],
		[IfrsEntryClassificationId],
		[ResourceId],
		[MoneyAmount],
		[CurrencyId],
		[Mass],
		[MassUnitId],
		[Volume],
		[VolumeUnitId],
		[Area],
		[AreaUnitId],
		[Length],
		[LengthUnitId],
		[Time],
		[TimeUnitId],
		[Count],
		[CountUnitId],
		[Value],
		[Memo],
		[ExternalReference],
		[AdditionalReference]
	FROM [dbo].[fi_Journal](@fromDate, @toDate)
	WHERE [AccountId] = @AccountId;
GO