CREATE FUNCTION [map].[DetailsEntries] ()
	RETURNS TABLE
	AS
	RETURN
	SELECT
		E.*,
		IIF(EU.UnitType = N'Pure',
			E.[Quantity],
			CAST(
				E.[Quantity] -- Quantity in E.UnitId
			*	EU.[BaseAmount] / EU.[UnitAmount] -- Quantity in Standard Unit of that type
			*	RBU.[UnitAmount] / RBU.[BaseAmount]
				AS DECIMAL (19,4)
			)
		) As [BaseQuantity],-- Quantity in Base unit of that resource
		R.[UnitId] AS [BaseUnitId]
	FROM dbo.[Entries] E
	LEFT JOIN dbo.[Resources] R ON E.[ResourceId] = R.[Id]
	LEFT JOIN dbo.[Units] EU ON E.[UnitId] = EU.[Id]
	LEFT JOIN dbo.[Units] RBU ON R.[UnitId] = RBU.[Id]