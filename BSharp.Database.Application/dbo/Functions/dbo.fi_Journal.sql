CREATE FUNCTION [dbo].[fi_Journal] (-- SELECT * FROM [dbo].[fi_Journal]('01.01.2015','01.01.2020')
	@fromDate Date = '2000.01.01',
	--@param int = 0,
	@toDate Date = '2100.01.01'
) RETURNS TABLE
AS
RETURN
	WITH
	IntegerList AS ( -- can be defined recursively, or simply read from a table
		SELECT 0 As I UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION
		SELECT 5 As I UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9
	)
	SELECT
		V.[Id],
		V.[LineId],
		V.[DocumentId],
		V.[DocumentDefinitionId],
		V.[SerialNumber],
		V.[DocumentDate],
		V.[VoucherNumericReference],
		V.[DocumentLookup1Id],
		V.[DocumentLookup2Id],
		V.[DocumentLookup3Id],
		V.[DocumentText1],
		V.[DocumentText2],
		--D.[Frequency],
		--D.[Repetitions],
		--D.[EndDate],
		V.[LineDefinitionId],
		V.[Memo],
		V.[EntryNumber],
		V.[Direction],
		A.[AccountClassificationId],
		V.[AccountId],		
		V.[ContractType],
		V.[AgentDefinitionId],
		V.[ResourceClassificationId],
		V.[IsCurrent],		
		V.[CurrencyId],
		V.[AgentId],
		V.[ResourceId],
		V.[ResponsibilityCenterId],
		V.[AccountDescriptorId],
		V.[ResourceDescriptorId],
		V.[DueDate],		
		V.[EntryClassificationId],
		V.[MonetaryValue], -- normalization is already done in the Value and stored in the entry
		R.[CountUnitId],
		V.[Count], -- we can normalize every measure, but just showing a proof of concept
		R.[MassUnitId],
		V.[Mass],
		R.[VolumeUnitId],
		V.[Volume],
		R.[TimeUnitId],
		V.[Time],
		V.[Value],
		V.[ExternalReference],
		V.[AdditionalReference],
		V.[RelatedAgentId],
		V.[RelatedAmount],
		V.[Time1],
		V.[Time2],
		V.[CreatedAt],
		V.[CreatedById],
		V.[ModifiedAt],
		V.[ModifiedById]
	FROM dbo.[DocumentLineEntriesDetailsView] V
	JOIN dbo.Accounts A ON V.AccountId = A.Id
	LEFT JOIN dbo.Resources R ON V.ResourceId = R.Id
	--LEFT JOIN dbo.MeasurementUnits MU ON R.MassUnitId = MU.Id
	--LEFT JOIN dbo.MeasurementUnits VU ON R.VolumeUnitId = VU.Id
	--LEFT JOIN dbo.MeasurementUnits CU ON R.CountUnitId = CU.Id
	--LEFT JOIN dbo.Resources RR ON V.RelatedResourceId = RR.Id
	WHERE --V.[Frequency]		= N'OneTime' AND 
	(@fromDate IS NULL OR [DocumentDate] >= @fromDate)
	AND (@toDate IS NULL OR [DocumentDate] < DATEADD(DAY, 1, @toDate))
	--AND [DocumentDate] <
	--	CASE 
	--		WHEN @Param = 0 THEN @toDate
	--		WHEN @Param = 1 THEN DATEADD(DAY, 7, @fromDate)
	--		WHEN @Param = 2 THEN DATEADD(MONTH, 1, @fromDate)
	--	END
