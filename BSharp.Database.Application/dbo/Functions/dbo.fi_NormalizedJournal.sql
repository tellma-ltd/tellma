CREATE FUNCTION [dbo].[fi_NormalizedJournal] (
	@fromDate Date = '2000.01.01', 
	@toDate Date = '2100.01.01',
	@MassUnitId INT,
	@CountUnitId INT
) RETURNS TABLE
AS
RETURN
	WITH UnitRatios AS (
		SELECT [Id], [UnitAmount] * (SELECT [BaseAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @MassUnitId)
		/ ([BaseAmount] * (SELECT [UnitAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @MassUnitId)) As [Ratio]
		FROM dbo.MeasurementUnits
		WHERE UnitType = N'Mass'
		UNION
		SELECT [Id], [UnitAmount] * (SELECT [BaseAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @CountUnitId)
		/ ([BaseAmount] * (SELECT [UnitAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @CountUnitId))
		FROM dbo.MeasurementUnits
		WHERE UnitType = N'Count'
	)
	SELECT
		J.[Id],
		J.[DocumentLineId],
		J.[DocumentId],
		J.[DocumentDate],
		J.[SerialNumber],
		J.[VoucherNumericReference],
		J.[DocumentDefinitionId],
		J.[LineDefinitionId],
		J.[Direction],
		J.[AccountId],
		J.[AccountTypeId],
		J.[AgentId],
		J.[EntryTypeId],
		J.[ResponsibilityCenterId],
		J.[ResourceId],
		J.[BatchCode],
		J.[MonetaryValue],
		J.[MonetaryValueCurrencyId],
		J.[Mass] * ISNULL(MR.[Ratio], 0) AS [Mass],
		J.[Volume],
		J.[Area],
		J.[Length],
		J.[Time],
		J.[Count] * ISNULL(CR.[Ratio], 0) AS [Count],
		J.[Value],
		J.[Memo],
		J.[ExternalReference],
		J.[AdditionalReference],
		J.[RelatedResourceId],
		J.[RelatedAgentId],
		J.[RelatedQuantity],
		J.[RelatedMonetaryValue]
	FROM dbo.fi_Journal(@fromDate, @toDate) J
	LEFT JOIN dbo.Resources R ON J.ResourceId = R.Id
	LEFT JOIN UnitRatios MR ON R.MassUnitId = MR.Id
	LEFT JOIN UnitRatios CR ON R.CountUnitId = CR.Id
