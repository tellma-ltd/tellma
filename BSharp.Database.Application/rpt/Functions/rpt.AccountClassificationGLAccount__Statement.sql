CREATE FUNCTION [rpt].[AccountClassificationGLAccount__Statement] (

-- SELECT * FROM [rpt].[AccountClassificationGLAccount__Statement](104, '01.01.2015', '01.01.2020')
	@AccountId INT,
	@AccountClassificationId INT,
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100'
) RETURNS TABLE
AS 
RETURN
	SELECT 	
		[Id],
		[DocumentLineId],
		[DocumentDate],
		[DocumentDefinitionId],
		[SerialNumber],
		[Direction],
		[EntryTypeId],
		[ResourceId],
		[MonetaryValue],
		[CurrencyId],
		[Mass],
		[MassUnitId],
		[Volume],
		[VolumeUnitId],
		[Time],
		[TimeUnitId],
		[Count],
		[CountUnitId],
		[Value],
		[Memo],
		[ExternalReference],
		[AdditionalReference]
	FROM [dbo].[fi_Journal](@fromDate, @toDate)
	WHERE (@AccountId IS NOT NULL AND [AccountId] = @AccountId)
	OR (@AccountClassificationId IS NOT NULL AND [AccountClassificationId] = @AccountClassificationId)
GO