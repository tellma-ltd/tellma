CREATE PROCEDURE [bll].[DocumentDefinitions_Validate__Save]
	@Entities [DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY,
	@DocumentDefinitionMarkupTemplates [DocumentDefinitionMarkupTemplateList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	SELECT TOP (@Top) * FROM @ValidationErrors;