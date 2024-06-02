CREATE FUNCTION bll.fn_Resource_ResourceQuantity_EntryUnit__EntryQuantity(
	@ResourceId INT,
	@ResourceQuantity DECIMAL (19, 6),
	@EntryUnitId INT
)
RETURNS DECIMAL (19, 6)
AS BEGIN
	DECLARE @ResourceUnitId INT = dal.fn_Resource__UnitId(@ResourceId);
	SET @EntryUnitId = ISNULL(@EntryUnitId, @ResourceUnitId);

	DECLARE @EntryQuantity DECIMAL (19, 6) = @ResourceQuantity * dal.fn_Unit__BaseAmount(@ResourceUnitId) / dal.fn_Unit__BaseAmount(@EntryUnitId);
	RETURN @EntryQuantity;
END
GO