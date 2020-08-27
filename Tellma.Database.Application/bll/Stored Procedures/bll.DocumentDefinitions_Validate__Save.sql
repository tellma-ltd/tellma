CREATE PROCEDURE [bll].[DocumentDefinitions_Validate__Save]
	@Entities [DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY,
	@DocumentDefinitionMarkupTemplates [DocumentDefinitionMarkupTemplateList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
	DECLARE @ManualJV INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');

	-- List all document definitions where no account type is common to All their line definitions
	WITH DDAccountTypes AS (
		SELECT LD.HeaderIndex, LDE.AccountTypeId, COUNT(DISTINCT LD.LineDefinitionId) AS AccountTypeOccurrences
		FROM dbo.LineDefinitionEntries LDE
		JOIN @DocumentDefinitionLineDefinitions LD ON LDE.LineDefinitionId = LD.LineDefinitionId
		WHERE LD.[Id] <> @ManualLineLD
		GROUP BY LD.HeaderIndex, LDE.AccountTypeId
	),
	DDLineDefinitions AS (
		SELECT [HeaderIndex], COUNT(LineDefinitionId) AS TabCount
		FROM @DocumentDefinitionLineDefinitions
		WHERE LineDefinitionId <> @ManualLineLD
		GROUP BY [HeaderIndex]
	),
	ConformantDD AS (
		SELECT DDAT.HeaderIndex, DDAT.AccountTypeId
		FROM DDAccountTypes DDAT
		JOIN DDLineDefinitions DDLD ON DDAT.HeaderIndex = DDLD.HeaderIndex
		WHERE DDAT.AccountTypeOccurrences = DDLD.TabCount
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].LineDefinitions',
		N'Error_TheLineDefinitionsHaveNoSharedAccountType'
	FROM @Entities
	WHERE [Index] NOT IN (SELECT [HeaderIndex] FROM ConformantDD)
	AND [Id] <> @ManualJV;

	SELECT TOP (@Top) * FROM @ValidationErrors;