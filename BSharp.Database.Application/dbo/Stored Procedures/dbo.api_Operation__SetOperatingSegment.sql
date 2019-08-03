CREATE PROCEDURE [dbo].[api_Operation__SetOperatingSegment]
	@OperationId INT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
DECLARE @Id INT;
-- Validate
	--EXEC [dbo].[bll_Operation_Validate__SetOperatingSegment]
	--	@Entity = @OperationId,
	--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	--IF @ValidationErrorsJson IS NOT NULL
	--	RETURN;

	SET @Id = dbo.[fe_ResponsibilityCenter__FirstSibling](@OperationId)

	-- run it recusrivsely
	EXEC [dbo].[dal_Operation__SetOperatingSegment]
			@OperationId = @Id;
END;	