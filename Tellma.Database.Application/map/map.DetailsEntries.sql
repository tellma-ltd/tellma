﻿CREATE FUNCTION [map].[DetailsEntries] ()
	RETURNS TABLE
	AS
	RETURN
	SELECT
		E.[Id],
		E.[LineId],
		E.[Index],
		E.[Direction],
		E.[AccountId],
		E.[CurrencyId],
		E.[AgentId],	
		E.[NotedAgentId],
		E.[ResourceId],	
		E.[CenterId],	
		E.[EntryTypeId],
		E.[MonetaryValue],
		E.[Quantity],
		E.[UnitId],
		E.[Value],
		E.[RValue],
		E.[PValue],
		E.[Time1],
		E.[Duration],
		E.[DurationUnitId],		
		E.[Time2],		
		E.[ExternalReference],
		E.[ReferenceSourceId],
		E.[InternalReference],
		E.[NotedAgentName],
		E.[NotedAmount],
		E.[NotedDate],
		E.[NotedResourceId],
		E.[CreatedAt],	
		E.[CreatedById],
		E.[ModifiedAt],
		E.[ModifiedById],
		IIF(EU.UnitType = N'Pure',
			E.[Quantity],
			CAST(
				E.[Quantity] -- Quantity in E.UnitId
			*	EU.[BaseAmount] / EU.[UnitAmount] -- Quantity in Standard Unit of that type
			*	ISNULL(RBU.[UnitAmount] / RBU.[BaseAmount], 1) -- When only qty but not resource, such as PPE
				AS DECIMAL (19,4)
			)
		) As [BaseQuantity],-- Quantity in Base unit of that resource
		R.[UnitId] AS [BaseUnitId]
	FROM dbo.[Entries] E
	LEFT JOIN dbo.[Resources] R ON E.[ResourceId] = R.[Id]
	LEFT JOIN dbo.[Units] EU ON E.[UnitId] = EU.[Id]
	LEFT JOIN dbo.[Units] RBU ON R.[UnitId] = RBU.[Id];