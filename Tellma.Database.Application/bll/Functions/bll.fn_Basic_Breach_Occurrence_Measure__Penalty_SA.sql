CREATE FUNCTION bll.fn_Basic_Breach_Occurrence_Measure__Penalty_SA(
	@BasicSalary DECIMAL (19, 6),
	@ResourceId INT,
	@Occurrence INT,
	@Measure DECIMAL (19, 6),
	@ProposedPenalty DECIMAL(19, 6)
)
RETURNS DECIMAL (19, 6)
AS BEGIN
	DECLARE @MyResult DECIMAL (19, 6)
	DECLARE @L1 INT, @L2 INT, @L3 INT, @L4 INT;
	DECLARE @Code1 NVARCHAR (10), @Code2 NVARCHAR (10),  @Code3 NVARCHAR (10), @Code4 NVARCHAR (10);
	SELECT @L1 = [Lookup1Id], @L2 = [Lookup2Id], @L3 = [Lookup3Id], @L4 = [Lookup4Id]
	FROM dbo.Resources
	WHERE [Id] = @ResourceId

	SELECT @Code1 = dal.fn_Lookup__Code(@L1), @Code2 = dal.fn_Lookup__Code(@L2), @Code3 = dal.fn_Lookup__Code(@L3), @Code4 = dal.fn_Lookup__Code(@L4);

	SET @MyResult =
			CASE
			WHEN @Occurrence = 1
				THEN CASE
					WHEN @Code1 LIKE N'%Daily' THEN CAST(LEFT(@Code1, LEN(@Code1) - 5) AS DECIMAL (19, 6)) * @BasicSalary / 30.0 * @Measure
					WHEN @Code1 = N'Manual' THEN @ProposedPenalty
					WHEN @Code1 = N'Custom1' THEN @BasicSalary / 30.0 * @Measure
					WHEN @Code1 = N'Custom2' THEN @BasicSalary / 30.0 * @Measure / 300
					WHEN @Code1 = N'Custom3' THEN @BasicSalary / 30.0 * @Measure / 480
					ELSE 0
				END
			WHEN @Occurrence = 2
				THEN CASE
					WHEN @Code2 LIKE N'%Daily' THEN CAST(LEFT(@Code2, LEN(@Code2) - 5) AS DECIMAL (19, 6)) * @BasicSalary / 30.0 * @Measure
					WHEN @Code2 = N'Manual' THEN @ProposedPenalty
					WHEN @Code2 = N'Custom1' THEN @BasicSalary / 30.0 * @Measure
					WHEN @Code2 = N'Custom2' THEN @BasicSalary / 30.0 * @Measure / 300
					WHEN @Code2 = N'Custom3' THEN @BasicSalary / 30.0 * @Measure / 480
					ELSE 0
				END
			WHEN @Occurrence = 3
				THEN CASE
					WHEN @Code3 LIKE N'%Daily' THEN CAST(LEFT(@Code3, LEN(@Code3) - 5) AS DECIMAL (19, 6)) * @BasicSalary / 30.0 * @Measure
					WHEN @Code3 = N'Manual' THEN @ProposedPenalty
					WHEN @Code3 = N'Custom1' THEN @BasicSalary / 30.0 * @Measure
					WHEN @Code3 = N'Custom2' THEN @BasicSalary / 30.0 * @Measure / 300
					WHEN @Code3 = N'Custom3' THEN @BasicSalary / 30.0 * @Measure / 480
					ELSE 0
				END
			WHEN @Occurrence >= 4
				THEN CASE
					WHEN @Code4 LIKE N'%Daily' THEN CAST(LEFT(@Code4, LEN(@Code4) - 5) AS DECIMAL (19, 6)) * @BasicSalary / 30.0 * @Measure
					WHEN @Code4 = N'Manual' THEN @ProposedPenalty
					WHEN @Code4 = N'Custom1' THEN @BasicSalary / 30.0 * @Measure
					WHEN @Code4 = N'Custom2' THEN @BasicSalary / 30.0 * @Measure / 300
					WHEN @Code4 = N'Custom3' THEN @BasicSalary / 30.0 * @Measure / 480
					ELSE 0
				END
			END
	Return @MyResult
END
GO