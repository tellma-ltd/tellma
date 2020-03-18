CREATE FUNCTION [dbo].[fe_Center__FirstSibling] ( -- TODO: still needed?
	@OperationId INT
)
RETURNS INT
AS
BEGIN
	RETURN 
	CASE WHEN (
		SELECT ParentId 
		FROM dbo.[Centers] 
		WHERE [Id] = @OperationId
	) IS NULL THEN (
		SELECT MIN([Id]) FROM dbo.[Centers]
		WHERE ParentId IS NULL
	)
	ELSE (
		SELECT MIN([Id]) FROM dbo.[Centers]
		WHERE ParentId = (
			SELECT ParentId 
			FROM dbo.[Centers] 
			WHERE [Id] = @OperationId
		)
	) END;
END;