CREATE FUNCTION [rpt].[Account__Statement] (
-- SELECT * FROM [rpt].[Account__Statement]('01.01.2015', '01.01.2020', 104, NULL, NULL)
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100',
	@AccountId INT,
	@ResponsibilityCenterId INT,
	@AgentId INT,
	@ResourceId INT
) RETURNS TABLE
AS 
RETURN
	SELECT 	
		[Id],
		[LineId],
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
	FROM [map].[DetailsEntries](@fromDate, @toDate, NULL, NULL, NULL)
	WHERE
		(@AccountId					IS NULL	OR [AccountId]				= @AccountId)
	AND (@ResponsibilityCenterId	IS NULL	OR [ResponsibilityCenterId] = @ResponsibilityCenterId)
	AND (@AgentId					IS NULL	OR [AgentId]				= @AgentId)
	AND (@ResourceId				IS NULL	OR [ResourceId]				= @ResourceId)
GO