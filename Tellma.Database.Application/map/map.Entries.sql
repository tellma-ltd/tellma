CREATE FUNCTION [map].[Entries]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[LineId],
		[Index],
		[Direction],
		[AccountId],
		[CurrencyId],
		[AgentId],
		[NotedAgentId],
		[ResourceId],
		[CenterId],
		[EntryTypeId],
		[MonetaryValue],
		[Quantity],
		[UnitId],
		[Value],
		[RValue],
		[PValue],
		[Time1],
		[Duration],
		[DurationUnitId],
		[Time2],
		[ExternalReference],
		[ReferenceSourceId],
		[InternalReference],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate],
		[NotedResourceId],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM dbo.Entries
);
