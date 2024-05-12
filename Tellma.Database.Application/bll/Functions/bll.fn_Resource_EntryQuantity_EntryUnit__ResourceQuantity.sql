CREATE FUNCTION bll.fn_Resource_EntryQuantity_EntryUnit__ResourceQuantity(
	@ResourceId INT,
	@EntryQuantity DECIMAL (19, 6),
	@EntryUnitId INT
)
RETURNS DECIMAL (19, 6)
AS BEGIN
	DECLARE @ResourceUnitId INT = dal.fn_Resource__UnitId(@ResourceId);
	SET @EntryUnitId = ISNULL(@EntryUnitId, @ResourceUnitId);

	DECLARE @ResourceQuantity DECIMAL (19, 6) = @EntryQuantity * dal.fn_Unit__BaseAmount(@EntryUnitId) / dal.fn_Unit__BaseAmount(@ResourceUnitId);
	RETURN @ResourceQuantity;
END
GO
