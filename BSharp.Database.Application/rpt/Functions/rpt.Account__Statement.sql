CREATE FUNCTION  [rpt].[Account__Statement] (
-- SELECT * FROM [rpt].[Account__Statement](104, '01.01.2015', '01.01.2020')
	@AccountId INT,
	@AgentRelationDefinitionId NVARCHAR (50),
	@AgentId INT,
	@ResourceId INT,
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
	WHERE [AccountId] = @AccountId
	AND (@AgentId IS NULL OR AgentId = @AgentId)
	AND (@ResourceId IS NULL OR [ResourceId] = @ResourceId)
	
	
	;
GO