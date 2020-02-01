CREATE PROCEDURE [dbo].[Test]
@WideLines WideLineList READONLY
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @ProcessedWideLines WideLineList;

	INSERT INTO @ProcessedWideLines
	SELECT * FROM @WideLines;

	UPDATE @ProcessedWideLines
	SET
		Count0 = 0,
		Count1 = 1,
		Count2 = 2
	SELECT * FROM @ProcessedWideLines;
END