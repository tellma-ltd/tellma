CREATE PROCEDURE [dal].[ChartOfAccount__Anomalies]
AS
IF  [dal].[fn_FeatureCode__IsEnabled](N'AccountNullDefinitionsIncludeAll') = 1
	SELECT DISTINCT LD.[Code], LD.TitlePlural, [AC].[Concept], [Code1], [Name1], [AD1], [RD1], [NAD1], [NRD1], [A1], [R1], [NA1], [NR1], [C1],
	[Code2], A.[Name2], [AD2], [RD2], [NAD2], [NRD2], [A2], [R2], [NA2], [NR2], [C2]
	FROM LineDefinitionEntries LDE
	JOIN dbo.LineDefinitions LD ON LD.Id = LDE.LineDefinitionId
	JOIN dbo.AccountTypes AC ON AC.[Id] = LDE.[ParentAccountTypeId]
	CROSS APPLY dal.fi_ParentAccountType__ChartAnomalies(LDE.[ParentAccountTypeId]) A
	WHERE LD.[Code] <> N'ManualLine' AND LD.Id IN (SELECT LineDefinitionId FROM dbo.DocumentDefinitionLineDefinitions)
ELSE
	SELECT DISTINCT LD.[Code], LD.TitlePlural, [AC].[Concept], [Code1], [Name1], [AD1], [RD1], [NAD1], [NRD1], [A1], [R1], [NA1], [NR1], [C1],
	[Code2], A.[Name2], [AD2], [RD2], [NAD2], [NRD2], [A2], [R2], [NA2], [NR2], [C2]
	FROM LineDefinitionEntries LDE
	Join dbo.LineDefinitions LD ON LD.Id = LDE.LineDefinitionId
	JOIN dbo.AccountTypes AC ON AC.[Id] = LDE.[ParentAccountTypeId]
	CROSS APPLY dal.fi_ParentAccountType__ChartAnomaliesCurrent(LDE.[ParentAccountTypeId]) A
	WHERE LD.[Code] <> N'ManualLine' AND LD.Id IN (SELECT LineDefinitionId FROM dbo.DocumentDefinitionLineDefinitions)