CREATE PROCEDURE [dbo].[bll_IfrsDisclosureDetails_Validate__Save]
	@Entities [IfrsDisclosureDetailList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Field must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheConcept0AndValidDate1AreUsed',
		FE.[IfrsDisclosureId],
		FE.[ValidSince]
	FROM @Entities FE 
	JOIN [dbo].[IfrsDisclosureDetails] BE 
	ON FE.[IfrsDisclosureId] = BE.[IfrsDisclosureId]
	AND FE.[ValidSince] = BE.[ValidSince]
	WHERE (FE.[Id] <> BE.[Id]);

	SELECT @ValidationErrorsJson = (SELECT * FROM @ValidationErrors	FOR JSON PATH);