CREATE FUNCTION [dal].[fn_ErrorNames_Index___Localize](
	@ErrorNames dbo.ErrorNameList READONLY,
	@ErrorIndex TINYINT
) RETURNS NVARCHAR (255)
AS
BEGIN

	RETURN (
		SELECT [ErrorName]
		FROM @ErrorNames
		WHERE [ErrorIndex] = @ErrorIndex
		AND [Language] = dal.fn_UserLanguage()
	);
END