/* TODO: Uncomment when stabilized.
	UNION ALL
	SELECT
		V.[Id],
		V.[DocumentId],
		CONVERT(DATE, V.[DocumentDate]) AS DocumentDate,
		V.[SerialNumber],
		V.[VoucherReference],
		V.[DocumentType],
		V.[Direction],
		V.[AccountId],
		V.[IfrsAccountId],
		V.[IfrsEntryClassificationId],
		V.[ResponsibilityCenterId],
		V.[AgentAccountId],
		V.[ResourceId],
		V.[Quantity],
		V.[MoneyAmount],
		V.[Mass],
		V.[Mass] * MU.[BaseAmount] / MU.[UnitAmount] As NormalizedMass,
		V.[Volume], 
		V.[Volume] * VU.[BaseAmount] / VU.[UnitAmount] As NormalizedVolume,
		V.[Count],
		V.[Volume] * CU.[BaseAmount] / CU.[UnitAmount] As NormalizedCount,
		V.[Time],
		V.[Value],
		V.[ExpectedSettlingDate],
		V.[Memo],
		-- Not happy about COALESCE, but 
		-- In case of financial instruments, the external reference is a property of the Related Resource
		COALESCE(RR.[Reference], V.[ExternalReference]) AS [RelatedReference],
		V.[RelatedResourceId],
		V.[RelatedAgentAccountId],
		V.[RelatedResponsibilityCenterId],
		V.[RelatedQuantity],
		V.[RelatedMoneyAmount],
		V.[RelatedMass],
		V.[RelatedVolume],
		V.[RelatedCount],
		V.[RelatedTime],
		V.[RelatedValue]
	FROM dbo.[TransactionEntriesView] V
	CROSS JOIN IntegerList IL
	JOIN dbo.Resources R ON V.ResourceId = R.Id
	JOIN dbo.MeasurementUnits MU ON R.MassUnitId = MU.Id
	JOIN dbo.MeasurementUnits VU ON R.VolumeUnitId = VU.Id
	JOIN dbo.MeasurementUnits CU ON R.CountUnitId = CU.Id
	JOIN dbo.Resources RR ON V.RelatedResourceId = RR.Id
	WHERE V.[Frequency]		= N'Daily'
	AND (@fromDate IS NULL OR [DocumentDate] >= @fromDate)
	AND (@toDate IS NULL OR DATEADD(DAY, IL.I, [DocumentDate]) < @toDate)
	AND ([EndDate] IS NULL OR DATEADD(DAY, IL.I, [DocumentDate]) < [EndDate])

	UNION ALL
	SELECT 
		V.[Id],
		V.[DocumentId],
		CONVERT(DATE, V.[DocumentDate]) AS DocumentDate,
		V.[SerialNumber],
		V.[VoucherReference],
		V.[DocumentType],
		V.[Direction],
		V.[AccountId],
		V.[IfrsAccountId],
		V.[IfrsEntryClassificationId],
		V.[ResponsibilityCenterId],
		-- [OperationId],
		-- [ProductCategoryId],
		-- [GeographicRegionId],
		-- [CustomerSegmentId],
		-- [TaxSegmentId],
		V.[AgentAccountId],
		V.[ResourceId],
		V.[Quantity],
		V.[MoneyAmount],
		V.[Mass],
		V.[Mass] * MU.[BaseAmount] / MU.[UnitAmount] As NormalizedMass,
		V.[Volume], 
		V.[Volume] * VU.[BaseAmount] / VU.[UnitAmount] As NormalizedVolume,
		V.[Count],
		V.[Volume] * CU.[BaseAmount] / CU.[UnitAmount] As NormalizedCount,
		V.[Time],
		V.[Value],
		V.[ExpectedSettlingDate],
		V.[Reference],
		V.[Memo],
		COALESCE(RR.[Reference], V.[RelatedReference]) AS [RelatedReference],
		V.[RelatedResourceId],
		V.[RelatedAgentAccountId],
		V.[RelatedResponsibilityCenterId],
		V.[RelatedQuantity],
		V.[RelatedMoneyAmount],
		V.[RelatedMass],
		V.[RelatedVolume],
		V.[RelatedCount],
		V.[RelatedTime],
		V.[RelatedValue]
	FROM dbo.[TransactionEntriesView] V
	CROSS JOIN IntegerList IL
	JOIN dbo.Resources R ON V.ResourceId = R.Id
	JOIN dbo.MeasurementUnits MU ON R.MassUnitId = MU.Id
	JOIN dbo.MeasurementUnits VU ON R.VolumeUnitId = VU.Id
	JOIN dbo.MeasurementUnits CU ON R.CountUnitId = CU.Id
	JOIN dbo.Resources RR ON V.RelatedResourceId = RR.Id
	WHERE V.[Frequency]		= N'Weekly'
	AND (@fromDate IS NULL OR [DocumentDate] >= @fromDate)
	AND (@toDate IS NULL OR DATEADD(WEEK, IL.I, [DocumentDate]) < @toDate)
	AND ([EndDate] IS NULL OR DATEADD(WEEK, IL.I, [DocumentDate]) < [EndDate])

	UNION ALL
	SELECT 
		V.[Id],
		V.[DocumentId],
		CONVERT(DATE, V.[DocumentDate]) AS DocumentDate,
		V.[SerialNumber],
		V.[VoucherReference],
		V.[DocumentType],
		V.[Direction],
		V.[AccountId],
		V.[IfrsAccountId],
		V.[IfrsEntryClassificationId],
		V.[ResponsibilityCenterId],
		-- [OperationId],
		-- [ProductCategoryId],
		-- [GeographicRegionId],
		-- [CustomerSegmentId],
		-- [TaxSegmentId],
		V.[AgentAccountId],
		V.[ResourceId],
		V.[Quantity],
		V.[MoneyAmount],
		V.[Mass],
		V.[Mass] * MU.[BaseAmount] / MU.[UnitAmount] As NormalizedMass,
		V.[Volume], 
		V.[Volume] * VU.[BaseAmount] / VU.[UnitAmount] As NormalizedVolume,
		V.[Count],
		V.[Volume] * CU.[BaseAmount] / CU.[UnitAmount] As NormalizedCount,
		V.[Time],
		V.[Value],
		V.[ExpectedSettlingDate],
		V.[Reference],
		V.[Memo],
		COALESCE(RR.[Reference], V.[RelatedReference]) AS [RelatedReference],
		V.[RelatedResourceId],
		V.[RelatedAgentAccountId],
		V.[RelatedResponsibilityCenterId],
		V.[RelatedQuantity],
		V.[RelatedMoneyAmount],
		V.[RelatedMass],
		V.[RelatedVolume],
		V.[RelatedCount],
		V.[RelatedTime],
		V.[RelatedValue]
	FROM dbo.[TransactionEntriesView] V
	CROSS JOIN IntegerList IL
	JOIN dbo.Resources R ON V.ResourceId = R.Id
	JOIN dbo.MeasurementUnits MU ON R.MassUnitId = MU.Id
	JOIN dbo.MeasurementUnits VU ON R.VolumeUnitId = VU.Id
	JOIN dbo.MeasurementUnits CU ON R.CountUnitId = CU.Id
	JOIN dbo.Resources RR ON V.RelatedResourceId = RR.Id
	WHERE V.[Frequency]		= N'Monthly'
	AND (@fromDate IS NULL OR [DocumentDate] >= @fromDate)
	AND (@toDate IS NULL OR DATEADD(MONTH, IL.I, [DocumentDate]) < @toDate)
	AND ([EndDate] IS NULL OR DATEADD(MONTH, IL.I, [DocumentDate]) < [EndDate])
	
	UNION ALL
	SELECT
		V.[Id],
		V.[DocumentId],
		CONVERT(DATE, V.[DocumentDate]) AS DocumentDate,
		V.[SerialNumber],
		V.[VoucherReference],
		V.[DocumentType],
		V.[IsSystem],
		V.[Direction],
		V.[AccountId],
		V.[IfrsAccountId],
		V.[IfrsEntryClassificationId],
		V.[ResponsibilityCenterId],
		-- [OperationId],
		-- [ProductCategoryId],
		-- [GeographicRegionId],
		-- [CustomerSegmentId],
		-- [TaxSegmentId],
		V.[AgentAccountId],
		V.[ResourceId],
		V.[Quantity],
		V.[MoneyAmount],
		V.[Mass],
		V.[Mass] * MU.[BaseAmount] / MU.[UnitAmount] As NormalizedMass,
		V.[Volume], 
		V.[Volume] * VU.[BaseAmount] / VU.[UnitAmount] As NormalizedVolume,
		V.[Count],
		V.[Volume] * CU.[BaseAmount] / CU.[UnitAmount] As NormalizedCount,
		V.[Time],
		V.[Value],
		V.[ExpectedSettlingDate],
		V.[Reference],
		V.[Memo],
		COALESCE(RR.[Reference], V.[RelatedReference]) AS [RelatedReference],
		V.[RelatedResourceId],
		V.[RelatedAgentAccountId],
		V.[RelatedResponsibilityCenterId],
		V.[RelatedQuantity],
		V.[RelatedMoneyAmount],
		V.[RelatedMass],
		V.[RelatedVolume],
		V.[RelatedCount],
		V.[RelatedTime],
		V.[RelatedValue]
	FROM dbo.[TransactionEntriesView] V
	CROSS JOIN IntegerList IL
	JOIN dbo.Resources R ON V.ResourceId = R.Id
	JOIN dbo.MeasurementUnits MU ON R.MassUnitId = MU.Id
	JOIN dbo.MeasurementUnits VU ON R.VolumeUnitId = VU.Id
	JOIN dbo.MeasurementUnits CU ON R.CountUnitId = CU.Id
	JOIN dbo.Resources RR ON V.RelatedResourceId = RR.Id
	WHERE V.[Frequency]		= N'Quarterly'
	AND (@fromDate IS NULL OR [DocumentDate] >= @fromDate)
	AND (@toDate IS NULL OR DATEADD(QUARTER, IL.I, [DocumentDate]) < @toDate)
	AND ([EndDate] IS NULL OR DATEADD(QUARTER, IL.I, [DocumentDate]) < [EndDate])

	UNION ALL
	SELECT 
		V.[Id],
		V.[DocumentId],
		CONVERT(DATE, V.[DocumentDate]) AS DocumentDate,
		V.[SerialNumber],
		V.[VoucherReference],
		V.[DocumentType],
		V.[IsSystem],
		V.[Direction],
		V.[AccountId],
		V.[IfrsAccountId],
		V.[IfrsEntryClassificationId],
		V.[ResponsibilityCenterId],
		-- [OperationId],
		-- [ProductCategoryId],
		-- [GeographicRegionId],
		-- [CustomerSegmentId],
		-- [TaxSegmentId],
		V.[AgentAccountId],
		V.[ResourceId],
		V.[Quantity],
		V.[MoneyAmount],
		V.[Mass],
		V.[Mass] * MU.[BaseAmount] / MU.[UnitAmount] As NormalizedMass,
		V.[Volume], 
		V.[Volume] * VU.[BaseAmount] / VU.[UnitAmount] As NormalizedVolume,
		V.[Count],
		V.[Volume] * CU.[BaseAmount] / CU.[UnitAmount] As NormalizedCount,
		V.[Time],
		V.[Value],
		V.[ExpectedSettlingDate],
		V.[Reference],
		V.[Memo],
		COALESCE(RR.[Reference], V.[RelatedReference]) AS [RelatedReference],
		V.[RelatedResourceId],
		V.[RelatedAgentAccountId],
		V.[RelatedResponsibilityCenterId],
		V.[RelatedQuantity],
		V.[RelatedMoneyAmount],
		V.[RelatedMass],
		V.[RelatedVolume],
		V.[RelatedCount],
		V.[RelatedTime],
		V.[RelatedValue]
	FROM dbo.[TransactionEntriesView] V
	CROSS JOIN IntegerList IL
	JOIN dbo.Resources R ON V.ResourceId = R.Id
	JOIN dbo.MeasurementUnits MU ON R.MassUnitId = MU.Id
	JOIN dbo.MeasurementUnits VU ON R.VolumeUnitId = VU.Id
	JOIN dbo.MeasurementUnits CU ON R.CountUnitId = CU.Id
	JOIN dbo.Resources RR ON V.RelatedResourceId = RR.Id
	WHERE V.[Frequency]		= N'Yearly'
	AND (@fromDate IS NULL OR [DocumentDate] >= @fromDate)
	AND (@toDate IS NULL OR DATEADD(YEAR, IL.I, [DocumentDate]) < @toDate)
	AND ([EndDate] IS NULL OR DATEADD(YEAR, IL.I, [DocumentDate]) < [EndDate])
	*/
;